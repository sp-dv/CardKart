using CardKartShared.GameState;
using CardKartShared.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardKartServer
{
    internal static class PackGenerator
    {
        private static Dictionary<CardSets, SetPackBuilder> PackBuilders { get; } = new Dictionary<CardSets, SetPackBuilder>();

        public static void Load()
        {
            foreach (var card in Card.AllRealCards)
            {
                if (!PackBuilders.ContainsKey(card.CardSet)) { PackBuilders[card.CardSet] = new SetPackBuilder(); }

                PackBuilders[card.CardSet].AddCard(card);
            }
        }

        public static List<CardTemplates> GeneratePack(Packs pack)
        {
            var rippedTemplates = new List<CardTemplates>();

            for (int i = 0; i < 7; i++)
            {
                rippedTemplates.Add(PackBuilders[CardSets.FirstEdition].GetRandomTemplate(CardRarities.Common));
            }
            for (int i = 0; i < 4; i++)
            {
                rippedTemplates.Add(PackBuilders[CardSets.FirstEdition].GetRandomTemplate(CardRarities.Uncommon));
            }
            for (int i = 0; i < 1; i++)
            {
                rippedTemplates.Add(PackBuilders[CardSets.FirstEdition].GetRandomTemplate(CardRarities.Rare));
            }

            return rippedTemplates;
        }
    }


    class SetPackBuilder
    {
        private List<Card> Common { get; } = new List<Card>();
        private List<Card> Uncommon { get; } = new List<Card>();
        private List<Card> Rare { get; } = new List<Card>();
        private List<Card> Legendary { get; } = new List<Card>();

        private Random RNG = new Random();

        public void AddCard(Card card)
        {
            if (!card.IsInPacks) { return; }

            switch (card.Rarity)
            {
                case CardRarities.Common: { Common.Add(card); } break;
                case CardRarities.Uncommon: { Uncommon.Add(card); } break;
                case CardRarities.Rare: { Rare.Add(card); } break;
                case CardRarities.Legendary: { Legendary.Add(card); } break;
            }
        }

        public CardTemplates GetRandomTemplate(CardRarities rarity)
        {
            List<Card> pickfrom;

            switch (rarity)
            {
                case CardRarities.Common: pickfrom = Common; break;
                case CardRarities.Uncommon: pickfrom = Uncommon; break;
                case CardRarities.Rare: pickfrom = Rare; break;
                default: return CardTemplates.None;
            }

            if (rarity == CardRarities.Rare)
            {
                if (RNG.Next(9) == 0)
                {
                    pickfrom = Legendary;
                }
            }

            var randomNumber = RNG.Next(pickfrom.Count - 1);
            return pickfrom[randomNumber].Template;
        }
    }
}
