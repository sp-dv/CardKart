using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardKartShared.GameState
{
    public class Deck
    {
        public CardTemplates HeroCardTemplate;
        public CardTemplates[] DeckTemplates;
        public CardTemplates[] AllTemplates => (new CardTemplates[] { HeroCardTemplate }).Concat(DeckTemplates).ToArray();

        public Deck(CardTemplates heroTemplate, CardTemplates[] deckTemplates)
        {
            HeroCardTemplate = heroTemplate;
            DeckTemplates = deckTemplates;
        }

        public Deck(CardTemplates[] allTemplates)
        {
            HeroCardTemplate = allTemplates[0];
            DeckTemplates = allTemplates.Skip(1).ToArray();
        }

        public Deck()
        {
        }
    }
}
