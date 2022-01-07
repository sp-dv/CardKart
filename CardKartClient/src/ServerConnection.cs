using CardKartShared.GameState;
using CardKartShared.Network;
using CardKartShared.Network.Messages;
using CardKartShared.Util;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace CardKartClient
{
    internal class ServerConnection
    {
        private Connection Connection;
        private PrivateSaxophone<RawMessage> RequestResponseSaxophone 
            = new PrivateSaxophone<RawMessage>();
        private PrivateSaxophone<GameChoiceMessage> GameChoiceMessageSaxophone
            = new PrivateSaxophone<GameChoiceMessage>();

        public bool Connect()
        {
            Connection = Constants.CurrentServer.Connect();
            var publicKey = Constants.CurrentServer.PublicKey;

            var magic = new Magic();
            var encryptionSuite = new EncryptionSuite(magic.AesParams);

            var handshakeMessage = new HandshakeMessage();
            handshakeMessage.VersionString = Constants.Version;
            handshakeMessage.MagicBytes = RSAEncryption.RSAEncrypt(JsonConvert.SerializeObject(magic), publicKey);
            Connection.SendMessage(handshakeMessage.Encode());

            var responseRaw = Connection.ReceiveMessage();
            var response = new HandshakeResponse();
            response.Decode(responseRaw);


            if (response.Error != null)
            {
                Logging.Log(LogLevel.Warning, $"Failed to connect to server with reason: {response.Error}");
                return false;
            }
            if (response.MagicBytes == null)
            {
                return false;
            }
            var decryptedNonce = encryptionSuite.Decrypt(response.MagicBytes);
            if (decryptedNonce.Length != magic.Nonce.Length)
            {
                return false;
            }

            for (int i = 0; i < magic.Nonce.Length; i++)
            {
                if (magic.Nonce[i] != decryptedNonce[i]) { return false; }
            }

            Logging.Log(LogLevel.Info, "Successfully connected to server.");

            Connection.EncryptionSuite = encryptionSuite;

            new Thread(() =>
            {
                while (!Connection.IsClosed)
                {
                    HandleIncomingMessage(Connection.ReceiveMessage());
                }
            }).Start();

            return true;
        }

        public GenericResponseMessage JoinQueue()
        {
            var joinQueueRequest = new JoinQueueRequest();
            Connection.SendMessage(joinQueueRequest);

            return WaitForGenericResponse();
        }

        public GenericResponseMessage LogIn(string username, string password)
        {
            var request = new LoginRequest {
                Username = username,
                Password = password
            };

            Connection.SendMessage(request);
            
            return WaitForGenericResponse();
        }

        public void SurrenderGame(int gameID)
        {
            var message = new SurrenderGameMessage();
            message.GameID = gameID;
            Connection.SendMessage(message);
        }

        #region Ugly game synch hack functions
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
        #endregion

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
                            startGameMessage.PlayerIndex,
                            startGameMessage.RNGSeed);
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

                case MessageTypes.GameEndedMessage:
                    {
                        var gameEndedMessage = new GameEndedMessage();
                        gameEndedMessage.Decode(rawMessage);
                        CardKartClient.Controller.EndGame(
                            gameEndedMessage.GameID, 
                            gameEndedMessage.WinnerIndex, 
                            gameEndedMessage.Reason);
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
        public PrivateSaxophone<GameChoiceMessage> Saxophone;

        public ClientSideGameSynchronizer(
            int gameID, 
            PrivateSaxophone<GameChoiceMessage> saxophone)
        {
            GameID = gameID;
            Saxophone = saxophone;
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
