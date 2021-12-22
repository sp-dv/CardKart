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
            ChoiceHelper.ResetGUI();

            GameState.LoadDecks(
                new Deck(new[] {
                    CardTemplates.AngryGoblin,
                    CardTemplates.ArmoredZombie,
                }),
                new Deck(new[] {
                    CardTemplates.ArmoredZombie,
                    CardTemplates.AngryGoblin
                }));
        }

        private void GameLoop()
        {
            while (true)
            {
                GameState.ActivePlayer.Draw();
                GameState.ActivePlayer.MaxMana.Red++;
                GameState.ActivePlayer.ResetMana();

                Priority(GameState.ActivePlayer);

                GameState.SwapActivePlayer();
            }
        }

        private void Priority(Player castingPlayer)
        {
            AbilityCastingContext context = MakeContext(castingPlayer);
            if (castingPlayer == Hero)
            {
                PriorityInner(context);
                GameChoiceSynchronizer.SendChoice(context.Choices);
            }
            else
            {
                context.Choices = GameChoiceSynchronizer.ReceiveChoice();
            }

            var gameChoice = context.Choices;

            if (gameChoice.Singletons.ContainsKey("_card") && 
                gameChoice.Singletons.ContainsKey("_abilityIndex"))
            {

                var cardID = gameChoice.Singletons["_card"];
                var card = (Card)GameState.GetByID(cardID);
                var abilityIndex = gameChoice.Singletons["_abilityIndex"];
                var ability = card.ActiveAbilities[abilityIndex];

                ability.EnactCastChoices(context); // doesn't sync for now

                ability.EnactResolveChoices(context);

                if (card.Type == CardTypes.Monster || 
                    card.Type == CardTypes.Relic)
                {
                    card.Owner.Battlefield.Add(card);
                }
                else
                {
                    card.Owner.Graveyard.Add(card);
                }
            }
        }
        
        private void PriorityInner(AbilityCastingContext context)
        {
            ChoiceHelper.Text = "Choose a card to cast";
            ChoiceHelper.ShowPass = true;
            var card = ChoiceHelper.ChooseCardOrNull(card =>
            {
                var abilities = card.GetUsableAbilities(context);
                if (abilities.Length == 1) { return true; }

                return false;
            });

            if (card == null) { return; }

            var ability = card.GetUsableAbilities(context)[0];

            if (!ability.MakeCastChoices(context)) { return; }
            
            context.Choices.Singletons["_card"] = card.ID;
            context.Choices.Singletons["_abilityIndex"] =
                card.IndexOfActiveAbility(ability);
        }

        private AbilityCastingContext MakeContext(Player castingPlayer)
        {
            var context = new AbilityCastingContext();
            context.CastingPlayer = castingPlayer;
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

    }

    public class ChoiceHelper
    {
        public PublicSaxophone<PlayerChoiceStruct> PlayerChoiceSaxophone { get; }
            = new PublicSaxophone<PlayerChoiceStruct>();

        public string Text = "";
        public bool ShowPass;

        // Ugly hack to make UI updates accessible inside
        // abilities...
        public delegate void RequestGUIUpdateHandler();
        public event RequestGUIUpdateHandler RequestGUIUpdate;

        public ChoiceHelper()
        {
        }

        public Card ChooseCardOrNull(Func<Card, bool> filter)
        {
            RequestGUIUpdate?.Invoke();
            var choice = PlayerChoiceSaxophone.Listen(pcs =>
            {
                if (pcs.IsOptionChoice) { return true; }
                if (pcs.GameObject is Card)
                {
                    return filter(pcs.GameObject as Card);
                }
                return false;
            });
            ResetGUI();

            if (choice.IsOptionChoice) { return null; }
            else { return choice.GameObject as Card; }
        }

        public void ResetGUI()
        {
            Text = "";
            ShowPass = false;

            RequestGUIUpdate?.Invoke();
        }
    }
}
