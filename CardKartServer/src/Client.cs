using CardKartServer.Schemas;
using CardKartShared.GameState;
using CardKartShared.Network;
using CardKartShared.Network.Messages;
using CardKartShared.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CardKartServer
{
    internal class ClientHandler
    {
        private List<Client> ConnectedClients = new List<Client>();
        private Dictionary<string, Client> LoggedInClients = new Dictionary<string, Client>();


        public void AddConnection(Connection clientConnection)
        {
            var client = new Client(clientConnection);
            if (client.ServersideHandshake())
            {
                ConnectedClients.Add(client);
                client.Connection.Closed += () => RemoveClient(client);
                Logging.Log(LogLevel.Info, "Client verified.");
                client.StartListening();
            }
        }

        public void RemoveClient(Client client)
        {
            ConnectedClients.Remove(client);
            if (client.IsLoggedIn)
            {
                LoggedInClients.Remove(client.UserID);
            }

            client.LogOut();
        }

        public void HandleMessage(Client client, RawMessage rawMessage)
        {
            if (!(client.IsLoggedIn || rawMessage.AllowUnverified))
            {
                client.Connection.SendMessage(new GenericResponseMessage(
                    GenericResponseMessage.Codes.Error,
                    "Request requires login.").Encode());
                return;
            }

            switch (rawMessage.MessageType)
            {

                case MessageTypes.GameChoiceMessage:
                    {
                        CardKartServer.GameCoordinator.HandleMessage(client, rawMessage);
                    }
                    break;

                case MessageTypes.JoinQueueRequest:
                    {
                        var joinQueueRequest = new JoinQueueRequest();
                        joinQueueRequest.Decode(rawMessage);

                        CardKartServer.GameCoordinator.JoinQueue(client);

                        var response = new GenericResponseMessage();
                        response.Code = GenericResponseMessage.Codes.OK;
                        client.Connection.SendMessage(response.Encode());
                    } break;

                case MessageTypes.LoginRequest:
                    {
                        if (client.IsLoggedIn) { ErrorOutAndSendClientGenericError(client, "You are logged in sir."); }

                        var loginRequest = new LoginRequest();
                        loginRequest.Decode(rawMessage);

                        LoginInfo.VerifyUser(loginRequest.Username, loginRequest.Password).Then(info => {
                            if (LoggedInClients.ContainsKey(info.UserID))
                            {
                                client.Connection.SendMessage(new GenericResponseMessage(GenericResponseMessage.Codes.Error, "You are logged in already."));
                                return;
                            }
                            client.LogIn(info);
                            LoggedInClients[client.UserID] = client;
                            client.Connection.SendMessage(new GenericResponseMessage(GenericResponseMessage.Codes.OK, ""));
                            Logging.Log(LogLevel.Info, $"{client.Username} logged in.");
                        }).Err(err => { 
                            client.Connection.SendMessage(new GenericResponseMessage(GenericResponseMessage.Codes.Error, err.ErrorMessage));
                        });

                    } break;

                case MessageTypes.SurrenderGameMessage:
                    {
                        var surrenderMessage = new SurrenderGameMessage();
                        surrenderMessage.Decode(rawMessage);

                        CardKartServer.GameCoordinator.SurrenderGame(client, surrenderMessage.GameID);
                    } break;

                case MessageTypes.RegisterRequestMessage:
                    {
                        if (client.IsLoggedIn) { ErrorOutAndSendClientGenericError(client, "You are logged in sir."); }

                        var registerRequest = new RegisterUserRequest();
                        registerRequest.Decode(rawMessage);

                        var response = new GenericResponseMessage();
                        LoginInfo.RegisterUser(registerRequest.Username, registerRequest.Password).Then(info => {
                            response.Code = GenericResponseMessage.Codes.OK;
                        }).Err(err => {
                            response.Code = GenericResponseMessage.Codes.Error;
                            response.Info = err.ErrorMessage;
                        });
                        client.Connection.SendMessage(response);
                    } break;

                case MessageTypes.RipPackRequest:
                    {
                        var request = new RipPackRequest();
                        request.Decode(rawMessage);

                        var response = new RipPackResponse();
                        
                        Collections.RipPack(client.UserID, request.Pack).Then(res => { 
                            response.Templates = res.ToArray();
                        }).Err(err => {
                            response.Templates = new CardTemplates[0];
                        });
                        
                        client.Connection.SendMessage(response);
                    } break;

                case MessageTypes.GetCollectionRequest:
                    {
                        var request = new GetCollectionRequest();
                        request.Decode(rawMessage);

                        var response = new GetCollectionResponse();

                        var collection = Collections.GetCollectionByUserID(client.UserID).Then(res => {
                            response.Galds = res.Galds;
                            response.OwnedCards = res.OwnedCards;
                            response.OwnedPacks = res.OwnedPacks;
                        }).Err(err => {
                            response.Galds = -1;
                            response.OwnedCards = new Dictionary<CardTemplates, int>();
                            response.OwnedPacks = new Dictionary<Packs, int>();
                        });

                        client.Connection.SendMessage(response);
                    } break;

                case MessageTypes.CardQuoteRequest:
                    {
                        var request = new GetQuoteRequest();
                        request.Decode(rawMessage);

                        var response = new GetQuoteResponse
                        {
                            Quote = CardKartServer.AuctionHouse.GetQuote(request.Template)
                        };
                        client.Connection.SendMessage(response);
                    } break;

                case MessageTypes.SellCardRequest:
                    {
                        var request = new SellCardRequest();
                        request.Decode(rawMessage);

                        if (!request.IsQuickSellOrder)
                        {
                            client.Connection.SendMessage(new SellCardResponse { 
                                Success = false,
                                Error  = "Not supported",
                            });
                            return;
                        }

                        var newBalance = CardKartServer.AuctionHouse.QuickSell(client.UserID, request.CardTemplate);

                        var response = new SellCardResponse
                        {
                            Quote = CardKartServer.AuctionHouse.GetQuote(request.Template)
                        };
                        client.Connection.SendMessage(response);
                    } break;
            }
        }

        private void ErrorOutAndSendClientGenericError(Client client, string errorMessage)
        {
            client.Connection.SendMessage(new GenericResponseMessage
            {
                Code = GenericResponseMessage.Codes.Error,
                Info = errorMessage,
            });
        }
    }

    internal class Client
    {
        public Connection Connection { get; }

        public string Username { get; private set; }
        public string UserID { get; private set; }

        public bool IsLoggedIn => Username != null && UserID != null;

        public Client(Connection connection)
        {
            Connection = connection;
            Connection.Closed += () =>
            {
                CardKartServer.ClientHandler.RemoveClient(this);
            };
        }

        public bool ServersideHandshake()
        {
            var handshakeMessageRaw = Connection.ReceiveMessage();

            if (handshakeMessageRaw.MessageType != MessageTypes.HandshakeMessage)
            {
                Connection.SendMessage(
                    new GenericResponseMessage(GenericResponseMessage.Codes.Error,
                    "First message must be handshake.").Encode());
                Logging.Log(
                    LogLevel.Info,
                    "Received intial message which wasn't a handshake.");

                return false;
            }

            var handshakeMessage = new HandshakeMessage();
            handshakeMessage.Decode(handshakeMessageRaw);

            var response = new HandshakeResponse();

            if (handshakeMessage.VersionString != Constants.Version)
            {
                Logging.Log(
                    LogLevel.Info,
                    $"Outdated version connected. Version: {handshakeMessage.VersionString}");

                response.Error = $"Outdated version. Current version is {Constants.Version}";
                Connection.SendMessage(response.Encode());
                return false;
            }

            var text = RSAEncryption.RSADecrypt(handshakeMessage.MagicBytes, CardKartServer.ServerKey);

            // User doesn't have the right public key.
            if (text == null) 
            {
                Logging.Log(
                    LogLevel.Info,
                    "Received handshake with wrong public key.");

                response.Error = $"Wrong public key.";
                Connection.SendMessage(response.Encode());
                return false;
            } 

            var magic = JsonConvert.DeserializeObject<Magic>(text);
            var encryptionSuite = new EncryptionSuite(magic.AesParams);

            response.MagicBytes = encryptionSuite.Encrypt(magic.Nonce);
            Connection.SendMessage(response.Encode());

            Connection.EncryptionSuite = encryptionSuite;

            return true;
        }

        public void LogIn(LoginInfoEntry entry)
        {
            Username = entry.Username;
            UserID = entry.UserID;
        }

        public void LogOut()
        {
            Username = null;
            UserID = null;
        }

        public void StartListening()
        {
            new Thread(() =>
            {
                while (!Connection.IsClosed)
                {
                    var rawMessage = Connection.ReceiveMessage();
                    CardKartServer.ClientHandler.HandleMessage(this, rawMessage);
                }
            }).Start();
        }

    }
}
