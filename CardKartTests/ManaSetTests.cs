using CardKartShared.GameState;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CardKartTests
{
    [TestClass]
    public class ManaSetTests
    {
        [TestMethod]
        public void InlineArrayInitilization()
        {
            ManaSet manaSet;

            manaSet = new ManaSet(
                ManaColour.White,
                ManaColour.Colourless,
                ManaColour.White,
                ManaColour.Green,
                ManaColour.Green,
                ManaColour.White,
                ManaColour.White,
                ManaColour.Purple,
                ManaColour.Blue,
                ManaColour.Red,
                ManaColour.Blue,
                ManaColour.Purple,
                ManaColour.Colourless,
                ManaColour.Green,
                ManaColour.Black,
                ManaColour.Colourless
                );
            Assert.AreEqual(3, manaSet.Green);
            Assert.AreEqual(2, manaSet.Blue);
            Assert.AreEqual(1, manaSet.Red);
            Assert.AreEqual(1, manaSet.Black);
            Assert.AreEqual(4, manaSet.White);
            Assert.AreEqual(2, manaSet.Purple);
            Assert.AreEqual(3, manaSet.Colourless);

            manaSet = new ManaSet();
            Assert.AreEqual(0, manaSet.Green);
            Assert.AreEqual(0, manaSet.Blue);
            Assert.AreEqual(0, manaSet.Red);
            Assert.AreEqual(0, manaSet.Black);
            Assert.AreEqual(0, manaSet.White);
            Assert.AreEqual(0, manaSet.Purple);
            Assert.AreEqual(0, manaSet.Colourless);

            manaSet = new ManaSet(ManaColour.Purple);
            Assert.AreEqual(0, manaSet.Green);
            Assert.AreEqual(0, manaSet.Blue);
            Assert.AreEqual(0, manaSet.Red);
            Assert.AreEqual(0, manaSet.Black);
            Assert.AreEqual(0, manaSet.White);
            Assert.AreEqual(1, manaSet.Purple);
            Assert.AreEqual(0, manaSet.Colourless);

        }

        [TestMethod]
        public void CopyValues()
        {
            var manaSet = new ManaSet();
            manaSet.Green = 1;
            manaSet.Blue = 2;
            manaSet.Red = 3;
            manaSet.Black = 4;
            manaSet.White = 5;
            manaSet.Purple = 6;
            manaSet.Colourless = 7;

            var manaSetCopy = new ManaSet();
            manaSetCopy.CopyValues(manaSet);

            Assert.AreEqual(manaSet.Green, manaSetCopy.Green);
            Assert.AreEqual(manaSet.Blue, manaSetCopy.Blue);
            Assert.AreEqual(manaSet.Red, manaSetCopy.Red);
            Assert.AreEqual(manaSet.Black, manaSetCopy.Black);
            Assert.AreEqual(manaSet.White, manaSetCopy.White);
            Assert.AreEqual(manaSet.Purple, manaSetCopy.Purple);
            Assert.AreEqual(manaSet.Colourless, manaSetCopy.Colourless);
        }

        [TestMethod]
        public void Subtract()
        {
            var manaSetA = new ManaSet();
            manaSetA.Green = 1;
            manaSetA.Blue = 2;
            manaSetA.Red = 3;
            manaSetA.Black = 4;
            manaSetA.White = 5;
            manaSetA.Purple = 6;
            manaSetA.Colourless = 7;

            var manaSetB = new ManaSet();
            manaSetB.Green = 1;
            manaSetB.Blue = 1;
            manaSetB.Red = 1;
            manaSetB.Black = 1;
            manaSetB.White = 1;
            manaSetB.Purple = 2;
            manaSetB.Colourless = 3;

            manaSetA.Subtract(manaSetB);

            Assert.AreEqual(0, manaSetA.Green);
            Assert.AreEqual(1, manaSetA.Blue);
            Assert.AreEqual(2, manaSetA.Red);
            Assert.AreEqual(3, manaSetA.Black);
            Assert.AreEqual(4, manaSetA.White);
            Assert.AreEqual(4, manaSetA.Purple);
            Assert.AreEqual(4, manaSetA.Colourless);
        }

        [TestMethod]
        public void ToIntArrayAndBack()
        {
            var manaSet = new ManaSet();
            manaSet.Green = 1;
            manaSet.Blue = 2;
            manaSet.Red = 3;
            manaSet.Black = 4;
            manaSet.White = 5;
            manaSet.Purple = 6;
            manaSet.Colourless = 7;

            var manaSetCopy = new ManaSet();
            var ints = manaSet.ToInts();
            manaSetCopy.FromInts(ints);

            Assert.AreEqual(manaSet.Green, manaSetCopy.Green);
            Assert.AreEqual(manaSet.Blue, manaSetCopy.Blue);
            Assert.AreEqual(manaSet.Red, manaSetCopy.Red);
            Assert.AreEqual(manaSet.Black, manaSetCopy.Black);
            Assert.AreEqual(manaSet.White, manaSetCopy.White);
            Assert.AreEqual(manaSet.Purple, manaSetCopy.Purple);
            Assert.AreEqual(manaSet.Colourless, manaSetCopy.Colourless);
        }
    }
}