using CardKartServer.Schemas;
using CardKartShared.GameState;
using CardKartShared.Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardKartServer
{
    internal class AuctionHouse
    {
        private Dictionary<CardTemplates, double> BuyBackPrices = new Dictionary<CardTemplates, double>();

        public AuctionHouse()
        {
            Func<Card, double> calculateBuyBackPrice = card => { 
                if (!card.IsInPacks) { return double.NaN; }

                switch (card.Rarity)
                {
                    case CardRarities.Common: return 1;
                    case CardRarities.Uncommon: return 5;
                    case CardRarities.Rare: return 20;
                    case CardRarities.Legendary: return 75;
                    default: return double.NaN;
                }
            };

            foreach (var card in Card.AllRealCards.Where(card => card.IsInPacks))
            {
                BuyBackPrices[card.Template] = calculateBuyBackPrice(card);
            }
        }

        public Quote GetQuote(CardTemplates template)
        {
            var bid = Bids.GetBestBid(template);
            var ask = Bids.GetBestAsk(template);

            var bidValue = bid != null ? bid.StrikePrice : GetBuyBackPrice(template);
            var askValue = ask != null ? ask.StrikePrice : double.NaN;

            return new Quote
            {
                Bid = bidValue,
                Ask = askValue,
            };
        }

        public double? QuickSell(string userID, CardTemplates template)
        {
            if (userID == null) { return null; }

            if (!Collections.DislodgeCardByUserID(userID, template)) { return null; }


            throw new NotImplementedException();
        }

        private double GetBuyBackPrice(CardTemplates template)
        {
            if (!BuyBackPrices.ContainsKey(template)) { return double.NaN; }
            return BuyBackPrices[template];
        }
    }
}
