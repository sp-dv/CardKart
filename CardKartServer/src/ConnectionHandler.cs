
using CardKartShared.Util;

namespace CardKartServer
{
    internal class ConnectionHandler
    {
        public void Start()
        {
            Logging.Log(LogLevel.Info, "Listening for connections");

            Constants.DebugServer.Host((connection, client) =>
            {
                Logging.Log(LogLevel.Info, $"{client.Client.RemoteEndPoint} connected.");
                CardKartServer.ClientHandler.AddConnection(connection);
            });
        }
    }
}
