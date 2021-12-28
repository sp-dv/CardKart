
using CardKartShared.GameState;
using SGL;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CardKartClient.GUI.Components
{
    internal class ShowCardsPanel : GuiComponent
    {
        public delegate void CardClickedHandler(CardComponent card);
        public event CardClickedHandler CardClicked;

        public ShowCardsPanel(IEnumerable<CardTemplates> templates)
        {
            Width = 0.24f;
            Height = 1f;

            var dummy = new CardComponent(null);
            var cardHeight = dummy.Height;
            var cardWidth = dummy.Width;

            var y0 = Y + Height - cardHeight;

            var cards = templates.Select(template => new Card(template)).ToArray();

            for (int i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                var component = new CardComponent(card);
                Components.Add(component);
                component.X = X + 0.01f;
                component.Y = y0 - i * 0.05f;
                component.Clicked += () => CardClicked?.Invoke(component);
            }

            var closeButton = new SmartTextPanel();
            closeButton.X = X;
            closeButton.Width = Width;
            closeButton.Height = 0.1f;
            closeButton.Text = "Close";
            closeButton.BackgroundColor = Color.DarkRed;
            closeButton.Clicked += () => Visible = false;
            Components.Add(closeButton);
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.Silver);
        }
    }
}
