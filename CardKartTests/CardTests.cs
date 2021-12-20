using CardKartShared.GameState;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CardKartTests
{
    [TestClass]
    public class CardTests
    {
        CardTemplates[] AllTemplates = (CardTemplates[])Enum.GetValues(typeof(CardTemplates));

        [TestMethod]
        public void TemplateInitilization()
        {

            foreach (var template in AllTemplates)
            {
                var card = new Card(template);

                Assert.IsNotNull(card.Name);

                Assert.AreNotEqual(CardTypes.None, card.Type);
            }
        }
    }
}