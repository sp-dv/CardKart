using System;
using System.Collections.Generic;
using System.Text;

namespace CardKartShared.GameState
{
    public abstract class TriggeredAbility : Ability
    {
        public abstract bool IsTriggeredBy(Trigger trigger);

        public abstract void SaveTriggerInfo(
            Trigger trigger, 
            AbilityCastingContext context);

        public override bool IsCastable(AbilityCastingContext context)
        {
            return false;
        }
    }

    public class TestTriggeredAbility : TriggeredAbility
    {
        public override bool IsTriggeredBy(Trigger trigger)
        {
            if (trigger is DrawTrigger)
            {
                var drawTrigger = trigger as DrawTrigger;

                return true;
            }

            return false;
        }


        public override void SaveTriggerInfo(Trigger trigger, AbilityCastingContext context)
        {
            var drawTrigger = trigger as DrawTrigger;
            context.SetPlayer("player", drawTrigger.Player);
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
            var player = context.GetPlayer("player");
            var card = context.Card;
            context.GameState.DealDamage(card, player.HeroCard.Token, 1);
        }

    }

    public class Trigger
    {
        
    }

    public class MoveTrigger : Trigger
    {
        public Card Card { get; }
        public Pile From { get; }
        public Pile To { get; }

        public MoveTrigger(Card card, Pile from, Pile to)
        {
            Card = card;
            From = from;
            To = to;
        }
    }

    public class DrawTrigger : Trigger
    {
        public Player Player { get; }
        public int CardCount { get; }

        public DrawTrigger(Player player, int cardCount)
        {
            Player = player;
            CardCount = cardCount;
        }
    }
    
}
