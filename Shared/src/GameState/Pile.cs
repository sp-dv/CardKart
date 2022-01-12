using CardKartShared.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardKartShared.GameState
{
    public class Pile : IEnumerable<Card>
    {
        private List<Card> Cards = new List<Card>();

        public Card this[int index]
        {
            get => Cards[index];
        }

        public int Count => Cards.Count;
        
        public PileLocation Location;

        public delegate void PileChangedHandler();
        public event PileChangedHandler PileChanged;

        public Pile(PileLocation location)
        {
            Location = location;
        }


        /// <summary>
        /// Also removes the card from the pile (if any) the Card is
        /// already in; so no need to do it manually.
        /// </summary>
        public void Add(Card cardToAdd)
        {
            if (cardToAdd.Pile == this) { return; }

            if (cardToAdd.Pile != null)
            {
                cardToAdd.Pile.Remove(cardToAdd);
            }

            cardToAdd.Pile = this;
            Cards.Add(cardToAdd);
            PileChanged?.Invoke();
        }

        public void Add(IEnumerable<Card> cardsToAdd)
        {
            foreach (var cardToAdd in cardsToAdd)
            {
                if (cardToAdd.Pile == this) { return; }

                if (cardToAdd.Pile != null)
                {
                    var removeFrom = cardToAdd.Pile;
                    removeFrom.Cards.Remove(cardToAdd);
                    removeFrom.PileChanged?.Invoke();
                }

                cardToAdd.Pile = this;
            }

            Cards.AddRange(cardsToAdd);
            PileChanged?.Invoke();
        }

        public bool Remove(Card card)
        {
            var rt = Cards.Remove(card);
            PileChanged?.Invoke();
            return rt;
        }

        public IEnumerable<Card> Peek(int count)
        {
            return Cards.Reverse<Card>().Take(count).ToArray();
        }

        public Card TopCard()
        {
            if (Cards.Count == 0) { return null; }
            return Cards.Last();
        }

        public void MoveToBottom(Card card)
        {
            if (!Cards.Remove(card))
            {
                Logging.Log(LogLevel.Warning, "Tried to move a card to the bottom of a pile it is not in.");
                return;
            }
            Cards.Insert(0, card);
        }

        public void Shuffle(Random RNG)
        {
            int n = Cards.Count;
            while (n > 1)
            {
                n--;
                int k = RNG.Next(n + 1);
                Card value = Cards[k];
                Cards[k] = Cards[n];
                Cards[n] = value;
            }
        }

        public IEnumerator<Card> GetEnumerator()
        {
            return Cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Cards.GetEnumerator();
        }
    }

    public enum PileLocation
    {
        None,

        Hand,
        Battlefield,
        Graveyard,
        Deck,
        Stack,
        Banished,

    }
}
