using CardKartShared.GameState;
using SGL;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static CardKartClient.GUI.Scenes.GameScene;

namespace CardKartClient.GUI.Components
{
    internal class CardChoicePanel : GuiComponent
    {
        public event HandComponent.CardClickedHandler CardClicked;

        public event RequestInfoDisplayHandler RequestInfoDisplay;

        public CardChoicePanel()
        {
            Width = 0.24f;
            Height = 1f;

        }

        public void Update(IEnumerable<Card> cards)
        {
            Components.Clear();

            if (cards == null || cards.Count() == 0)
            {
                return;
            }

            var dummy = new CardComponent(null);
            var cardHeight = dummy.Height;
            var cardWidth = dummy.Width;

            var y0 = Y + Height - cardHeight;

            int i = 0;
            foreach (var card in cards)
            {
                var component = new CardComponent(card);
                Components.Add(component);
                component.X = X + 0.01f;
                component.Y = y0 - i * 0.05f;
                component.Clicked += () => CardClicked?.Invoke(component);
                component.MouseEnteredEvent += () => RequestInfoDisplay?.Invoke(card);
                component.MouseExitedEvent += () => RequestInfoDisplay?.Invoke(null);

                i++;
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.Khaki);
        }
    }
}
