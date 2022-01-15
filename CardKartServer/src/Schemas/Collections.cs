
using CardKartShared.GameState;
using CardKartShared.Util;
using LiteDB;
using System;
using System.Collections.Generic;

namespace CardKartServer.Schemas
{
    internal static class Collections
    {
        private static ILiteCollection<UserCollectionEntry> Collection { get; set; }

        private static object ModifyLock = new object();

        public static void Load(ILiteCollection<UserCollectionEntry> collectionEntries)
        {
            Collection = collectionEntries;

            Collection.EnsureIndex(info => info.UserID);
        }

        public static DBQuery<UserCollectionEntry> GetCollectionByUserID(string userID)
        {
            if (userID == null) { return new DBError("'userID' was null in GetCollection."); }

            var entry = Collection.FindOne(info => info.UserID == userID);
            if (entry == null)
            {
                var newEntry = new UserCollectionEntry
                {
                    UserID = userID,
                    OwnedCards = new Dictionary<CardTemplates, int>(),
                    OwnedPacks = new Dictionary<Packs, int>()
                };

                Collection.Insert(newEntry);
                return newEntry;
            }
            else
            {
                return entry;
            }
        }

        public static DBQuery<List<CardTemplates>> RipPack(string userID, Packs pack)
        {
            UserCollectionEntry entry = null;
            GetCollectionByUserID(userID)
                .Then(res => entry = res);
            if (entry == null) { return new DBError("Couldn't get user collection."); }

            var rippedTemplates = PackGenerator.GeneratePack(pack);

            foreach (var template in rippedTemplates)
            {
                entry.AddCard(template);
            }

            Collection.Update(entry);

            return rippedTemplates;
        }

        public static bool DislodgeCardByUserID(string userID, CardTemplates template)
        {
            if (userID == null) { return false; }

            lock (ModifyLock)
            {
                var entry = Collection.FindOne(info => info.UserID == userID);
                if (entry == null || entry.OwnedCards == null) { return false; }
                if (!entry.RemoveCard(template)) { return false; }
                
                Collection.Update(entry);
                return true;
            }
        }

        public static double? GiveGaldzByUserID(string userID, double amount)
        {
            if (userID == null) { return null; }
            if (amount <= 0)
            {
                Logging.Log(LogLevel.Warning, $"Tried to give and illegal amount of Galdz: {amount}.");
                return null;
            }

            lock (ModifyLock)
            {
                var entry = Collection.FindOne(info => info.UserID == userID);
                if (entry == null) { return null; }

                entry.Galds += amount;
                Collection.Update(entry);
                
                return entry.Galds;
            }
        }
    }


    public class UserCollectionEntry
    {
        public int Id { get; set; }
        public string UserID { get; set; }
        
        public double Galds { get; set; }
        public Dictionary<CardTemplates, int> OwnedCards { get; set; }
        public Dictionary<Packs, int> OwnedPacks { get; set; }

        public void AddCard(CardTemplates template)
        {
            if (!OwnedCards.ContainsKey(template)) { OwnedCards[template] = 0; }

            OwnedCards[template]++;
        }

        public bool RemoveCard(CardTemplates template)
        {
            if (!OwnedCards.ContainsKey(template)) { return false; }

            var ownedCount = OwnedCards[template];
            
            if (ownedCount <= 0)
            {
                Logging.Log(LogLevel.Warning, "Tomfuckery when removing a Card from a UserCollectionEntry.");
                return false;
            }
            else if (ownedCount == 1)
            {
                OwnedCards.Remove(template);
                return true;
            }
            else
            {
                OwnedCards[template]--;
                return true;
            }
        }
    }
}
