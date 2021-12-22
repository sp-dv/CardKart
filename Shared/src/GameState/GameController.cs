using CardKartShared.Network.Messages;
using CardKartShared.Util;
using System;
using System.Linq;
using System.Threading;

namespace CardKartShared.GameState
{
    public class GameController
    {
        public int GameID { get; }
        public GameState GameState { get; }

        public Player Hero { get; }
        public Player Villain { get; }

        public Player ActivePlayer => GameState.ActivePlayer;

        public GameChoiceSynchronizer GameChoiceSynchronizer { get; }
        public ChoiceHelper ChoiceHelper { get; } = new ChoiceHelper();

        public GameController(
            int gameID, 
            int heroIndex, 
            GameChoiceSynchronizer gameChoiceSynchronizer)
        {
            GameChoiceSynchronizer = gameChoiceSynchronizer;

            GameID = gameID;
            GameState = new GameState();

            if (heroIndex == 1)
            {
                Hero = GameState.Player1;
                Villain = GameState.Player2;
            }
            else if (heroIndex == 2)
            {
                Hero = GameState.Player2;
                Villain = GameState.Player1;
            }

        }
        
        public void Start()
        {
            new Thread(() =>
            {
                GameSetup();
                GameLoop();
            }).Start();
        }

        private void GameSetup()
        {
            ChoiceHelper.ResetGUIOptions();

            GameState.LoadDecks(
                new Deck(new[] {
                    CardTemplates.AngryGoblin,
                    CardTemplates.Zap,
                    CardTemplates.ArmoredZombie,
                }),
                new Deck(new[] {
                    CardTemplates.ArmoredZombie,
                    CardTemplates.Zap,
                    CardTemplates.AngryGoblin
                }));

            GameState.Player1.MaxMana.Red = 4;
            GameState.Player1.MaxMana.Green = 4;
            GameState.Player2.MaxMana.Red = 4;
            GameState.Player2.MaxMana.Green = 4;

            GameState.ResetMana(GameState.Player1);
            GameState.ResetMana(GameState.Player2);
        }

        private void GameLoop()
        {
            while (true)
            {
                DrawAndGainManaStep();

                Priority(ActivePlayer);

                GameState.SwapActivePlayer();
            }
        }

        private void DrawAndGainManaStep()
        {
            GameState.DrawCards(ActivePlayer, 2);

            ManaColour colour;
            if (ActivePlayer == Hero)
            {
                ChoiceHelper.Text = "Choose a mana colour to gain.";
                colour = ChoiceHelper.ChooseColour(c => true);
                var choice = new GameChoice();
                choice.Singletons["_colour"] = (int)colour;
                GameChoiceSynchronizer.SendChoice(choice);
            }
            else
            {
                var choice = GameChoiceSynchronizer.ReceiveChoice();
                colour = (ManaColour)choice.Singletons["_colour"];
            }

            GameState.GainMana(ActivePlayer, colour);
            GameState.ResetMana(ActivePlayer);
        }

        private void Priority(Player castingPlayer)
        {
            AbilityCastingContext context = MakeContext();
            context.CastingPlayer = castingPlayer;

            if (castingPlayer == Hero)
            {
                // Hack for early return without helper function.
                new Action(() =>
                {
                    ChoiceHelper.Text = "Choose a card to cast";
                    ChoiceHelper.ShowPass = true;
                    var card = ChoiceHelper.ChooseCard(card =>
                    {
                        var abilities = card.GetUsableAbilities(context);
                        if (abilities.Length == 1) { return true; }

                        return false;
                    });

                    if (card == null) { return; }
                    context.Card = card;

                    var ability = card.GetUsableAbilities(context)[0];
                    if (!ability.MakeCastChoices(context)) { return; }
                    context.Ability = ability;

                })();
                
                GameChoiceSynchronizer.SendChoice(context.Choices);
            }
            else
            {
                context.Choices = GameChoiceSynchronizer.ReceiveChoice();
            }

            var gameChoice = context.Choices;

            if (context.Card != null && context.Ability != null)
            {

                var card = context.Card;
                var ability = context.Ability;

                ability.EnactCastChoices(context);

                ability.EnactResolveChoices(context);

                if (card.Type == CardTypes.Monster || 
                    card.Type == CardTypes.Relic)
                {
                    GameState.MoveCard(card, card.Owner.Battlefield);
                }
                else
                {
                    GameState.MoveCard(card, card.Owner.Graveyard);
                }
            }
        }
        
        private AbilityCastingContext MakeContext()
        {
            var context = new AbilityCastingContext();
            context.ChoiceHelper = ChoiceHelper;
            context.Choices = new GameChoice();
            context.GameState = GameState;

            return context;
        }
    }

    public interface GameChoiceSynchronizer
    {
        void SendChoice(GameChoice choice);
        GameChoice ReceiveChoice();
    }

    public class PlayerChoiceStruct
    {
        public OptionChoice OptionChoice;
        public GameObject GameObject;

        public bool IsOptionChoice => OptionChoice != OptionChoice.None;

        public PlayerChoiceStruct(GameObject gameObject)
        {
            GameObject = gameObject;
            OptionChoice = OptionChoice.None;
        }

        public PlayerChoiceStruct(OptionChoice optionChoice)
        {
            OptionChoice = optionChoice;
        }
    }

    public enum OptionChoice
    {
        None,

        Pass,
        Yes,
        No,
        Cancel,
        Ok,

        Red,
        White,
        Blue,
        Black,
        Purple,
        Green,
        Colourless,
    }

    public class ChoiceHelper
    {
        public PublicSaxophone<PlayerChoiceStruct> PlayerChoiceSaxophone { get; }
            = new PublicSaxophone<PlayerChoiceStruct>();


        // Remember to reset these in ResetGUIOptions.
        public string Text = "";
        public bool ShowPass;
        public bool ShowOk;
        public bool ShowCancel;
        public bool ShowManaChoices;

        // Ugly hack to make UI updates accessible inside abilities...
        public delegate void RequestGUIUpdateHandler();
        public event RequestGUIUpdateHandler RequestGUIUpdate;

        public ChoiceHelper()
        {
        }

        public Card ChooseCard(Func<Card, bool> filter)
        {
            RequestGUIUpdate?.Invoke();
            var choice = PlayerChoiceSaxophone.Listen(pcs =>
            {
                if (pcs.IsOptionChoice) { return true; }
                if (pcs.GameObject is Card)
                {
                    return filter(pcs.GameObject as Card);
                }
                if (pcs.GameObject is Token)
                {
                    return filter((pcs.GameObject as Token).TokenOf);
                }
                return false;
            });
            ResetGUIOptions();

            if (choice.IsOptionChoice) { return null; }
            else { return choice.GameObject as Card; }
        }

        public Token ChooseToken(Func<Token, bool> filter)
        {
            RequestGUIUpdate?.Invoke();
            var choice = PlayerChoiceSaxophone.Listen(pcs =>
            {
                if (pcs.IsOptionChoice) { return true; }
                if (pcs.GameObject is Token)
                {
                    return filter(pcs.GameObject as Token);
                }
                return false;
            });
            ResetGUIOptions();

            if (choice.IsOptionChoice) { return null; }
            else { return choice.GameObject as Token; }

        }

        public ManaColour ChooseColour(Func<ManaColour, bool> filter)
        {
            ShowManaChoices = true;
            RequestGUIUpdate?.Invoke();
            var choice = PlayerChoiceSaxophone.Listen(pcs =>
            {
            if (!pcs.IsOptionChoice) { return false; }

                if (pcs.OptionChoice == OptionChoice.Red) { return filter(ManaColour.Red); }
                else if (pcs.OptionChoice == OptionChoice.Blue) { return filter(ManaColour.Blue); }
                else if (pcs.OptionChoice == OptionChoice.Green) { return filter(ManaColour.Green); }
                else if (pcs.OptionChoice == OptionChoice.White) { return filter(ManaColour.White); }
                else if (pcs.OptionChoice == OptionChoice.Purple) { return filter(ManaColour.Purple); }
                else if (pcs.OptionChoice == OptionChoice.Black) { return filter(ManaColour.Black); }
                else if (pcs.OptionChoice == OptionChoice.Colourless) { return filter(ManaColour.Colourless); }

                // Accept any other option choices.
                return true;
            });
            ResetGUIOptions();

            switch (choice.OptionChoice)
            {
                case OptionChoice.Red: { return ManaColour.Red; }
                case OptionChoice.Blue: { return ManaColour.Blue; }
                case OptionChoice.Green: { return ManaColour.Green; }
                case OptionChoice.White: { return ManaColour.White; }
                case OptionChoice.Purple: { return ManaColour.Purple; }
                case OptionChoice.Black: { return ManaColour.Black; }
                case OptionChoice.Colourless: { return ManaColour.Colourless; }
                default: { return ManaColour.None; }
            }
        }

        public void ResetGUIOptions()
        {
            Text = "";
            ShowPass = false;
            ShowOk = false;
            ShowCancel = false;
            ShowManaChoices = false;

            RequestGUIUpdate?.Invoke();
        }
    }
}
