using CardKartShared.GameState;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CardKartClient.GUI.Components
{
    internal class StackPanel : GuiComponent
    {
        private CardComponent[] CardComponents;

        public StackPanel(CastingStack stack)
        {
            Width = 0.24f;
            Height = 1f;

            stack.StackChanged += Update;
        }

        private void Update(Card[] cards)
        {
            var dummy = new CardComponent(null);
            var cardHeight = dummy.Height;
            var cardWidth = dummy.Width;

            var y0 = Y + Height - cardHeight;

            CardComponents = new CardComponent[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                var component = 
                    new CardComponent(cards[cards.Length - i - 1]);
                CardComponents[i] = component;
                component.X = X + 0.01f;
                component.Y = y0 - i * 0.05f;
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.Silver);

            if (CardComponents != null)
            {
                foreach (var cardComponent in CardComponents)
                {
                    cardComponent.Draw(drawAdapter);
                }
            }
        }
    }
}
