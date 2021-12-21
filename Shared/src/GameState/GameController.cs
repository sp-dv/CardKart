using CardKartShared.Network.Messages;
using CardKartShared.Util;
using System.Threading;

namespace CardKartShared.GameState
{
    public class GameController
    {
        public int GameID { get; }
        public GameState GameState { get; private set; }

        public PublicSaxophone<GameObject> GameObjectSaxophone { get; } 
            = new PublicSaxophone<GameObject>();

        public Player Hero;
        public Player Villain;

        public GameChoiceSynchronizer GameChoiceSynchronizer { get; }

        public GameController(int gameID, int heroIndex, GameChoiceSynchronizer gameChoiceSynchronizer)
        {
            GameChoiceSynchronizer = gameChoiceSynchronizer;

            GameID = gameID;
            GameState = new GameState();

            Logging.Log(LogLevel.Debug, $"heroIndex: {heroIndex}.");

            if (heroIndex == 1)
            {
                Hero = GameState.Player1;
                Villain = GameState.Player2;
            }
            else if (heroIndex == 2)
            {
                Hero = GameState.Player2;
                Villain = GameState.Player1;
            }

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
                    CardTemplates.ArmoredZombie,
                }),
                new Deck(new[] {
                    CardTemplates.ArmoredZombie,
                    CardTemplates.AngryGoblin
                }));
        }

        private void GameLoop()
        {
            while (true)
            {
                GameState.ActivePlayer.Draw();
                Card card;
                if (GameState.ActivePlayer == Hero)
                {
                    card = (Card)GameObjectSaxophone.Listen(obj =>
                    {
                        if (!(obj is Card)) { return false; }
                        return true;
                    });
                    var gameChoice = new GameChoice();
                    gameChoice.Choices["asd"] = card.ID;
                    GameChoiceSynchronizer.SendChoice(gameChoice);
                }
                else
                {
                    var gameChoice = GameChoiceSynchronizer.ReceiveChoice();
                    var cardID = gameChoice.Choices["asd"];
                    card = (Card)GameState.GetByID(cardID);
                }
                GameState.ActivePlayer.Battlefield.Add(card);
                GameState.SwapActivePlayer();
            }
        }
    }

    public interface GameChoiceSynchronizer
    {
        void SendChoice(GameChoice choice);
        GameChoice ReceiveChoice();
    }
}
