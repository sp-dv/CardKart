using CardKartShared.GameState;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardKartServer.Schemas
{
    internal class Bids
    {
        private static ILiteCollection<BidEntry> SellOrders { get; set; }
        private static ILiteCollection<BidEntry> BuyOrders { get; set; }

        private object DealInProgressLock = new object();

        public static BidEntry GetBestBid(CardTemplates template)
        {
            return 
                BuyOrders.Query()
                .Where(entry => entry.CardTemplate == template)
                .OrderByDescending(entry => entry.StrikePrice)
                .FirstOrDefault();
        }

        public static BidEntry GetBestAsk(CardTemplates template)
        {
            return
                SellOrders.Query()
                .Where(entry => entry.CardTemplate == template)
                .OrderBy(entry => entry.StrikePrice)
                .FirstOrDefault();
        }

        public static double? FillSell(double ask)
        {

        }

        public static void Load(ILiteCollection<BidEntry> sellOrders, ILiteCollection<BidEntry> buyOrders)
        {
            SellOrders = sellOrders;
            BuyOrders = buyOrders;

            SellOrders.EnsureIndex(info => info.OwnerUserID);
            BuyOrders.EnsureIndex(info => info.OwnerUserID);

            SellOrders.EnsureIndex(info => info.CardTemplate);
            BuyOrders.EnsureIndex(info => info.CardTemplate);
        }
    }

    public class BidEntry
    {
        public int Id { get; set; }
        public string OwnerUserID { get; set; }
        public CardTemplates CardTemplate { get; set; }
        public double StrikePrice { get; set; }
    }

}
