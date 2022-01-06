using CardKartShared.GameState;
using SGL;
using System.Drawing;
using System.Linq;
using static CardKartClient.GUI.Scenes.GameScene;

namespace CardKartClient.GUI.Components
{
    internal class HandComponent : GuiComponent
    {
        private float PaddingY = 0.02f;

        private Pile Hand;

        public delegate void CardClickedHandler(CardComponent cardComponent);
        public event CardClickedHandler CardClicked;

        public event RequestInfoDisplayHandler RequestInfoDisplay;

        public HandComponent(Pile hand)
        {
            Width = 1.1f;
            Height = 0.56f;

            Hand = hand;
            Hand.PileChanged += Layout;

            Layout();
        }

        private void Layout()
        {
            lock (Components)
            {
                var cards = Hand.ToArray();

                Components.Clear();

                for (int i = 0; i < cards.Length; i++)
                {
                    var card = cards[i];
                    var cardComponent = new CardComponent(card);
                    Components.Add(cardComponent);

                    cardComponent.X = X + i * cardComponent.Width;
                    cardComponent.Y = Y + PaddingY;
                    cardComponent.Clicked += () => CardClicked?.Invoke(cardComponent);
                    cardComponent.MouseEnteredEvent += () => RequestInfoDisplay?.Invoke(card);
                    cardComponent.MouseExitedEvent += () => RequestInfoDisplay?.Invoke(null);
                }
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.SaddleBrown);
        }
    }
}
