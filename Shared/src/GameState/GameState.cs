using CardKartShared.Util;
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

        private Random RNG;

        public int End { get; private set; }

        public int Turn => (End / 2) + 1;

        public IEnumerable<Token> AllTokens =>
            Player1.Battlefield
            .Concat(Player2.Battlefield)
            .Select(card => card.Token);

        public List<(Trigger, TriggeredAbility)> PendingTriggersPlayer1 { get; } =
            new List<(Trigger, TriggeredAbility)>();

        public List<(Trigger, TriggeredAbility)> PendingTriggersPlayer2 { get; } =
            new List<(Trigger, TriggeredAbility)>();

        public IEnumerable<Card> AllGraveyards => Player1.Graveyard.Concat(Player2.Graveyard);


        public GameState(int rngSeed)
        {
            RNG = new Random(rngSeed);

            // An ID of 0 is invalid so just make lookups return null.
            GameObjects.Add(null);

            Player1 = new Player();
            Player1.Index = 1;
            AddGameObject(Player1);

            Player2 = new Player();
            Player2.Index = 2;
            AddGameObject(Player2);

            Player1.Opponent = Player2;
            Player2.Opponent = Player1;

            ActivePlayer = Player1;
            InactivePlayer = Player2;
        }

        public void LoadDecks(Deck deckPlayer1, Deck deckPlayer2)
        {
            var heroCard1 = CreateCard(CardTemplates.HeroTest, Player1);
            var heroToken1 = CreateToken(heroCard1);
            Player1.HeroCard = heroCard1;

            Player1.Deck.Add(
                deckPlayer1.CardTemplates.Select(template => CreateCard(template, Player1)).ToArray());

            var heroCard2 = CreateCard(CardTemplates.HeroTest, Player2);
            heroCard2.Owner = Player2;
            var heroToken2 = CreateToken(heroCard2);
            Player2.HeroCard = heroCard2;

            Player2.Deck.Add(
                deckPlayer2.CardTemplates.Select(template => CreateCard(template, Player2)).ToArray());
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

        public void CastAbility(AbilityCastingContext context)
        {
            if (!context.IsValid)
            {
                Logging.Log(LogLevel.Warning, "Tried to cast invalid context");
                return;
            }

            CastingStack.Push(context);
            if (context.Ability.MoveToStackOnCast)
            {
                MoveCard(context.Card, context.Card.Owner.Stack);
            }

            Trigger(new AbilityCastTrigger(context));
        }

        public void ResolveAbility(AbilityCastingContext context)
        {
            var ability = context.Ability;
            var card = context.Card;

            ability.Resolve(context);

            if (ability.MoveToStackOnCast)
            {
                if (card.Type == CardTypes.Creature ||
                    card.Type == CardTypes.Relic)
                {
                    MoveCard(card, card.Owner.Battlefield);
                }
                else
                {
                    MoveCard(card, card.Owner.Graveyard);
                }
            }
        }

        public void ShuffleDeck(Player player)
        {
            player.Deck.Shuffle(RNG);
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

        public void GainPermanentMana(Player player, ManaColour colour)
        {
            player.MaxMana.IncrementColour(colour);
        }

        public void GainTemporaryMana(Player player, ManaColour colour, int count = 1)
        {
            player.CurrentMana.IncrementColour(colour, count);
            player.NotifyOfChange();
        }

        public void SpendMana(Player player, ManaSet spentMana)
        {
            player.CurrentMana.Subtract(spentMana);
            player.NotifyOfChange();
        }

        public void MoveCard(Card card, Pile to)
        {
            var from = card.Pile;

            if (card.Token != null)
            {
                card.Token.TokenOf = null;
                card.Token = null;
            }

            if (to.Location == PileLocation.Battlefield)
            {
                var token = CreateToken(card);
                token.SummoningSick = true;
            }

            if (card.IsTokenCard && to.Location != PileLocation.Battlefield)
            {
                // If we are moving a token card anywhere but the battlefield; destroy it by not adding
                // it to the 'to' Pile.
                from.Remove(card);
            }
            else
            {
                to.Add(card);
            }

            Trigger(new MoveTrigger(card, from, to));
        }

        public void DealDamage(Card source, Token target, int amount)
        {
            if (amount <= 0) { return; }

            if (target.KeywordAbilities[KeywordAbilityNames.Protected])
            {
                target.KeywordAbilities[KeywordAbilityNames.Protected] = false;
                return;
            }

            target.DamageTaken += amount;

            if (target.TokenOf.IsHero)
            {
                target.TokenOf.Owner.NotifyOfChange();
            }

            Trigger(new DamageDoneTrigger(source, target, amount));
        }

        public void RestoreHealth(Card source, Token target, int amount)
        {
            if (amount <= 0) { return; }

            target.DamageTaken -= amount;
            if (target.DamageTaken < 0) { target.DamageTaken = 0; }

            if (target.TokenOf.IsHero)
            {
                target.TokenOf.Owner.NotifyOfChange();
            }
        }

        public void SummonToken(CardTemplates template, Player controller)
        {
            var card = CreateCard(template, controller);
            if (!card.IsTokenCard)
            {
                Logging.Log(LogLevel.Warning, "Tried to summon a token of non-token card.");
                return;
            }

            MoveCard(card, controller.Battlefield);
        }

        public void Counterspell(Card source, AbilityCastingContext counterspelledContext)
        {
            CastingStack.Remove(counterspelledContext);
            if (counterspelledContext.Ability.MoveToStackOnCast)
            {
                MoveCard(counterspelledContext.Card, counterspelledContext.Card.Owner.Graveyard);
            }
        }

        public void ExhaustToken(Token exhaustee)
        {
            exhaustee.Exhausted = true;
        }

        public void StunToken(Token stunee)
        {
            stunee.Exhausted = true;
            stunee.Stunned = true;
        }

        public void SetTime(GameTime time)
        {
            if (time == GameTime.StartOfTurn) { End++; }
            Trigger(new GameTimeTrigger(time, ActivePlayer));
        }

        #endregion

        private Card CreateCard(CardTemplates template, Player owner)
        {
            var card = new Card(template);
            AddGameObject(card);
            Cards.Add(card);
            card.Owner = owner;

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
            var context = Peek();
            Contexts.RemoveAt(Contexts.Count - 1);

            StackChanged?.Invoke(Contexts.ToArray());

            return context;
        }

        public AbilityCastingContext Peek()
        {
            return Contexts[Contexts.Count - 1];
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
