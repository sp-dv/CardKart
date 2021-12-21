using CardKartShared.GameState;
using CardKartShared.Network;
using System;
using System.Drawing;

namespace CardKartShared.Util
{
    public static class Constants
    {
        public const string Version = "0.0.0";
        public static bool IsDevVersion = true;

        public static Server CurrentServer => DebugServer;
        public static readonly Server ProductionServer 
            = new Server(4444, "78.138.17.232");
        public static readonly Server DebugServer 
            = new Server(4444, "localhost");

        public static Color PaletteColors(GameColour colour)
        {
            switch (colour)
            {
                case GameColour.Red: { return Color.Firebrick; }
                case GameColour.Black: { return Color.Gray; }
                default: throw new Exception();
            }
        }
    }
}
