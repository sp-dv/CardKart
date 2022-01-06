using CardKartShared.GameState;
using CardKartShared.Network;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Security.Cryptography;

namespace CardKartShared.Util
{
    public static class Constants
    {
        public const string Version = "0.0.1";
        public static bool IsDevVersion = false;

        private static RSAParameters DebugPublicKey =>
            JsonConvert.DeserializeObject<RSAParameters>("{\"D\":null,\"DP\":null,\"DQ\":null,\"Exponent\":\"AQAB\",\"InverseQ\":null,\"Modulus\":\"qIsOn6mUS08MDm2MNngj9UN1ZqM5bqKbic4nRBSrt4FkzE5vxv7gFlRW0t6phBvrlTBcGpYWxput6PMHJQ2zHzgPnOt9kgHKUy/Oh44p7IqeYoGKmSBDeUfw1vr6+kCRmBXSUVxug9RcRgnT1daVClCaKsLs/zTNosVlgx17RgU=\",\"P\":null,\"Q\":null}");

        private static RSAParameters ProductionKey =>
            JsonConvert.DeserializeObject<RSAParameters>("{\"D\":null,\"DP\":null,\"DQ\":null,\"Exponent\":\"AQAB\",\"InverseQ\":null,\"Modulus\":\"7y+IZntswo6sojDQeewdPM5RO6U25eNVU9nBBwWnpDHRw/Ppq9uH5l1EyZMGd32QydDwGpeUKtbk35YsL4f5vp/YFbWkasqbp3Hv260RoyZCcw0HwLqBey9YVqe25B+CFGTv5MLNMtIiCVsqUUA4qFeVNR/pPVkKSphFYbqQORk=\",\"P\":null,\"Q\":null}");
        
        public static Server CurrentServer => IsDevVersion ? DebugServer : ProductionServer;
        public static readonly Server ProductionServer
            = new Server("78.138.17.232", 4444, ProductionKey);
        public static readonly Server DebugServer
            = new Server("localhost", 4444, DebugPublicKey);

        public static GUIConstants GUI { get; } = new GUIConstants();

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
                case CardRarities.Token: { return Color.Black; }
            }

            throw new NotImplementedException();
        }

        public static string KeywordExplanation(KeywordAbilityNames ability)
        {
            switch (ability)
            {
                case KeywordAbilityNames.Bloodlust: { return "Creatures with Bloodlust don't suffer from summoning sickness"; }
                case KeywordAbilityNames.Vigilance: { return "Creatures with Vigilance don't exhaust when attacking"; }
                case KeywordAbilityNames.Flying: { return "Creatures with Flying can only be blocked by creatures with Flying or Range"; }
                case KeywordAbilityNames.Range: { return "Creatures with Range can block creatures with Flying"; }
                default: return "";
            }
        }

        public static int MaxCountByRarity(CardRarities rarity)
        {
            switch (rarity)
            {
                case CardRarities.Common: return 4;
                case CardRarities.Uncommon: return 3;
                case CardRarities.Rare: return 2;
                case CardRarities.Legendary: return 1;
                default: return 0;
            }
        }
    }

    public class GUIConstants
    {
        /*  Reuse me daddy
        
        public float A => 0.0f;
        public float B => 0.0f;
        public float C => 0.0f;
        public float D => 0.0f;
        public float E => 0.0f;
        public float F => 0.0f;
        public float G => 0.0f;
        public float H => 0.0f;
        public float I => 0.0f;
        public float J => 0.0f;

         */

        public float DeckPanelX => -0.95f;
        public float DeckPanelY => -0.65f;
        public float DeckPanelWidth => 0.35f;
        public float DeckPanelHeight => 1.5f;


        public float A => 0.02f;
        public float B => 0.079f;


        public float C => 0.02f;
        public float D => 1.39f;

        public float E => 0.06f;

        public float F => 0.17f;
        public float G => 0.060f;
        
        public float H => 0.31f;
        public float I => 0.05f;
        public float J => 0.0f;

    }
}
