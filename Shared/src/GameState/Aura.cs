using System;

namespace CardKartShared.GameState
{
    public class Aura
    {
        public string BreadText { get; set; }

        public Action<Token, GameState> ApplyAura = (token, gameState) => { };
        public Func<Trigger, Token, GameState, bool> IsCancelledBy = (trigger, token, gameState) => false;
    }
}
