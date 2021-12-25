using CardKartShared.Network.Messages;
using CardKartShared.Util;
using System;
using System.Collections.Generic;
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
        public Player InactivePlayer => GameState.InactivePlayer;

        public GameChoiceSynchronizer GameChoiceSynchronizer { get; }
        public ChoiceHelper ChoiceHelper { get; } = new ChoiceHelper();


        public delegate void RedrawAttackerAnimationsHandler(Token[] attackers, (Token, Token)[] defenders);
        public event RedrawAttackerAnimationsHandler RedrawAttackerAnimations;

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
                    CardTemplates.ArmoredZombie,
                    CardTemplates.DepravedBloodhound,
                    CardTemplates.StandardBearer,
                    CardTemplates.Enlarge,
                    CardTemplates.AngryGoblin,
                    CardTemplates.Test,
                    CardTemplates.Zap,
                    CardTemplates.AlterFate,


                }),
                new Deck(new[] {
                    CardTemplates.Zap,
                    CardTemplates.DepravedBloodhound,
                    CardTemplates.ArmoredZombie,
                    CardTemplates.AngryGoblin,
                }));

            GameState.ResetMana(GameState.Player1);
            GameState.ResetMana(GameState.Player2);
        }

        private void GameLoop()
        {
            while (true)
            {
                DrawStep();
                CastStep();
                CombatStep();
                CastStep();

                GameState.SetTime(GameTime.EndOfTurn);
                EnforceGameRules(true);

                GameState.SwapActivePlayer();
            }
        }

        private void DrawStep()
        {
            foreach (var card in ActivePlayer.Battlefield)
            {
                card.Token.Exhausted = false;
                card.Token.SummoningSick = false;
            }

            EnforceGameRules(true);
            GameState.DrawCards(ActivePlayer, 1);
            EnforceGameRules(true);

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
                ChoiceHelper.ShowText("Opponent is choosing mana to gain.");
                var choice = GameChoiceSynchronizer.ReceiveChoice();
                ChoiceHelper.ResetGUIOptions();

                colour = (ManaColour)choice.Singletons["_colour"];
            }

            GameState.GainMana(ActivePlayer, colour);
            GameState.ResetMana(ActivePlayer);
            
            EnforceGameRules(true);
        }

        private void CastStep()
        {
            while (true)
            {
                var castingPlayer = ActivePlayer;
                while (true)
                {
                    var context = Priority(castingPlayer);
                    if (context == null) { break; }

                    EnforceGameRules(false);

                    castingPlayer = GameState.OtherPlayer(castingPlayer);
                }

                if (GameState.CastingStack.Count == 0) { break; }
                ResolveStack();
            }
        }

        private void CombatStep()
        {
            List<Token> attackers;

            if (ActivePlayer == Hero)
            {
                attackers = new List<Token>();
                while (true)
                {
                    ChoiceHelper.Text = "Choose attackers";
                    ChoiceHelper.ShowOk = true;
                    var choice = ChoiceHelper.ChooseToken(token => 
                    token.TokenOf.Controller == Hero &&
                    token.CanAttack);

                    if (choice == null) { break; }

                    if (attackers.Contains(choice))
                    {
                        attackers.Remove(choice);
                    }
                    else
                    {
                        attackers.Add(choice);
                    }

                    RedrawAttackerAnimations(attackers.ToArray(), null);
                }
                var attackerIDs = attackers.Select(token => token.ID).ToArray();
                var attackersChoice = new GameChoice();
                attackersChoice.Arrays["_attackers"] = attackerIDs;
                GameChoiceSynchronizer.SendChoice(attackersChoice);
            }
            else
            {
                ChoiceHelper.ShowText("Opponent is attacking.");
                var attackersChoice = GameChoiceSynchronizer.ReceiveChoice();
                ChoiceHelper.ResetGUIOptions();

                var attackerIDs = attackersChoice.Arrays["_attackers"];
                attackers =
                    attackerIDs.Select(id => GameState.GetByID(id) as Token).ToList();
            }

            RedrawAttackerAnimations(attackers.ToArray(), null);
            EnforceGameRules(true);

            if (attackers.Count == 0) { return; }

            List<(Token, Token)> defenders;

            if (ActivePlayer == Hero)
            {
                ChoiceHelper.ShowText("Opponent is blocking.");
                var choice = GameChoiceSynchronizer.ReceiveChoice();
                ChoiceHelper.ResetGUIOptions();

                var blockerIDs = choice.Arrays["_blockers"];
                var blockedIDs = choice.Arrays["_blockeds"];
                if (blockerIDs.Length != blockedIDs.Length) { throw new NotImplementedException(); }

                defenders = new List<(Token, Token)>();
                for (int i = 0; i < blockerIDs.Length; i++)
                {
                    var blocker = GameState.GetByID(blockerIDs[i]) as Token;
                    var blocked = GameState.GetByID(blockedIDs[i]) as Token;
                    defenders.Add((blocker, blocked));
                }
            }
            else
            {
                defenders = new List<(Token, Token)>();
                while (true)
                {
                    ChoiceHelper.Text = "Choose a defender.";
                    ChoiceHelper.ShowOk = true;
                    var defender = ChoiceHelper.ChooseToken(token => 
                    token.TokenOf.Controller == Hero &&
                    token.CanBlock);

                    if (defender == null) { break; }

                    // Find the defender in the list
                    int ix = -1;
                    for (int i = 0; i < defenders.Count; i++)
                    {
                        if (defenders[i].Item1 == defender)
                        {
                            ix = i;
                            break;
                        }
                    }

                    if (ix != -1)
                    {
                        defenders.RemoveAt(ix);
                        RedrawAttackerAnimations(
                            attackers.ToArray(),
                            defenders.ToArray());
                        continue;
                    }

                    ChoiceHelper.Text = "Choose a defender.";
                    ChoiceHelper.ShowCancel = true;
                    var blocked = ChoiceHelper.ChooseToken(
                        token => token.TokenOf.Controller == Villain &&
                        defender.CanBlockToken(token));

                    if (blocked == null) { continue; }

                    defenders.Add((defender, blocked));
                    RedrawAttackerAnimations(
                        attackers.ToArray(),
                        defenders.ToArray());
                }

                var blockerIDs =
                    defenders.Select(pair => pair.Item1.ID).ToArray();
                var blockedIDs =
                    defenders.Select(pair => pair.Item2.ID).ToArray();

                var choice = new GameChoice();
                choice.Arrays["_blockers"] = blockerIDs;
                choice.Arrays["_blockeds"] = blockedIDs;
                GameChoiceSynchronizer.SendChoice(choice);
            }

            RedrawAttackerAnimations(attackers.ToArray(), defenders.ToArray());
            EnforceGameRules(true);

            var unblockedAttackers = attackers.ToList();

            foreach (var pair in defenders)
            {
                var blocker = pair.Item1;
                var blocked = pair.Item2;

                if (unblockedAttackers.Contains(blocked))
                {
                    unblockedAttackers.Remove(blocked);
                }

                GameState.DealDamage(blocker.TokenOf, blocked, blocker.Attack);
                GameState.DealDamage(blocked.TokenOf, blocker, blocked.Attack);
            }

            foreach (var unblocked in unblockedAttackers)
            {
                GameState.DealDamage(
                    unblocked.TokenOf,
                    InactivePlayer.HeroCard.Token,
                    unblocked.Attack);
            }

            Thread.Sleep(1000);

            EnforceGameRules(true);

            RedrawAttackerAnimations(null, null);
        }

        private AbilityCastingContext Priority(Player castingPlayer)
        {
            AbilityCastingContext context = MakeContext();
            context.CastingPlayer = castingPlayer;

            if (castingPlayer == Hero)
            {
                // Hack for early return without helper function.
                new Action(() =>
                {
                    while (true)
                    {
                        ChoiceHelper.Text = "Choose a card to cast.";
                        ChoiceHelper.ShowPass = true;
                        var card = ChoiceHelper.ChooseCard(card =>
                        {
                            var abilities = card.GetUsableAbilities(context);
                            if (abilities.Length == 1) { return true; }

                            return false;
                        });
                        if (card == null) { return; }

                        var ability = card.GetUsableAbilities(context)[0];
                        if (!ability.MakeCastChoices(context)) { continue; }

                        context.Card = card;
                        context.Ability = ability;
                        return;
                    }
                })();

                GameChoiceSynchronizer.SendChoice(context.Choices);
            }
            else
            {
                ChoiceHelper.ShowText("Opponent is casting.");
                context.Choices = GameChoiceSynchronizer.ReceiveChoice();
                ChoiceHelper.ResetGUIOptions();
            }

            var gameChoice = context.Choices;

            if (context.Card != null && context.Ability != null)
            {
                var ability = context.Ability;
                var card = context.Card;

                ability.EnactCastChoices(context);

                GameState.CastingStack.Push(context);
                if (ability.MoveToStackOnCast)
                {
                    GameState.MoveCard(card, card.Owner.Stack);
                }

                return context;
            }
            else
            {
                return null;
            }
        }

        private void ResolveStack()
        {
            while (GameState.CastingStack.Count > 0)
            {
                Thread.Sleep(1000);

                var context = GameState.CastingStack.Pop();

                var card = context.Card;
                var ability = context.Ability;

                if (context.CastingPlayer == Hero)
                {
                    ability.MakeResolveChoicesCastingPlayer(context);
                    GameChoiceSynchronizer.SendChoice(context.Choices);

                    context.Choices = GameChoiceSynchronizer.ReceiveChoice();
                }
                else
                {
                    context.Choices = GameChoiceSynchronizer.ReceiveChoice();

                    ability.MakeResolveChoicesNonCastingPlayer(context);
                    GameChoiceSynchronizer.SendChoice(context.Choices);
                }

                ability.EnactResolveChoices(context);

                if (ability.MoveToStackOnCast)
                {
                    if (card.Type == CardTypes.Creature ||
                        card.Type == CardTypes.Relic)
                    {
                        GameState.MoveCard(card, card.Owner.Battlefield);
                    }
                    else
                    {
                        GameState.MoveCard(card, card.Owner.Graveyard);
                    }
                }

                EnforceGameRules(false);
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

        private void EnforceGameRules(bool resolveStack)
        {
            foreach (var token in GameState.AllTokens)
            {
                token.AuraModifiers.Reset();
            }

            foreach (var token in GameState.AllTokens)
            {
                foreach (var aura in token.Auras)
                {
                    aura.ApplyAura(token, GameState);
                }
            }

            var deadCards = new List<Card>();

            foreach (var card in
                GameState.Player1.Battlefield.Concat(GameState.Player2.Battlefield))
            {
                if (card.Token.CurrentHealth <= 0)
                {
                    deadCards.Add(card);
                }
            }

            foreach (var dying in deadCards)
            {
                GameState.MoveCard(dying, dying.Owner.Graveyard);
            }


            var pendingActive =
                GameState.Player1 == ActivePlayer ?
                GameState.PendingTriggersPlayer1 :
                GameState.PendingTriggersPlayer2;

            var pendingInactive =
                GameState.Player1 == InactivePlayer ?
                GameState.PendingTriggersPlayer1 :
                GameState.PendingTriggersPlayer2;

            var pendingContexts = new List<AbilityCastingContext>();


            foreach (var pending in pendingActive)
            {
                var trigger = pending.Item1;
                var ability = pending.Item2;

                var context = MakeContext();
                context.CastingPlayer = ActivePlayer;
                context.Card = ability.Card;
                context.Ability = ability;
                ability.SaveTriggerInfo(trigger, context);
                pendingContexts.Add(context);
            }
            pendingActive.Clear();


            foreach (var pending in pendingInactive)
            {
                var trigger = pending.Item1;
                var ability = pending.Item2;

                var context = MakeContext();
                context.CastingPlayer = InactivePlayer;
                context.Card = ability.Card;
                context.Ability = ability;
                ability.SaveTriggerInfo(trigger, context);
                pendingContexts.Add(context);
            }
            pendingInactive.Clear();

            foreach (var context in pendingContexts)
            {
                var ability = context.Ability;
                var castingPlayer = context.CastingPlayer;

                if (castingPlayer == Hero)
                {
                    ability.MakeCastChoices(context);
                    GameChoiceSynchronizer.SendChoice(context.Choices);
                }
                else
                {
                    context.Choices = GameChoiceSynchronizer.ReceiveChoice();
                }

                GameState.CastingStack.Push(context);
            }

            if (resolveStack)
            {
                ResolveStack();
            }
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

    public class CastingStack
    {
        private Stack<AbilityCastingContext> Contexts { get; } = new Stack<AbilityCastingContext>();
        public delegate void StackChangedHandler(Card[] cards);
        public event StackChangedHandler StackChanged;

        public int Count => Contexts.Count;

        public void Push(AbilityCastingContext context)
        {
            Contexts.Push(context);
            StackChanged?.Invoke(
                Contexts.Select(context => context.Card).ToArray());

        }

        public AbilityCastingContext Pop()
        {
            var context = Contexts.Pop();

            StackChanged?.Invoke(
                Contexts.Select(context => context.Card).ToArray());
            
            return context;
        }
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

        public IEnumerable<Card> CardChoices;

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
                if (pcs.GameObject is Player)
                {
                    return filter((pcs.GameObject as Player).HeroCard.Token);
                }
                return false;
            });
            ResetGUIOptions();

            if (choice.GameObject is Token) { return choice.GameObject as Token; }
            else if (choice.GameObject is Player) { return (choice.GameObject as Player).HeroCard.Token; }
            else { return null; }
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

        public void ShowText(string text)
        {
            Text = text;
            RequestGUIUpdate();
        }

        public void ResetGUIOptions()
        {
            Text = "";
            ShowPass = false;
            ShowOk = false;
            ShowCancel = false;
            ShowManaChoices = false;

            CardChoices = null;

            RequestGUIUpdate?.Invoke();
        }
    }

    public enum GameTime
    {
        EndOfTurn,
    }
}
