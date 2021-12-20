using System;
using System.Threading;

namespace CardKartShared.GameState
{
    public class GameController
    {
        public GameState GameState { get; private set; }

        public PublicSaxophone<GameObject> GameObjectSaxophone { get; } 
            = new PublicSaxophone<GameObject>();

        public GameController()
        {
            GameState = new GameState();
        }

        public void Start()
        {
            new Thread(() =>
            {
                GameSetup();
                GameLoop();
            }).Start();
        }

        private void GameSetup()
        {
            GameState.LoadDecks(
                new Deck(new[] {
                    CardTemplates.AngryGoblin,
                    CardTemplates.AngryGoblin,
                }),
                new Deck(new[] {
                    CardTemplates.AngryGoblin }));

            GameState.PlayerA.Draw();
            GameState.PlayerA.Draw();
        }

        private void GameLoop()
        {

            while (true)
            {
                var v = GameObjectSaxophone.Listen();
                int i = 4;
            }
        }
    }
}
