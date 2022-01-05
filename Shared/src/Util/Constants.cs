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


        public static Color PaletteColor(ManaColour colour)
        {
            switch (colour)
            {
                case ManaColour.Red: { return Color.Firebrick; }
                case ManaColour.Black: { return Color.DarkSlateGray; }
                case ManaColour.Blue: { return Color.Aqua; }
                case ManaColour.Purple: { return Color.Purple; }
                case ManaColour.White: { return Color.White; }
                case ManaColour.Green: { return Color.Green; }
                case ManaColour.Colourless: { return Color.Gray; }
                case ManaColour.Mixed: { return Color.Gold; }

                default: throw new Exception();
            }
        }

        public static Color RarityColor(CardRarities rarity)
        {
            switch (rarity)
            {
                case CardRarities.Common: { return Color.GhostWhite; }
                case CardRarities.Uncommon: { return Color.Blue; }
                case CardRarities.Rare: { return Color.Purple; }
                case CardRarities.Legendary: { return Color.DarkOrange; }
            }

            throw new NotImplementedException();
        }
    }
}
