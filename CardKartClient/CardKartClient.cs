using CardKartClient.GUI;
using CardKartShared.Util;

namespace CardKartClient
{
    static class CardKartClient
    {
        public static GUIController GUI;
        public static ClientController Controller;
        public static ServerConnection Server;

        static void Main(string[] args)
        {
            if (Constants.IsDevVersion)
            {
                Logging.AddConsoleLogger();
            }

            Server = new ServerConnection();
            Server.Connect();

            GUI = new GUIController();

            Controller = new ClientController();
            Controller.Startup();
        }
    }
}
