using CardKartShared.GameState;
using CardKartShared.Network;
using CardKartShared.Network.Messages;
using CardKartShared.Util;
using System;
using System.Threading;

namespace CardKartClient
{
    internal class ServerConnection
    {
        private Connection Connection;
        private PublicSaxophone<RawMessage> RequestResponseSaxophone 
            = new PublicSaxophone<RawMessage>();
        private PublicSaxophone<GameChoiceMessage> GameChoiceMessageSaxophone
            = new PublicSaxophone<GameChoiceMessage>();

        public void Connect()
        {
            Connection = Constants.CurrentServer.Connect();

            var handshakeMessage = new HandshakeMessage();
            handshakeMessage.VersionString = Constants.Version;
            Connection.SendMessage(handshakeMessage.Encode());

            var responseRaw = Connection.ReceiveMessage();
            var response = new GenericResponseMessage();
            response.Decode(responseRaw);

            if (response.Code == GenericResponseMessage.Codes.Error)
            {
                Logging.Log(LogLevel.Error, response.Info);
            }

            new Thread(() =>
            {
                while (true)
                {
                    HandleIncomingMessage(Connection.ReceiveMessage());
                }
            }).Start();
        }

        public void JoinQueue()
        {
            var joinQueueRequest = new JoinQueueRequest();
            Connection.SendMessage(joinQueueRequest.Encode());

            var response = WaitForGenericResponse();
            if (response.Code != GenericResponseMessage.Codes.OK)
            {
                throw new NotImplementedException();
            }
        }

        public ClientSideGameSynchronizer CreateGameChoiceSynchronizer(int gameID)
        {
            return new ClientSideGameSynchronizer(gameID, GameChoiceMessageSaxophone);
        }

        public void SendGameChoice(int gameID, GameChoice choice)
        {
            var gameChoiceMessage = new GameChoiceMessage();
            gameChoiceMessage.GameID = gameID;
            gameChoiceMessage.Choices = choice;
            Connection.SendMessage(gameChoiceMessage.Encode());
        }

        private GenericResponseMessage WaitForGenericResponse()
        {
            var rawResponse = RequestResponseSaxophone.Listen();
            if (rawResponse.MessageType != MessageTypes.GenericResponse)
            {
                throw new NotImplementedException();
            }

            var response = new GenericResponseMessage();
            response.Decode(rawResponse);
            return response;
        }

        private void HandleIncomingMessage(RawMessage rawMessage)
        {
            switch (rawMessage.MessageType)
            {
                case MessageTypes.StartGameMessage:
                    {
                        var startGameMessage = new StartGameMessage();
                        startGameMessage.Decode(rawMessage);
                        CardKartClient.Controller.StartGame(
                            startGameMessage.GameID, 
                            startGameMessage.PlayerIndex);
                    }break;

                case MessageTypes.GenericResponse:
                    {
                        RequestResponseSaxophone.Play(rawMessage);
                    } break;

                case MessageTypes.GameChoiceMessage:
                    {
                        var gameChoiceMessage = new GameChoiceMessage();
                        gameChoiceMessage.Decode(rawMessage);
                        GameChoiceMessageSaxophone.Play(gameChoiceMessage);
                    } break;

                default:
                    {
                        Logging.Log(
                            LogLevel.Warning, 
                            $"Unhandled message type {rawMessage.MessageType}");
                    } break;
            }
        }
    }

    internal class ClientSideGameSynchronizer : GameChoiceSynchronizer
    {
        public int GameID;
        public PublicSaxophone<GameChoiceMessage> Saxophone;

        public ClientSideGameSynchronizer(
            int gameID, 
            PublicSaxophone<GameChoiceMessage> saxophone)
        {
            GameID = gameID;
            this.Saxophone = saxophone;
        }

        public GameChoice ReceiveChoice()
        {
            while (true)
            {
                var choiceMessage = Saxophone.Listen();
                if (choiceMessage.GameID == GameID)
                {
                    return choiceMessage.Choices;
                }
            }
        }

        public void SendChoice(GameChoice choice)
        {
            CardKartClient.Server.SendGameChoice(GameID, choice);
        }
    }
}
