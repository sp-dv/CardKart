using CardKartShared.GameState;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CardKartClient.GUI.Components
{
    internal class CombatAnimator : GuiComponent
    {
        public BattlefieldComponent BattlefieldA;
        public BattlefieldComponent BattlefieldB;

        private (TokenComponent, TokenComponent)[] BlockingPairs;

        public CombatAnimator(
            BattlefieldComponent battlefieldA, 
            BattlefieldComponent battlefieldB,
            GameController gameController)
        {
            BattlefieldA = battlefieldA;
            BattlefieldB = battlefieldB;

            gameController.RedrawAttackerAnimations += Update;
        }

        private void Update(Token[] attackers, (Token, Token)[] defenders)
        {
            Func<Token, TokenComponent> getTokenComponent = token =>
            {
                TokenComponent rt;

                rt = BattlefieldA.GetComponent(token);
                if (rt != null) { return rt; }

                rt = BattlefieldB.GetComponent(token);
                if (rt != null) { return rt; }

                return null;
            };

            BattlefieldA.ResetHighlighting();
            BattlefieldB.ResetHighlighting();

            if (attackers != null)
            {
                var attackersComponents =
                    attackers.Select(token => getTokenComponent(token)).ToArray();

                foreach (var attacker in attackersComponents)
                {
                    attacker.HighlightColor = Color.Green;
                }
            }

            if (defenders != null)
            {
                BlockingPairs = defenders.Select(pair =>
                    (getTokenComponent(pair.Item1), getTokenComponent(pair.Item2)))
                    .ToArray();
            }
            else
            {
                BlockingPairs = new (TokenComponent, TokenComponent)[0];
            }
        }


        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            if (BlockingPairs != null)
            {
                foreach (var pair in BlockingPairs)
                {
                    var blocker = pair.Item1;
                    var blocked = pair.Item2;

                    drawAdapter.DrawLine(
                        blocker.X + blocker.Width / 2, 
                        blocker.Y + blocker.Height / 2, 
                        blocked.X + blocked.Width / 2, 
                        blocked.Y + blocked.Height / 2, 
                        Color.Black);
                }
            }
        }
    }
}
