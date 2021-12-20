using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardKartShared.GameState
{
    public class Deck
    {
        public CardTemplates[] CardTemplates;

        public Deck(CardTemplates[] cardTemplates)
        {
            CardTemplates = cardTemplates;
        }
    }
}
