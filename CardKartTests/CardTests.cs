using CardKartShared.GameState;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CardKartTests
{
    [TestClass]
    public class CardTests
    {
        Card[] AllCards = Enum.GetValues(typeof(CardTemplates)).Cast<CardTemplates>()
            .Where(template => template != CardTemplates.None)
            .Select(template => new Card(template)).ToArray();

        [TestMethod]
        public void TemplateInitilization()
        {
            foreach (var card in AllCards)
            {
                Assert.IsNotNull(card.Name);
                Assert.IsNotNull(card.Abilities);
                Assert.AreNotEqual(CardTypes.None, card.Type);
                Assert.AreNotEqual(CardRarities.None, card.Rarity);

                if (card.Type == CardTypes.Hero)
                {

                }
                else
                {
                    Assert.IsNotNull(card.CastingCost);
                }
            }
        }
    }
}