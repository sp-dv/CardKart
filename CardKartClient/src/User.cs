using CardKartShared.GameState;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CardKartClient
{
    internal static class User
    {
        public static string Username { get; set; }
        
        public static UserConfig Configuration { get; private set; } = LoadConfig();
        private const string UserConfigFile = "./user.config";

        public static void SaveDeck(Deck deck)
        {
            if (deck == null) { return; }
            var deckString = JsonConvert.SerializeObject(deck);
            File.WriteAllText("./a.ckd", deckString);
        }

        public static Deck LoadDeck()
        {
            if (!File.Exists("./a.ckd")) { return null; }
            var deckString = File.ReadAllText("./a.ckd");
            return JsonConvert.DeserializeObject<Deck>(deckString);
        }

        public static void SaveConfig()
        {
            var configString = JsonConvert.SerializeObject(Configuration);
            File.WriteAllText(UserConfigFile, configString);
        }

        private static UserConfig LoadConfig()
        {
            if (!File.Exists(UserConfigFile))
            {
                return new UserConfig();
            }

            try
            {
                var configString = File.ReadAllText(UserConfigFile);
                return JsonConvert.DeserializeObject<UserConfig>(configString);
            }
            catch (Exception ex)
            {
                return new UserConfig();
            }
        }


    }

    class UserConfig
    {
        public string DefaultUsername { get; set; } = "";
    }
}
