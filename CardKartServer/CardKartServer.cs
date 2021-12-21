using CardKartShared.Util;

namespace CardKartServer
{
    internal class CardKartServer
    {
        public static ConnectionHandler ConnectionHandler { get; } = new ConnectionHandler();
        public static ClientHandler ClientHandler { get; } = new ClientHandler();
        public static GameCoordinator GameCoordinator { get; } = new GameCoordinator();

        static void Main(string[] args)
        {
            Logging.AddConsoleLogger();
            Logging.Log(LogLevel.Info, $"Running CardKart server version {Constants.Version}.");

            ConnectionHandler.Start();
        }
    }
}
