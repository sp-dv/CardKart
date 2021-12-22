using CardKartShared.GameState;
using SGL;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CardKartClient.GUI.Components
{
    internal class BattlefieldComponent : GuiComponent
    {
        private float PaddingY = 0.02f;

        private Pile Battlefield;
        private List<TokenComponent> TokenComponents = new List<TokenComponent>();
        private object TokenComponentsLock = new object();

        public delegate void TokenClickedHandler(TokenComponent cardComponent);
        public event TokenClickedHandler CardClicked;

        public BattlefieldComponent(Pile battlefield)
        {
            Width = 0.7f;
            Height = 0.56f;

            Battlefield = battlefield;
            Battlefield.PileChanged += Layout;

            Layout();

        }

        private void Layout()
        {
            lock (TokenComponentsLock)
            {
                var cards = Battlefield.ToArray();
                TokenComponents.Clear();

                for (int i = 0; i < cards.Length; i++)
                {
                    var card = cards[i];
                    var tokenComponent = new TokenComponent(card);
                    TokenComponents.Add(tokenComponent);

                    tokenComponent.X = X + i * tokenComponent.Width;
                    tokenComponent.Y = Y;
                }
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            lock (TokenComponentsLock)
            {
                if (TokenComponents == null) { return; }

                drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.SandyBrown);

                foreach (var tokenComponent in TokenComponents)
                {
                    tokenComponent.Draw(drawAdapter);
                }
            }
        }

        protected override bool HandleClickInternal(GLCoordinate location)
        {
            return true;
        }
    }
}
