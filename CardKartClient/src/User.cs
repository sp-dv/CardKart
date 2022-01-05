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

        public static void SaveDeck(IEnumerable<Card> DeckCards)
        {
            var deck = new Deck(DeckCards.Select(card => card.Template).ToArray());
            var deckString = JsonConvert.SerializeObject(deck);
            File.WriteAllText("./a.ckd", deckString);
        }

        public static Deck LoadDeck()
        {
            if (!File.Exists("./a.ckd")) { return null; }
            var deckString = File.ReadAllText("./a.ckd");
            return JsonConvert.DeserializeObject<Deck>(deckString);
        }
    }
}
