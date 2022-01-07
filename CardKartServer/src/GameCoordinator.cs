using CardKartShared.Network;
using CardKartShared.Network.Messages;
using CardKartShared;
using System;
using System.Collections.Generic;
using CardKartShared.GameState;
using CardKartShared.Util;
using System.Threading;
using System.Text;

namespace CardKartServer
{
    internal class GameCoordinator
    {
        private Client Queued;

        private Dictionary<int, GameInstance> ActiveGames = new Dictionary<int, GameInstance>();

        private int GameIDCounter;
        private Random RNGSeedGenerator = new Random();

        public void JoinQueue(Client queuer)
        {
            if (Queued == null || !Queued.IsLoggedIn)
            {
                Queued = queuer;
            }
            else
            {
                StartGame(Queued, queuer);
                Queued = null;
            }
        }

        public void HandleMessage(Client client, RawMessage choices)
        {
            var message = new GameChoiceMessage();
            message.Decode(choices);

            var gameID = message.GameID;

            if (!ActiveGames.ContainsKey(gameID))
            {
                throw new ThisShouldNeverHappen();
            }

            ActiveGames[gameID].HandleGameChoiceMessage(message, client);
        }

        private void StartGame(Client clientA, Client clientB)
        {
            var gameID = GameIDCounter++;
            var rngSeed = RNGSeedGenerator.Next();
            var newGame = new GameInstance(clientA, clientB, gameID, rngSeed);
            ActiveGames[gameID] = newGame;
            newGame.Start();
        }

        public void EndGame(int gameID)
        {
            ActiveGames.Remove(gameID);
        }
    }

    class GameInstance
    {
        public int GameID;
        public Client Player1;
        public Client Player2;

        private GameController GameController;

        private PublicSaxophone<GameChoice> Saxophone;

        public GameInstance(Client player1, Client player2, int gameID, int rngSeed)
        {
            GameID = gameID;
            Player1 = player1;
            Player2 = player2;

            var startGameMessageA = new StartGameMessage();
            startGameMessageA.GameID = gameID;
            startGameMessageA.PlayerIndex = 1;
            startGameMessageA.RNGSeed = rngSeed;

            var startGameMessageB = new StartGameMessage();
            startGameMessageB.GameID = gameID;
            startGameMessageB.PlayerIndex = 2;
            startGameMessageB.RNGSeed = rngSeed;

            Saxophone = new PublicSaxophone<GameChoice>();
            GameController = new GameController(gameID, 0, rngSeed, new ServerObserverGameSynchronizer(Saxophone));

            GameController.GameEnded += End;

            Player1.Connection.SendMessage(startGameMessageA.Encode());
            Player2.Connection.SendMessage(startGameMessageB.Encode());

            /*
            new Thread(() =>
            {
                Thread.Sleep(5000);
                GameController.EndGame();
            }).Start();
            */
        }

        public void Start()
        {
            GameController.StartGame();
        }

        private void End(int winnerIndex, GameEndedReasons reason)
        {
            var message = new GameEndedMessage();
            message.GameID = GameID;
            message.Reason = reason;
            message.WinnerIndex = winnerIndex;

            Player1.Connection.SendMessage(message);
            Player2.Connection.SendMessage(message);

            CardKartServer.GameCoordinator.EndGame(GameID);
        }

        public void HandleGameChoiceMessage(GameChoiceMessage message, Client from)
        {
            /*
            var v = from == Player1 ? 1 : 2;
            Logging.Log(LogLevel.Debug, $"{v}");
            foreach (var asd in message.Choices.Singletons)
            {
                Logging.Log(LogLevel.Debug, $"{asd.Key} = {asd.Value}");
            }
            foreach (var asd in message.Choices.Arrays)
            {
                var sb = new StringBuilder();
                foreach (var i in asd.Value)
                {
                    sb.Append(i.ToString() + ", ");
                }
                if (sb.Length >= 2) { sb.Length -= 2; } // Trim trailing ', '.
                Logging.Log(LogLevel.Debug, $"{asd.Key} = [{sb}]");
            }
            */
            Saxophone.Play(message.Choices);

            var to = OtherClient(from);
            to.Connection.SendMessage(message);
        }

        private Client OtherClient(Client client)
        {
            if (client == Player1) return Player2;
            if (client == Player2) return Player1;
            throw new ThisShouldNeverHappen();
        }
    }

    class ServerObserverGameSynchronizer : GameChoiceSynchronizer
    {
        public PublicSaxophone<GameChoice> Saxophone;

        public ServerObserverGameSynchronizer(PublicSaxophone<GameChoice> saxophone)
        {
            Saxophone = saxophone;
        }

        public GameChoice ReceiveChoice()
        {
            return Saxophone.Listen();
        }

        public void SendChoice(GameChoice choice)
        {
            throw new ThisShouldNeverHappen("Server should never send choices.");
        }
    }
}
