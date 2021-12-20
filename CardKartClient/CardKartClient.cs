using CardKartClient.GUI;
using CardKartShared.Network.Messages;
using CardKartShared.Util;

namespace CardKartClient
{
    static class CardKartClient
    {
        public static GUIController GUI;
        public static Brainframe Controller;

        static void Main(string[] args)
        {
            if (Constants.IsDevVersion)
            {
                Logging.AddConsoleLogger();
            }

            var connection = Constants.DebugServer.Connect();
            connection.SendMessage(new HandshakeMessage(Constants.Version, 16).Encode());
            var responseRaw = connection.ReceiveMessage();
            var response = new GenericResponseMessage();
            response.Decode(responseRaw);
            int i = 4;

            GUI = new GUIController();

            Controller = new Brainframe();
            Controller.Startup();
        }
    }
}
