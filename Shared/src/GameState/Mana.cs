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

        public ManaSet()
        {
        }

        public ManaSet(ManaSet other)
        {
            Red = other.Red;
            Black = other.Black;
            Green = other.Green;
            Blue = other.Blue;
            White = other.White;
            Purple = other.Purple;
            Colourless = other.Colourless;
        }

        public ManaSet(params ManaColour[] colours)
        {
            foreach (var colour in colours)
            {
                switch (colour)
                {
                    case ManaColour.Red: { Red++; break; }
                    case ManaColour.Black: { Black++; break; }
                    case ManaColour.Green: { Green++; break; }
                    case ManaColour.Blue: { Blue++; break; }
                    case ManaColour.White: { White++; break; }
                    case ManaColour.Purple: { Purple++; break; }
                    case ManaColour.Colourless: { Colourless++; break; }
                }
            }
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
