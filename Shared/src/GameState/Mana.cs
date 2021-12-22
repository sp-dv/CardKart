using System;
using System.Collections.Generic;
using System.Text;

namespace CardKartShared.GameState
{
    public class ManaSet
    {
        public int Red;
        public int Black;
        public int Green;
        public int Blue;
        public int White;
        public int Purple;
        public int Colourless;

        public int Size => Red + Black + Green + Blue + White + Purple + Colourless;

        public ManaSet()
        {
        }

        public ManaSet(ManaSet other)
        {
            CopyValues(other);
        }

        public ManaSet(params ManaColour[] colours)
        {
            foreach (var colour in colours)
            {
                IncrementColour(colour);
            }
        }

        public void IncrementColour(ManaColour colour, int value = 1)
        {
            switch (colour)
            {
                case ManaColour.Red: { Red += 1; break; }
                case ManaColour.Black: { Black += 1; break; }
                case ManaColour.Green: { Green += 1; break; }
                case ManaColour.Blue: { Blue += 1; break; }
                case ManaColour.White: { White += 1; break; }
                case ManaColour.Purple: { Purple += 1; break; }
                case ManaColour.Colourless: { Colourless += 1; break; }
            }
        }

        public void CopyValues(ManaSet other)
        {
            Red = other.Red;
            Black = other.Black;
            Green = other.Green;
            Blue = other.Blue;
            White = other.White;
            Purple = other.Purple;
            Colourless = other.Colourless;
        }

        public bool Covers(ManaSet other)
        {
            var excessManaCount = 0;

            if (other.Red > Red) { return false; }
            excessManaCount += Red - other.Red;

            if (other.Black > Black) { return false; }
            excessManaCount += Black - other.Black;

            if (other.Green > Green) { return false; }
            excessManaCount += Green - other.Green;

            if (other.Blue > Blue) { return false; }
            excessManaCount += Blue - other.Blue;

            if (other.White > White) { return false; }
            excessManaCount += White - other.White;

            if (other.Purple > Purple) { return false; }
            excessManaCount += Purple - other.Purple;

            return Colourless + excessManaCount >= other.Colourless;
        }

        public void Subtract(ManaSet other)
        {
            Red -= other.Red;
            Green -= other.Green;
            Blue -= other.Blue;
            White -= other.White;
            Purple -= other.Purple;
            Black -= other.Black;
            Colourless -= other.Colourless;
        }

        public int[] ToInts()
        {
            return new[] { Red, Black, Green, Blue, White, Purple, Colourless};
        }

        public void FromInts(int[] values)
        {
            if (values.Length != 7) { throw new NotImplementedException(); }

            Red = values[0];
            Black = values[1];
            Green = values[2];
            Blue = values[3];
            White = values[4];
            Purple = values[5];
            Colourless = values[6];
        }

        public List<ManaColour> ToColourArray()
        {
            var colours = new List<ManaColour>();

            for (int i = 0; i < Red; i++) { colours.Add(ManaColour.Red); }
            for (int i = 0; i < Black; i++) { colours.Add(ManaColour.Black); }
            for (int i = 0; i < Green; i++) { colours.Add(ManaColour.Green); }
            for (int i = 0; i < Blue; i++) { colours.Add(ManaColour.Blue); }
            for (int i = 0; i < White; i++) { colours.Add(ManaColour.White); }
            for (int i = 0; i < Purple; i++) { colours.Add(ManaColour.Purple); }
            for (int i = 0; i < Colourless; i++) { colours.Add(ManaColour.Colourless); }

            return colours;
        }
    }

    public enum ManaColour
    {
        None,

        Red,
        Black,
        Green,
        Blue,
        White,
        Purple,

        Colourless,
        Mixed,
    }

}
