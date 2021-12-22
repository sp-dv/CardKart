using System;
using System.Collections.Generic;
using System.Text;

namespace CardKartShared.GameState
{
    public class Token : GameObject
    {
        public Card TokenOf;

        public int Damage;

        public int Attack => TokenOf.Attack;
        public int Defence => TokenOf.Defence - Damage;
    }
}
