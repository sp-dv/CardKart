﻿using CardKartClient.GUI;
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

            Logging.Log(
                LogLevel.Info,
                $"Starting CardKart version {Constants.Version}");

            Server = new ServerConnection();
            if (!Server.Connect()) { return; }

            GUI = new GUIController();

            Controller = new ClientController();
            Controller.Startup();
        }
    }
}
