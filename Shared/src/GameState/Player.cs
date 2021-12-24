﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardKartShared.GameState
{
    public class Player : GameObject
    {
        public Pile Hand { get; } = new Pile(PileLocation.Hand);
        public Pile Deck { get; } = new Pile(PileLocation.Deck);
        public Pile Battlefield { get; } = new Pile(PileLocation.Battlefield);
        public Pile Graveyard { get; } = new Pile(PileLocation.Graveyard);
        public Pile Stack { get; } = new Pile(PileLocation.Stack);

        public ManaSet CurrentMana { get; } = new ManaSet();
        public ManaSet MaxMana { get; } = new ManaSet();

        public Card HeroCard { get; set; }

        public Player Opponent { get; set; }

        public delegate void PlayerChangedHandler();
        public event PlayerChangedHandler PlayerChanged;

        public void NotifyOfChange()
        {
            PlayerChanged?.Invoke();
        }
    }
}
