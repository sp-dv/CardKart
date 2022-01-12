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
        // Last release: 0.0.4
        public const string Version = "0.0.0";
        public static bool IsReleaseVersion = false;

        private static RSAParameters DebugPublicKey => 
            JsonConvert.DeserializeObject<RSAParameters>("{\"D\":null,\"DP\":null,\"DQ\":null,\"Exponent\":\"AQAB\",\"InverseQ\":null,\"Modulus\":\"xt0Ce6WPiB2C9hFlRANg7cV96tYb/6QwC3zQaleQZUuchwnhIlMFWgKVECfUfGTXu/UrPa0JeG6kyJQycXnYJYTc6PO08I5BxuFfRR1rNcWmM3pWidJhTuFPgv0G+1J+elsGfS4K641OI+ZNgw/V1Hq1bZOl3XXKWKaWLOFvsKE=\",\"P\":null,\"Q\":null}");
        private static RSAParameters ProductionKey => 
            JsonConvert.DeserializeObject<RSAParameters>("{\"D\":null,\"DP\":null,\"DQ\":null,\"Exponent\":\"AQAB\",\"InverseQ\":null,\"Modulus\":\"4XELOaR99v2T9GjQ4g8CFTWwvvBF5zBcl+LIpw7W3usK9sfI7xSxh689Te+ltRnRD0NNiV5hNCqdFRbc2ycwhSdSeCAw3C7QgjVGooVq0h9FKJgraXi+ksIdpiko3CBB9XFsHtNM2W5uxSpcCJpLpObprSDHtkVBdK0vknLgyDU=\",\"P\":null,\"Q\":null}");
        
        public static Server CurrentServer => IsReleaseVersion ? ProductionServer : DebugServer;

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
                case KeywordAbilityNames.Bloodlust: { return "Doesn't suffer from summoning sickness"; }
                case KeywordAbilityNames.Vigilance: { return "Doesn't exhaust when attacking"; }
                case KeywordAbilityNames.Flying: { return "Can only be blocked by creatures with Flying or Range"; }
                case KeywordAbilityNames.Range: { return "Can block creatures with Flying"; }
                case KeywordAbilityNames.Reinforcement: { return "Can be cast whenever you can cast a scroll"; }
                case KeywordAbilityNames.Protected: { return "The next time this creature takes damage; prevent it"; }
                case KeywordAbilityNames.Terrify: { return "Can only be blocked by creatures with a higher mana cost"; }
                case KeywordAbilityNames.Rampage: { return "When this creature attacks any unblocked damage is dealt to the defending player"; }
                case KeywordAbilityNames.Lifesteal: { return "When this creature deals damage restore health to you equal to the damage dealt"; }
                case KeywordAbilityNames.Stoning: { return "When this creature deals damage to another creature; stun that creature"; }
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


        public float DeckPanelCountOffsetX => 0.02f;
        public float DeckPanelCountOffsetY => 0.079f;


        public float DeckPanelSubcomponentX0 => 0.02f;
        public float DeckPanelSubcomponentY0 => 1.39f;

        public float DeckPanelSubcomponentYOffset => 0.06f;

        public float DeckPanelCardNameOffsetX => 0.17f;
        public float DeckPanelCardNameOffsetY => 0.060f;
        
        public float DeckSubcomponentWidth => 0.31f;
        public float DeckSubcomponentHeight => 0.05f;

    }
}
