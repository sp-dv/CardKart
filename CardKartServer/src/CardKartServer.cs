using CardKartShared.Util;
using System;

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

            Database.Load();
            ConnectionHandler = new ConnectionHandler();
            ClientHandler = new ClientHandler();
            GameCoordinator = new GameCoordinator();
            
            ConnectionHandler.Start();
        }
    }
}
