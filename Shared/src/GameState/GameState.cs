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

        public GameState()
        {
            // An ID of 0 is invalid so just make lookups return null.
            GameObjects.Add(null);

            Player1 = new Player();
            AddGameObject(Player1);

            Player2 = new Player();
            AddGameObject(Player2);

            ActivePlayer = Player1;
            InactivePlayer = Player2;
        }

        public void LoadDecks(Deck deckPlayerA, Deck deckPlayerB)
        {
            Player1.Deck.Add(
                deckPlayerA.CardTemplates.Select(template => CreateCard(template)).ToArray());
            foreach (var card in Player1.Deck)
            {
                card.Owner = Player1;
            }


            Player2.Deck.Add(
                deckPlayerB.CardTemplates.Select(template => CreateCard(template)).ToArray());
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

        private Card CreateCard(CardTemplates template)
        {
            var card = new Card(template);
            AddGameObject(card);
            Cards.Add(card);

            return card;
        }

        private void AddGameObject(GameObject newObject)
        {
            newObject.ID = IDCounter++;
            GameObjects.Add(newObject);
        }
    }
}
