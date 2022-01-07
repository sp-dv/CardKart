using CardKartShared.Network.Messages;
using CardKartShared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        // Nasty hack to let decks be chosen after the game has started.
        public Func<Deck> LoadDeckDelegate { get; set; }

        public delegate void RedrawAttackerAnimationsHandler(Token[] attackers, (Token, Token)[] defenders);
        public event RedrawAttackerAnimationsHandler RedrawAttackerAnimations;

        public delegate void GameEndedHandler(int winnerIndex, GameEndedReasons reason);
        public event GameEndedHandler GameEnded;

        private Thread LoopThread;
        private bool GameHasEnded;
        private object GameEndLock = new object();

        public GameController(
            int gameID,
            int heroIndex,
            int rngSeed,
            GameChoiceSynchronizer gameChoiceSynchronizer)
        {
            GameChoiceSynchronizer = gameChoiceSynchronizer;

            GameID = gameID;
            GameState = new GameState(rngSeed);

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

            LoopThread = new Thread(() =>
            {
                try
                {
                    GameLoop();
                }
                catch (ThreadInterruptedException ex)
                {
                    Logging.Log(LogLevel.Debug, "LoopThread caught exception");
                }
                
                Logging.Log(LogLevel.Debug, "LoopThread terminated");
            });
        }

        public void StartGame()
        {
            LoopThread.Start();
        }

        public void EndGame(Player winner, GameEndedReasons reason)
        {
            lock (GameEndLock)
            {
                if (GameHasEnded) { return; }
            
                GameHasEnded = true;
                LoopThread.Interrupt();
                LoopThread = null;
                
                GameEnded?.Invoke(winner.Index, reason);
                Logging.Log(LogLevel.Debug, "ended game");
            }
        }

        private void GameSetup()
        {
            ChoiceHelper.ResetGUIOptions();

            Deck player1Deck;
            Deck player2Deck;

            if (Hero == GameState.Player1)
            {
                player1Deck = LoadDeckDelegate();
                var choices = new GameChoice();
                choices.Arrays["deck"] = player1Deck.CardTemplates.Select(template => (int)template).ToArray();
                GameChoiceSynchronizer.SendChoice(choices);
            }
            else
            {
                var player1Choices = GameChoiceSynchronizer.ReceiveChoice();
                var player1Templates = player1Choices.Arrays["deck"].Select(i => (CardTemplates)i).ToArray();
                player1Deck = new Deck(player1Templates);
            }

            if (Hero == GameState.Player2)
            {
                player2Deck = LoadDeckDelegate();
                var choices = new GameChoice();
                choices.Arrays["deck"] = player2Deck.CardTemplates.Select(template => (int)template).ToArray();
                GameChoiceSynchronizer.SendChoice(choices);
            }
            else
            {
                var player2Choices = GameChoiceSynchronizer.ReceiveChoice();
                var player2Templates = player2Choices.Arrays["deck"].Select(i => (CardTemplates)i).ToArray();
                player2Deck = new Deck(player2Templates);
            }

            GameState.LoadDecks(player1Deck, player2Deck);
            GameState.ShuffleDeck(GameState.Player1);
            GameState.ShuffleDeck(GameState.Player2);

            GameState.ResetMana(GameState.Player1);
            GameState.ResetMana(GameState.Player2);

            GameState.DrawCards(GameState.Player1, 4);
            GameState.DrawCards(GameState.Player2, 4);
        }

        private void GameLoop()
        {
            GameSetup();

            while (true)
            {
                GameState.SetTime(GameTime.StartOfTurn);
                EnforceGameRules(true);

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

            if (GameState.End != 0)
            {
                GameState.DrawCards(ActivePlayer, 1);
                EnforceGameRules(true);
            }

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

            GameState.GainPermanentMana(ActivePlayer, colour);
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

                    RedrawAttackerAnimations?.Invoke(attackers.ToArray(), null);
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
            
            RedrawAttackerAnimations?.Invoke(attackers.ToArray(), null);
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
                    ChoiceHelper.Text = "Choose a blocker.";
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

                    ChoiceHelper.Text = "Choose a creature to block.";
                    ChoiceHelper.ShowCancel = true;
                    var blocked = ChoiceHelper.ChooseToken(token => 
                        attackers.Contains(token) && 
                        token.TokenOf.Controller == Villain &&
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

            foreach (var attacker in attackers)
            {
                if (!attacker.HasKeywordAbility(KeywordAbilityNames.Vigilance))
                {
                    attacker.Exhausted = true;
                }
            }

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
                            if (card == null) { return false; } // This happened somehow.

                            var abilities = card.GetUsableAbilities(context);
                            if (abilities.Length > 0) { return true; }

                            return false;
                        });
                        if (card == null) { return; }

                        var usableAbilities = card.GetUsableAbilities(context);
                        Ability castAbility;
                        
                        if (usableAbilities.Length == 0) { return; } // This should never happen and if it does just pretend the user passed?
                        if (usableAbilities.Length == 1) { castAbility = usableAbilities[0]; }
                        else
                        {
                            ChoiceHelper.Text = "Choose which ability to cast.";
                            ChoiceHelper.AbilityChoices = usableAbilities;
                            ChoiceHelper.ShowCancel = true;
                            var choice = ChoiceHelper.ChooseAbility();
                            if (choice == null) { continue; }
                            castAbility = choice;
                        }

                        if (!castAbility.MakeCastChoices(context)) { continue; }

                        context.Card = card;
                        context.Ability = castAbility;
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

        private void showcontext(AbilityCastingContext context)
        {
            return;
            foreach (var asd in context.Choices.Singletons)
            {
                Logging.Log(LogLevel.Debug, $"{asd.Key} = {asd.Value}");
            }
            foreach (var asd in context.Choices.Arrays)
            {
                var sb = new StringBuilder();
                foreach (var i in asd.Value)
                {
                    sb.Append(i.ToString() + ", ");
                }
                if (sb.Length >= 2) { sb.Length -= 2; } // Trim trailing ', '.
                Logging.Log(LogLevel.Debug, $"{asd.Key} = [{sb}]");
            }
        }

        private void ResolveStack()
        {
            while (GameState.CastingStack.Count > 0)
            {
                var context = GameState.CastingStack.Pop();

                var card = context.Card;
                var ability = context.Ability;

                Logging.Log(LogLevel.Debug, "IN");
                showcontext(context);

                if (context.CastingPlayer == Hero)
                {
                    ability.MakeResolveChoicesCastingPlayer(context);
                    GameChoiceSynchronizer.SendChoice(context.Choices);
                    Logging.Log(LogLevel.Debug, "A");
                showcontext(context);
                }
                else
                {
                    context.Choices = GameChoiceSynchronizer.ReceiveChoice();
                    Logging.Log(LogLevel.Debug, "RA");
                    showcontext(context);

                }

                if (context.CastingPlayer == Villain)
                {
                    ability.MakeResolveChoicesNonCastingPlayer(context);
                    GameChoiceSynchronizer.SendChoice(context.Choices);
                    Logging.Log(LogLevel.Debug, "B");
                showcontext(context);
                }
                else
                {
                    context.Choices = GameChoiceSynchronizer.ReceiveChoice();
                    Logging.Log(LogLevel.Debug, "RB");
                    showcontext(context);

                }

                Logging.Log(LogLevel.Debug, "OUT");
                showcontext(context);



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
            context.Hero = Hero;

            return context;
        }

        private void EnforceGameRules(bool resolveStack)
        {
            if (GameState.Player1.CurrentHealth <= 0)
            {
                EndGame(GameState.Player2, GameEndedReasons.Health);
            }
            if (GameState.Player2.CurrentHealth <= 0)
            {
                EndGame(GameState.Player1, GameEndedReasons.Health);
            }

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
        public OptionChoice OptionChoice { get; }
        public GameObject GameObject { get; }
        public Ability AbilityChoice { get; }
        public AbilityCastingContext ContextChoice { get; }

        public bool IsOptionChoice => OptionChoice != OptionChoice.None;
        public bool IsGameObjectChoice => GameObject != null;
        public bool IsAbilityChoice => AbilityChoice != null;
        public bool IsCastinContextChoice => ContextChoice != null;

        public PlayerChoiceStruct(Ability abilityChoice)
        {
            AbilityChoice = abilityChoice;
        }

        public PlayerChoiceStruct(GameObject gameObject)
        {
            GameObject = gameObject;
            OptionChoice = OptionChoice.None;
        }

        public PlayerChoiceStruct(OptionChoice optionChoice)
        {
            OptionChoice = optionChoice;
        }

        public PlayerChoiceStruct(AbilityCastingContext contextChoice)
        {
            ContextChoice = contextChoice;
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

    

    public  class ChoiceHelper
    {
        public PrivateSaxophone<PlayerChoiceStruct> PlayerChoiceSaxophone { get; }
            = new PrivateSaxophone<PlayerChoiceStruct>();


        // Remember to reset these in ResetGUIOptions.
        public string Text = "";
        public bool ShowPass;
        public bool ShowOk;
        public bool ShowCancel;
        public bool ShowManaChoices;

        public IEnumerable<Card> CardChoices { get; private set; }
        public IEnumerable<Ability> AbilityChoices;

        // Ugly hack to make UI updates accessible inside abilities...
        public delegate void RequestGUIUpdateHandler();
        public event RequestGUIUpdateHandler RequestGUIUpdate;

        public delegate void RequestShowCardsHandler(IEnumerable<Card> cards);
        public event RequestShowCardsHandler RequestShowCards;

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
            else if (choice.GameObject is Card) { return choice.GameObject as Card; }
            else if (choice.GameObject is Token) { return (choice.GameObject as Token).TokenOf; }
            throw new NotImplementedException();
        }

        public Card ChooseCardFromOptions(IEnumerable<Card> options, Func<Card, bool> filter)
        {
            CardChoices = options.Where(option => filter(option)).ToArray();

            if (CardChoices.Count() == 0) { ShowCancel = true; }

            RequestGUIUpdate?.Invoke();
            var choice = PlayerChoiceSaxophone.Listen(pcs =>
            {
                if (pcs.IsOptionChoice) { return true; }
                if (pcs.GameObject is Card)
                {
                    var card = pcs.GameObject as Card;
                    return CardChoices.Contains(card) && (filter(card));
                }
                return false;
            });
            ResetGUIOptions();

            if (choice.IsOptionChoice) { return null; }
            else if (choice.GameObject is Card) { return choice.GameObject as Card; }
            throw new NotImplementedException();
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

        public Ability ChooseAbility()
        {
            RequestGUIUpdate?.Invoke();
            var choice = PlayerChoiceSaxophone.Listen(pcs =>
            {
                if (pcs.IsOptionChoice) { return true; }
                return pcs.IsAbilityChoice;
            });
            ResetGUIOptions();

            if (choice.IsOptionChoice) { return null; }
            return choice.AbilityChoice;
        }

        public Player ChoosePlayer()
        {
            RequestGUIUpdate?.Invoke();
            var choice = PlayerChoiceSaxophone.Listen(pcs =>
            {
                if (pcs.IsOptionChoice) { return true; }
                if (pcs.IsGameObjectChoice && pcs.GameObject is Player)
                {
                    return true;
                }
                return false;
            });
            ResetGUIOptions();

            if (choice.IsOptionChoice) { return null; }
            else { return choice.GameObject as Player; }
        }

        public AbilityCastingContext ChooseCastingContext(Func<AbilityCastingContext, bool> filter)
        {
            RequestGUIUpdate?.Invoke();
            var choice = PlayerChoiceSaxophone.Listen(pcs =>
            {
                if (pcs.IsOptionChoice) { return true; }
                return pcs.IsCastinContextChoice;
            });
            ResetGUIOptions();

            if (choice.IsOptionChoice) { return null; }
            return choice.ContextChoice;
        }

        public void ShowText(string text)
        {
            Text = text;
            RequestGUIUpdate?.Invoke();
        }

        public void ShowCards(IEnumerable<Card> cards)
        {
            RequestShowCards?.Invoke(cards);
        }

        public void ResetGUIOptions()
        {
            Text = "";
            ShowPass = false;
            ShowOk = false;
            ShowCancel = false;
            ShowManaChoices = false;

            CardChoices = null;
            AbilityChoices = null;

            RequestGUIUpdate?.Invoke();
        }
    }

    public enum GameTime
    {
        StartOfTurn,
        EndOfTurn,
    }

    public enum GameEndedReasons
    {
        Health,
        Deck,
        Surrender,
    }
}
