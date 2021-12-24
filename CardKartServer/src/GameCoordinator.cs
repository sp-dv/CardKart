using CardKartShared.GameState;
using CardKartShared.Network;
using CardKartShared.Network.Messages;
using CardKartShared.Util;
using System;

namespace CardKartServer
{
    internal class GameCoordinator
    {
        private Client Queued;
        private RunningGame ActiveGame;

        public void JoinQueue(Client queuer)
        {
            Logging.Log(LogLevel.Debug, "Queued");

            if (Queued == null)
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
            var game = ActiveGame;

            if (client == game.Player1)
            {
                game.Player2.Connection.SendMessage(choices);
            }
            else if (client == game.Player2)
            {
                game.Player1.Connection.SendMessage(choices);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void StartGame(Client clientA, Client clientB)
        {
            ActiveGame = new RunningGame();
            var gameID = 70;

            var startGameMessageA = new StartGameMessage();
            startGameMessageA.GameID = gameID;
            startGameMessageA.PlayerIndex = 1;
            ActiveGame.Player1 = clientA;
            clientA.Connection.SendMessage(startGameMessageA.Encode());

            var startGameMessageB = new StartGameMessage();
            startGameMessageB.GameID = gameID;
            startGameMessageB.PlayerIndex = 2;
            ActiveGame.Player2 = clientB;
            clientB.Connection.SendMessage(startGameMessageB.Encode());
        }
    }

    class RunningGame
    {
        public int GameID;
        public Client Player1;
        public Client Player2;
    }


}
