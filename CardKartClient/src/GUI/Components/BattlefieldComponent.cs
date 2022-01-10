using CardKartShared.GameState;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static CardKartClient.GUI.Scenes.GameScene;

namespace CardKartClient.GUI.Components
{
    internal class BattlefieldComponent : GuiComponent
    {
        private float PaddingY = 0.02f;

        private Pile Battlefield;

        public delegate void TokenClickedHandler(Token token);
        public event TokenClickedHandler TokenClicked;

        public event RequestInfoDisplayHandler RequestInfoDisplay; 

        public BattlefieldComponent(Pile battlefield)
        {
            Width = 1.15f;
            Height = 0.3f;

            Battlefield = battlefield;
            Battlefield.PileChanged += Layout;

            Layout();

        }

        public TokenComponent GetComponent(int id)
        {
            foreach (TokenComponent tokenComponent in Components)
            {
                if (tokenComponent?.Card?.ID == id ||
                    tokenComponent?.Card?.Token?.ID == id)
                {
                    return tokenComponent;
                }
            }

            return null;
        }

        public void ResetHighlighting()
        {
            foreach (TokenComponent component in Components)
            {
                component.HighlightColor = null;
            }
        }

        private void Layout()
        {
            lock (Components)
            {
                Components.Clear();

                var cards = Battlefield.ToArray();

                for (int i = 0; i < cards.Length; i++)
                {
                    var card = cards[i];
                    var tokenComponent = new TokenComponent(card);
                    tokenComponent.X = X + i * (tokenComponent.Width + 0.01f);
                    tokenComponent.Y = Y + 0.01f;
                    tokenComponent.Clicked += () => { TokenClicked?.Invoke(card.Token); };
                    tokenComponent.MouseEnteredEvent += () => RequestInfoDisplay?.Invoke(card);
                    tokenComponent.MouseExitedEvent += () => RequestInfoDisplay?.Invoke(null);

                    Components.Add(tokenComponent);
                }
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.SandyBrown);
        }
    }
}
