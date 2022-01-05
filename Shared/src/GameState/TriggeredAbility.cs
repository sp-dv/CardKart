using System;
using System.Collections.Generic;
using System.Text;

namespace CardKartShared.GameState
{
    public class TriggeredAbility : Ability
    {
        public Func<Trigger, bool> IsTriggeredBy { get; set; } = trigger => false;

        public Action<Trigger, AbilityCastingContext> SaveTriggerInfo { get; set; } = (trigger, context) => {};

    }

    public interface Trigger
    {
        
    }

    public class DamageDoneTrigger : Trigger
    {
        public Card Source { get; }
        public Token Target { get; }
        public int Amount { get; }

        public DamageDoneTrigger(Card source, Token target, int amount)
        {
            Source = source;
            Target = target;
            Amount = amount;
        }
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

    public class GameTimeTrigger : Trigger
    {
        public GameTime Time { get; }
        public Player ActivePlayer { get; }

        public GameTimeTrigger(GameTime time, Player activePlayer)
        {
            Time = time;
            ActivePlayer = activePlayer;
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
