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
        private PublicSaxophone<RawMessage> RequestResponseSaxophone 
            = new PublicSaxophone<RawMessage>();
        private PublicSaxophone<GameChoiceMessage> GameChoiceMessageSaxophone
            = new PublicSaxophone<GameChoiceMessage>();

        public bool Connect()
        {
            Connection = Constants.CurrentServer.Connect();

            var magic = new Magic();
            var encryptionSuite = new EncryptionSuite(magic.AesParams);

            var handshakeMessage = new HandshakeMessage();
            handshakeMessage.VersionString = Constants.Version;
            handshakeMessage.MagicBytes = RSAEncryption.RSAEncrypt(JsonConvert.SerializeObject(magic));
            Connection.SendMessage(handshakeMessage.Encode());

            var responseRaw = Connection.ReceiveMessage();
            var response = new HandshakeResponse();
            response.Decode(responseRaw);


            if (response.Error != null)
            {
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


            Connection.EncryptionSuite = encryptionSuite;

            new Thread(() =>
            {
                while (true)
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
