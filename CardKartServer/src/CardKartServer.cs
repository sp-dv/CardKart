using CardKartServer.Schemas;
using CardKartShared.Network;
using CardKartShared.Util;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CardKartServer
{
    internal class CardKartServer
    {
        public static ConnectionHandler ConnectionHandler { get; private set; }
        public static ClientHandler ClientHandler { get; private set; }
        public static GameCoordinator GameCoordinator { get; private set; }

        static void Main(string[] args)
        {
            Logging.AddConsoleLogger();
            Logging.Log(LogLevel.Info, $"Running CardKart server version {Constants.Version}.");

            RSAEncryption.LoadPrivateKey(JsonConvert.DeserializeObject<RSAParameters>(File.ReadAllText("server.key")));

            Database.Load();

            ClientHandler = new ClientHandler();
            GameCoordinator = new GameCoordinator();

            ConnectionHandler = new ConnectionHandler();
            ConnectionHandler.Start();
        }
    }
}
