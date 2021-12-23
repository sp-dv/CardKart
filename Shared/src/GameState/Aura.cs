namespace CardKartShared.GameState
{
    public abstract class Aura
    {
        public abstract void ApplyAura(Token token, GameState gameState);
        public abstract bool IsCancelledBy(Trigger trigger, Token token, GameState gameState);
    }

    public class StandardBearerAura : Aura
    {
        public override void ApplyAura(Token token, GameState gameState)
        {
            foreach (var otherToken in gameState.AllTokens)
            {
                if (otherToken != token && otherToken.Controller == token.Controller)
                {
                    otherToken.AuraModifiers.Attack += 1;
                    otherToken.AuraModifiers.Health += 1;
                }
            }
        }

        public override bool IsCancelledBy(Trigger trigger, Token token, GameState gameState)
        {
            return false;
        }
    }

    public class TestAura : Aura
    {
        public override void ApplyAura(Token token, GameState gameState)
        {
            token.AuraModifiers.Attack += 2;
            token.AuraModifiers.Health += 2;
        }

        public override bool IsCancelledBy(Trigger trigger, Token token, GameState gameState)
        {
            if (trigger is GameTimeTrigger)
            {
                var gameTime = trigger as GameTimeTrigger;

                return gameTime.Time == GameTime.EndOfTurn;
            }
            return false;
        }
    }
}
