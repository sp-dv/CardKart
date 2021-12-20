﻿using CardKartShared.Network;
using CardKartShared.Network.Messages;
using CardKartShared.Util;
using System.Collections.Generic;
using System.Text;

namespace CardKartServer
{
    internal class ClientHandler
    {
        private List<Client> ConnectedClients = new List<Client>();

        public void AddConnection(Connection clientConnection)
        {
            var client = new Client();
            client.Connection = clientConnection;

            if (client.Handshake())
            {
                ConnectedClients.Add(client);
                Logging.Log(LogLevel.Info, "Client verified.");
            }
        }
    }

    internal class Client
    {
        public Connection Connection;

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

    }
}
