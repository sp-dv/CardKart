using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardKartShared.GameState
{
    public class Player : GameObject
    {
        public Pile Hand { get; } = new Pile(PileLocation.Hand);
        public Pile Deck { get; } = new Pile(PileLocation.Deck);
        public Pile Battlefield { get; } = new Pile(PileLocation.Battlefield);

        public Card Draw()
        {
            if (Deck.Count == 0) { return null; }

            var drawnCard = Deck[Deck.Count - 1];
            Hand.Add(drawnCard);

            return drawnCard;
        }
    }
}
