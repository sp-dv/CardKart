using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardKartShared.GameState
{
    public class GameState
    {
        private int IDCounter = 1;
        private List<GameObject> GameObjects = new List<GameObject>();
        private List<Card> Cards = new List<Card>();

        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public Player ActivePlayer { get; private set; }
        public Player InactivePlayer { get; private set; }

        public CastingStack CastingStack { get; } = new CastingStack();

        public IEnumerable<Token> AllTokens =>
            Player1.Battlefield
            .Concat(Player2.Battlefield)
            .Select(card => card.Token);

        public List<(Trigger, TriggeredAbility)> PendingTriggersPlayer1 { get; } =
            new List<(Trigger, TriggeredAbility)>();

        public List<(Trigger, TriggeredAbility)> PendingTriggersPlayer2 { get; } =
            new List<(Trigger, TriggeredAbility)>();

        public GameState()
        {
            // An ID of 0 is invalid so just make lookups return null.
            GameObjects.Add(null);

            Player1 = new Player();
            AddGameObject(Player1);

            Player2 = new Player();
            AddGameObject(Player2);

            Player1.Opponent = Player2;
            Player2.Opponent = Player1;

            ActivePlayer = Player1;
            InactivePlayer = Player2;
        }

        public void LoadDecks(Deck deckPlayer1, Deck deckPlayer2)
        {
            var heroCard1 = CreateCard(CardTemplates.HeroTest);
            heroCard1.Owner = Player1;
            var heroToken1 = CreateToken(heroCard1);
            Player1.HeroCard = heroCard1;

            Player1.Deck.Add(
                deckPlayer1.CardTemplates.Select(template => CreateCard(template)).ToArray());
            foreach (var card in Player1.Deck)
            {
                card.Owner = Player1;
            }


            var heroCard2 = CreateCard(CardTemplates.HeroTest);
            heroCard2.Owner = Player2;
            var heroToken2 = CreateToken(heroCard2);
            Player2.HeroCard = heroCard2;

            Player2.Deck.Add(
                deckPlayer2.CardTemplates.Select(template => CreateCard(template)).ToArray());
            foreach (var card in Player2.Deck)
            {
                card.Owner = Player2;
            }
        }

        public GameObject GetByID(int gameID)
        {
            return GameObjects[gameID];
        }

        public void SwapActivePlayer()
        {
            var swap = ActivePlayer;
            ActivePlayer = InactivePlayer;
            InactivePlayer = swap;
        }

        public Player OtherPlayer(Player player)
        {
            if (player == Player1) { return Player2;}
            if (player == Player2) { return Player1;}
            throw new NotImplementedException();
        }

        #region Triggering 

        private void Trigger(Trigger trigger)
        {
            Action<Player, List<(Trigger, TriggeredAbility)>> addTriggers = 
                (player, list) =>
            {
                foreach (var card in player.Graveyard)
                {
                    foreach (var triggeredAbility in card.TriggeredAbilities)
                    {
                        if (triggeredAbility.IsTriggeredBy(trigger))
                        {
                            list.Add((trigger, triggeredAbility));
                        }
                    }
                }

                foreach (var card in player.Battlefield)
                {
                    foreach (var triggeredAbility in card.Token.TriggeredAbilities)
                    {
                        if (triggeredAbility.IsTriggeredBy(trigger))
                        {
                            list.Add((trigger, triggeredAbility));
                        }
                    }
                }
            };

            addTriggers(Player1, PendingTriggersPlayer1);
            addTriggers(Player2, PendingTriggersPlayer2);

            foreach (var token in AllTokens)
            {
                var cancelledAuras = new List<Aura>();
                foreach (var aura in token.Auras)
                {
                    if (aura.IsCancelledBy(trigger, token, this))
                    {
                        cancelledAuras.Add(aura);
                    }    
                }

                foreach (var cancelledAura in cancelledAuras)
                {
                    token.Auras.Remove(cancelledAura);
                }
            }
        }

        public void DrawCards(Player player, int cardCount)
        {
            Trigger(new DrawTrigger(player, cardCount));

            for (int i = 0; i < cardCount; i++)
            {
                if (player.Deck.Count == 0) { return; }
                var drawnCard = player.Deck[player.Deck.Count - 1];
                player.Hand.Add(drawnCard);
            }
        }

        public void ResetMana(Player player)
        {
            player.CurrentMana.CopyValues(player.MaxMana);
            player.NotifyOfChange();
        }

        public void GainMana(Player player, ManaColour colour)
        {
            player.MaxMana.IncrementColour(colour);
        }

        public void SpendMana(Player player, ManaSet spentMana)
        {
            player.CurrentMana.Subtract(spentMana);
            player.NotifyOfChange();
        }

        public void MoveCard(Card card, Pile to)
        {
            var from = card.Pile;

            card.Token = null;

            if (to.Location == PileLocation.Battlefield)
            {
                var token = CreateToken(card);
                token.SummoningSick = true;
            }

            to.Add(card);

            Trigger(new MoveTrigger(card, card.Pile, to));
        }

        public void DealDamage(Card source, Token target, int amount)
        {
            if (amount <= 0) { return; }

            target.DamageTaken += amount;

            if (target.TokenOf.IsHero)
            {
                target.TokenOf.Owner.NotifyOfChange();
            }    
        }

        public void Counterspell(Card source, AbilityCastingContext counterspelledContext)
        {
            CastingStack.Remove(counterspelledContext);
            if (counterspelledContext.Ability.MoveToStackOnCast)
            {
                MoveCard(counterspelledContext.Card, counterspelledContext.Card.Owner.Graveyard);
            }
        }

        public void SetTime(GameTime time)
        {
            Trigger(new GameTimeTrigger(time, ActivePlayer));
        }

        #endregion

        private Card CreateCard(CardTemplates template)
        {
            var card = new Card(template);
            AddGameObject(card);
            Cards.Add(card);

            return card;
        }

        private Token CreateToken(Card card)
        {
            var token = new Token(card);
            card.Token = token;
            AddGameObject(token);

            return token;
        }

        private void AddGameObject(GameObject newObject)
        {
            newObject.ID = IDCounter++;
            GameObjects.Add(newObject);
        }
    }

    public class CastingStack
    {
        private List<AbilityCastingContext> Contexts { get; } = new List<AbilityCastingContext>();
        public delegate void StackChangedHandler(AbilityCastingContext[] contexts);
        public event StackChangedHandler StackChanged;

        public int Count => Contexts.Count;

        // We need a way to reference things on the stack from abilities (e.g. for counterspell)
        // Since the stack might change this can't simply be indexes into the stack. Therefor generate
        // an ID every time a spell is added to the stack.
        private int IDCounter;
        private Dictionary<int, AbilityCastingContext> ContextDictionary = new Dictionary<int, AbilityCastingContext>();

        public void Push(AbilityCastingContext context)
        {
            Contexts.Add(context);
            ContextDictionary[IDCounter++] = context;
            StackChanged?.Invoke(Contexts.ToArray());
        }

        public AbilityCastingContext Pop()
        {
            var context = Contexts[Contexts.Count - 1];
            Contexts.RemoveAt(Contexts.Count - 1);

            StackChanged?.Invoke(Contexts.ToArray());

            return context;
        }

        public bool Remove(AbilityCastingContext context)
        {
            var rt = Contexts.Remove(context);
            StackChanged?.Invoke(Contexts.ToArray());
            return rt;
        }

        public List<int> GetTargetIDs(int index)
        {
            var context = Contexts.ElementAt(index);
            var targets = new List<int>();

            foreach (var kvp in context.Choices.Singletons)
            {
                if (kvp.Key[0] == '!')
                {
                    targets.Add(kvp.Value);
                }
            }
            foreach (var kvp in context.Choices.Arrays)
            {
                if (kvp.Key[0] == '!')
                {
                    targets.AddRange(kvp.Value);
                }
            }

            return targets;
        }

        public int IndexOf(AbilityCastingContext context)
        {
            return ContextDictionary.First(kvp => kvp.Value == context).Key;
        }

        public AbilityCastingContext GetAtIndex(int i)
        {
            return ContextDictionary[i];
        }
    }
}
