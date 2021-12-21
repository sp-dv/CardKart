using CardKartShared.Network;
using CardKartShared.Network.Messages;
using CardKartShared.Util;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CardKartServer
{
    internal class ClientHandler
    {
        private List<Client> ConnectedClients = new List<Client>();

        public void AddConnection(Connection clientConnection)
        {
            var client = new Client(clientConnection);
            if (client.Handshake())
            {
                ConnectedClients.Add(client);
                Logging.Log(LogLevel.Info, "Client verified.");
                client.StartListening();
            }
        }

        public void RemoveClient(Client client)
        {
            ConnectedClients.Remove(client);
        }

        public void HandleMessage(Client client, RawMessage rawMessage)
        {
            switch (rawMessage.MessageType)
            {
                case MessageTypes.JoinQueueRequest:
                    {
                        var joinQueueRequest = new JoinQueueRequest();
                        joinQueueRequest.Decode(rawMessage);

                        CardKartServer.GameCoordinator.JoinQueue(client);

                        var response = new GenericResponseMessage();
                        response.Code = GenericResponseMessage.Codes.OK;
                        client.Connection.SendMessage(response.Encode());
                    } break;

                case MessageTypes.GameChoiceMessage:
                    {
                        CardKartServer.GameCoordinator.HandleMessage(client, rawMessage);
                    } break;
            }
        }
    }

    internal class Client
    {
        public Connection Connection { get; }

        public Client(Connection connection)
        {
            Connection = connection;
            Connection.Closed += () =>
            {
                CardKartServer.ClientHandler.RemoveClient(this);
            };
        }

        public bool Handshake()
        {
            var handshakeMessageRaw = Connection.ReceiveMessage();

            if (handshakeMessageRaw.MessageType != MessageTypes.HandshakeMessage)
            {
                Connection.SendMessage(
                    new GenericResponseMessage(GenericResponseMessage.Codes.Error,
                    "First message must be handshake.").Encode());
                Logging.Log(
                    LogLevel.Debug,
                    "Received intial message which wasn't a handshake.");

                return false;
            }

            var handshakeMessage = new HandshakeMessage();
            handshakeMessage.Decode(handshakeMessageRaw);

            if (handshakeMessage.VersionString != Constants.Version)
            {
                Connection.SendMessage(
                    new GenericResponseMessage(GenericResponseMessage.Codes.Error,
                    $"Outdated version. Current version is {Constants.Version}").Encode());
                Logging.Log(
                    LogLevel.Debug,
                    $"Outdated version connected. Version: {handshakeMessage.VersionString}");
                return false;
            }

            Connection.SendMessage(
                    new GenericResponseMessage(GenericResponseMessage.Codes.OK,
                    $"{handshakeMessage.MagicNumber + 1}").Encode());

            return true;
        }

        public void StartListening()
        {
            new Thread(() =>
            {
                while (true)
                {
                    var rawMessage = Connection.ReceiveMessage();
                    CardKartServer.ClientHandler.HandleMessage(this, rawMessage);
                }
            }).Start();
        }

    }
}
