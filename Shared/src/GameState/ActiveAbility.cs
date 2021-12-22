using CardKartShared.Network.Messages;
using CardKartShared.Util;
using System;
using System.Linq;
using System.Text;

namespace CardKartShared.GameState
{
    public abstract class ActiveAbility
    {
        public Card Card;

        public bool MoveToStackOnCast { get; protected set; }

        public abstract bool Castable(AbilityCastingContext context);

        public abstract bool MakeCastChoices(AbilityCastingContext context);
        public abstract void EnactCastChoices(AbilityCastingContext context);

        public abstract void MakeResolveChoices(AbilityCastingContext context);
        public abstract void EnactResolveChoices(AbilityCastingContext context);


        #region Common helper functions

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

        protected Token GetToken(AbilityCastingContext context, string key)
        {
            var tokenID = context.Choices.Singletons[key];
            return context.GameState.GetByID(tokenID) as Token;
        }

        #endregion
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

        public ActiveAbility Ability
        {
            get => !HasAbility ? null : Card.ActiveAbilities[Choices.Singletons["_Ability"]];
            set => Choices.Singletons["_Ability"] = value.Card.IndexOfActiveAbility(value);
        }
        private bool HasAbility =>
            HasCard && Choices.Singletons.ContainsKey("_Ability");

        #endregion
    }

    #region Reusable Abilities

    public class GenericCreatureCast : ActiveAbility
    {
        public GenericCreatureCast()
        {
            MoveToStackOnCast = true;
        }

        public override bool Castable(AbilityCastingContext context)
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

        public override void MakeResolveChoices(AbilityCastingContext context)
        {
        }

        public override void EnactResolveChoices(AbilityCastingContext context)
        {
        }
    }

    #endregion

    public class ZapCast : ActiveAbility
    {
        public ZapCast()
        {
        }

        public override bool Castable(AbilityCastingContext context)
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

            context.ChoiceHelper.Text = "Choose a target for Zap.";
            context.ChoiceHelper.ShowCancel = true;
            var target = context.ChoiceHelper.ChooseToken(token => true);
            if (target == null) { return false; }

            context.Choices.Arrays["manacost"] = payment.ToInts();
            context.Choices.Singletons["target"] = target.ID;
            return true;
        }

        public override void EnactCastChoices(AbilityCastingContext context)
        {
            var payment = new ManaSet();
            payment.FromInts(context.Choices.Arrays["manacost"]);
            context.GameState.SpendMana(context.CastingPlayer, payment);
        }

        public override void MakeResolveChoices(AbilityCastingContext context)
        {
        }

        public override void EnactResolveChoices(AbilityCastingContext context)
        {
            var target = GetToken(context, "target");
            context.GameState.DealDamage(Card, target, 2);
        }

    }
}
