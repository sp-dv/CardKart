using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardKartShared.GameState
{
    public class GameState
    {
        private uint IDCounter = 1;
        private List<GameObject> GameObjects = new List<GameObject>();
        private List<Card> Cards = new List<Card>();

        public Player PlayerA { get; private set; }
        public Player PlayerB { get; private set; }

        public GameState()
        {
            PlayerA = new Player();
            AddGameObject(PlayerA);

            PlayerB = new Player();
            AddGameObject(PlayerB);
        }

        public void LoadDecks(Deck deckPlayerA, Deck deckPlayerB)
        {
            PlayerA.Deck.Add(
                deckPlayerA.CardTemplates.Select(template => CreateCard(template)));

            PlayerB.Deck.Add(
                deckPlayerB.CardTemplates.Select(template => CreateCard(template)));
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
