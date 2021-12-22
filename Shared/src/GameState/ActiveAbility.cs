using CardKartShared.Network.Messages;
using CardKartShared.Util;
using System;

namespace CardKartShared.GameState
{
    public abstract class ActiveAbility
    {
        public Card Card;

        public bool IsCastAbility { get; protected set; }

        public abstract bool Castable(AbilityCastingContext context);

        public abstract bool MakeCastChoices(AbilityCastingContext context);
        public abstract void EnactCastChoices(AbilityCastingContext context);

        public abstract void MakeResolveChoices(AbilityCastingContext context);
        public abstract void EnactResolveChoices(AbilityCastingContext context);
    }

    public class AbilityCastingContext
    {
        public GameChoice Choices;
        public ChoiceHelper ChoiceHelper;
        public GameState GameState;
        public Player CastingPlayer;
    }

    public class GenericCreatureCast : ActiveAbility
    {
        public GenericCreatureCast()
        {
            IsCastAbility = true;
        }

        public override bool Castable(AbilityCastingContext context)
        {
            if (Card.Location != PileLocation.Hand) { return false; }
            if (Card.Owner != context.CastingPlayer) { return false; }

            return true;
        }


        public override bool MakeCastChoices(AbilityCastingContext context)
        {
            return true;
        }

        public override void EnactCastChoices(AbilityCastingContext context)
        {
        }

        public override void MakeResolveChoices(AbilityCastingContext context)
        {

        }

        public override void EnactResolveChoices(AbilityCastingContext context)
        {

        }
    }
}
