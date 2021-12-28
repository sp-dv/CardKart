using CardKartShared.GameState;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CardKartClient.GUI.Components
{
    internal class PileDisplayPanel : GuiComponent
    {
        // Absolutely repulsive hack to get card to draw on top.

        private Pile Pile;

        public delegate void CardClickedHandler(CardComponent card);
        public event CardClickedHandler CardClicked;

        public PileDisplayPanel(Pile pile)
        {
            Width = 0.24f;
            Height = 1f;

            Pile = pile;
            Pile.PileChanged += Update;
        }

        private void Update()
        {
            var dummy = new CardComponent(null);
            var cardHeight = dummy.Height;
            var cardWidth = dummy.Width;

            var y0 = Y + Height - cardHeight;

            var cards = Pile.ToArray();

            lock (Components)
            {
                Components.Clear();
                for (int i = 0; i < cards.Length; i++)
                {
                    var card = cards[i];
                    var component = new CardComponent(card);
                    Components.Add(component);
                    component.X = X + 0.01f;
                    component.Y = y0 - i * 0.05f;
                    component.Clicked += () => CardClicked?.Invoke(component);
                }
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.Silver);
        }
    }
}
