using CardKartShared.Network.Messages;
using System.Linq;
using System.Text;

namespace CardKartShared.GameState
{
    public abstract class Ability
    {
        public Card Card;

        public bool MoveToStackOnCast { get; protected set; }

        public abstract bool IsCastable(AbilityCastingContext context);

        public abstract bool MakeCastChoices(AbilityCastingContext context);
        public abstract void EnactCastChoices(AbilityCastingContext context);

        public abstract void MakeResolveChoicesCastingPlayer(AbilityCastingContext context);
        public abstract void EnactResolveChoices(AbilityCastingContext context);

        protected ManaSet PayMana(ManaSet cost, AbilityCastingContext context)
        {
            if (cost.Colourless == 0)
            {
                return new ManaSet(cost);
            }

            if (cost.Size == context.CastingPlayer.CurrentMana.Size)
            {
                return new ManaSet(context.CastingPlayer.CurrentMana);
            }

            var pool = new ManaSet(context.CastingPlayer.CurrentMana);
            var colourlessToPay = cost.Colourless;
            var payment = new ManaSet(cost);
            payment.Colourless = 0;

            while (colourlessToPay > 0)
            {

                string paymentString;
                var paymentColours = payment.ToColourArray();
                if (paymentColours.Count == 0)
                {
                    paymentString = "Nothing yet!";
                }
                else
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var colour in paymentColours)
                    {
                        stringBuilder.Append(colour.ToString());
                        stringBuilder.Append(", ");
                    }
                    stringBuilder.Length -= 2; // Trim the trailing ", ".
                    paymentString = stringBuilder.ToString();
                }

                context.ChoiceHelper.Text =
                    $"Paying with:\n{paymentString}";
                context.ChoiceHelper.ShowCancel = true;
                var choice = context.ChoiceHelper.ChooseColour(colour =>
                {
                    var paymentCopy = new ManaSet(payment);
                    paymentCopy.IncrementColour(colour);
                    return pool.Covers(paymentCopy);
                });

                if (choice == ManaColour.None) { return null; }

                payment.IncrementColour(choice);
                colourlessToPay--;
            }

            return payment;
        }
        protected bool InHandAndOwned(AbilityCastingContext context)
        {
            if (Card.Location != PileLocation.Hand) { return false; }
            if (Card.Owner != context.CastingPlayer) { return false; }

            return true;
        }
        protected bool ChannelsAreCastable(AbilityCastingContext context)
        {
            if (context.GameState.ActivePlayer != context.CastingPlayer) { return false; }
            if (context.GameState.CastingStack.Count > 0) { return false; }

            return true;
        }

        protected bool ManaCostIsPayable(ManaSet cost, AbilityCastingContext context)
        {
            return context.CastingPlayer.CurrentMana.Covers(cost);

        }

    }

    public class AbilityCastingContext
    {
        public GameChoice Choices;
        public ChoiceHelper ChoiceHelper;
        public GameState GameState;

        #region Synced

        // Synched by putting it in Choices.

        public Player CastingPlayer
        {
            get => !HasCastingPlayer ? null : 
                GameState.GetByID(Choices.Singletons["_CastingPlayer"]) as Player;
            set => Choices.Singletons["_CastingPlayer"] = value.ID;
        }
        private bool HasCastingPlayer => 
            Choices.Singletons.ContainsKey("_CastingPlayer");

        public Card Card
        {
            get => !HasCard ? null : GameState.GetByID(Choices.Singletons["_Card"]) as Card;
            set => Choices.Singletons["_Card"] = value.ID;
        }
        private bool HasCard =>
            Choices.Singletons.ContainsKey("_Card");

        public Ability Ability
        {
            get => !HasAbility ? null : Card.Abilities[Choices.Singletons["_Ability"]];
            set => Choices.Singletons["_Ability"] = value.Card.IndexOfAbility(value);
        }
        private bool HasAbility =>
            HasCard && Choices.Singletons.ContainsKey("_Ability");

        #endregion

        
        public void SetCard(string key, Card card)
        {
            Choices.Singletons[key] = card.ID;
        }

        public Card GetCard(string key)
        {
            return GameState.GetByID(Choices.Singletons[key]) as Card;
        }

        public void SetToken(string key, Token token)
        {
            Choices.Singletons[key] = token.ID;
        }

        public Token GetToken(string key)
        {
            return GameState.GetByID(Choices.Singletons[key]) as Token;
        }

        public void SetPlayer(string key, Player player)
        {
            Choices.Singletons[key] = player.ID;
        }

        public Player GetPlayer(string key)
        {
            return GameState.GetByID(Choices.Singletons[key]) as Player;
        }

        public void SetManaSet(string key, ManaSet set)
        {
            Choices.Arrays[key] = set.ToInts();
        }

        public ManaSet GetManaSet(string key)
        {
            var set = new ManaSet();
            set.FromInts(Choices.Arrays[key]);
            return set;
        }
    }

    public class GenericCreatureCast : Ability
    {
        public GenericCreatureCast()
        {
            MoveToStackOnCast = true;
        }

        public override bool IsCastable(AbilityCastingContext context)
        {
            if (Card.Location != PileLocation.Hand) { return false; }
            if (Card.Owner != context.CastingPlayer) { return false; }
            if (!context.CastingPlayer.CurrentMana.Covers(Card.CastingCost)) { return false; }

            return true;
        }

        public override bool MakeCastChoices(AbilityCastingContext context)
        {
            var payment = PayMana(Card.CastingCost, context);
            if (payment == null) { return false; }

            context.Choices.Arrays["manacost"] = payment.ToInts();
            return true;
        }

        public override void EnactCastChoices(AbilityCastingContext context)
        {
            var payment = new ManaSet();
            payment.FromInts(context.Choices.Arrays["manacost"]);
            context.GameState.SpendMana(context.CastingPlayer, payment);
        }

        public override void MakeResolveChoicesCastingPlayer(AbilityCastingContext context)
        {
        }

        public override void EnactResolveChoices(AbilityCastingContext context)
        {
        }
    }

    public class ZapCast : Ability
    {
        public ZapCast()
        {
        }

        public override bool IsCastable(AbilityCastingContext context)
        {
            return InHandAndOwned(context) && 
                ManaCostIsPayable(Card.CastingCost, context);
        }

        public override bool MakeCastChoices(AbilityCastingContext context)
        {
            var payment = PayMana(Card.CastingCost, context);
            if (payment == null) { return false; }

            context.ChoiceHelper.Text = "Choose a target for Zap.";
            context.ChoiceHelper.ShowCancel = true;
            var target = context.ChoiceHelper.ChooseToken(token => true);
            if (target == null) { return false; }

            context.Choices.Arrays["manacost"] = payment.ToInts();
            context.SetToken("target", target);
            return true;
        }

        public override void EnactCastChoices(AbilityCastingContext context)
        {
            var payment = new ManaSet();
            payment.FromInts(context.Choices.Arrays["manacost"]);
            context.GameState.SpendMana(context.CastingPlayer, payment);
        }

        public override void MakeResolveChoicesCastingPlayer(AbilityCastingContext context)
        {
        }

        public override void EnactResolveChoices(AbilityCastingContext context)
        {
            var target = context.GetToken("target");
            context.GameState.DealDamage(Card, target, 2);
        }

    }

    public class EnlargeCast : Ability
    {
        public EnlargeCast()
        {
            MoveToStackOnCast = true;
        }

        public override bool IsCastable(AbilityCastingContext context)
        {
            return InHandAndOwned(context) &&
                ManaCostIsPayable(Card.CastingCost, context);
        }

        public override bool MakeCastChoices(AbilityCastingContext context)
        {
            var payment = PayMana(Card.CastingCost, context);
            if (payment == null) { return false; }

            context.ChoiceHelper.Text = "Choose a target for Enlarge.";
            context.ChoiceHelper.ShowCancel = true;
            var target = context.ChoiceHelper.ChooseToken(token => true);
            if (target == null) { return false; }

            context.SetManaSet("manacost", payment);
            context.SetToken("!target", target);
            return true;
        }

        public override void EnactCastChoices(AbilityCastingContext context)
        {
            context.GameState.SpendMana(
                context.CastingPlayer, 
                context.GetManaSet("manacost"));
        }


        public override void MakeResolveChoicesCastingPlayer(AbilityCastingContext context)
        {
        }

        public override void EnactResolveChoices(AbilityCastingContext context)
        {
            var target = context.GetToken("!target");
            target.Auras.Add(new EnlargeAura());
        }
    }

    public class TestCast : Ability
    {
        public override bool IsCastable(AbilityCastingContext context)
        {
            return
                InHandAndOwned(context) &&
                ChannelsAreCastable(context) &&
                ManaCostIsPayable(Card.CastingCost, context);

        }

        public override bool MakeCastChoices(AbilityCastingContext context)
        {
            var payment = PayMana(Card.CastingCost, context);
            if (payment == null) { return false; }

            context.SetManaSet("manacost", payment);
            return true;
        }

        public override void EnactCastChoices(AbilityCastingContext context)
        {
            context.GameState.SpendMana(
                context.CastingPlayer,
                context.GetManaSet("manacost"));
        }

        public override void MakeResolveChoicesCastingPlayer(AbilityCastingContext context)
        {
            var choices = context.CastingPlayer.Deck.Peek(3);
            context.ChoiceHelper.CardChoices = choices;
            context.ChoiceHelper.Text = "Choose a card to draw.";
            var choice =
                context.ChoiceHelper.ChooseCard(card => choices.Contains(card));
            context.SetCard("target", choice);
        }

        public override void EnactResolveChoices(AbilityCastingContext context)
        {
            var card = context.GetCard("target");
            context.GameState.MoveCard(card, card.Owner.Hand);
        }
    }
}
