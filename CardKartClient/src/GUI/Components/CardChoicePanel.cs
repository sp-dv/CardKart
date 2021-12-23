using CardKartShared.GameState;
using SGL;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CardKartClient.GUI.Components
{
    internal class CardChoicePanel : GuiComponent
    {
        private CardComponent[] CardComponents;

        public event HandComponent.CardClickedHandler CardClicked;

        public CardChoicePanel()
        {
            Width = 0.24f;
            Height = 1f;
        }

        public void Update(IEnumerable<Card> cards)
        {
            if (cards == null || cards.Count() == 0)
            {
                CardComponents = new CardComponent[0];
                return;
            }

            var dummy = new CardComponent(null);
            var cardHeight = dummy.Height;
            var cardWidth = dummy.Width;

            var y0 = Y + Height - cardHeight;

            CardComponents = new CardComponent[cards.Count()];
            int i = 0;
            foreach (var card in cards)
            {
                var component = new CardComponent(card);
                CardComponents[i] = component;
                component.X = X + 0.01f;
                component.Y = y0 - i * 0.05f;
                
                i++;
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.Khaki);

            if (CardComponents != null)
            {
                foreach (var cardComponent in CardComponents)
                {
                    cardComponent?.Draw(drawAdapter);
                }
            }
        }

        protected override void HandleClickInternal(GLCoordinate location)
        {
            for (int i = CardComponents.Length - 1; i >= 0; i--)
            {
                var component = CardComponents[i];
                if (component.HandleClick(location))
                {
                    CardClicked?.Invoke(component);
                    return;
                }
            }
        }
    }
}
