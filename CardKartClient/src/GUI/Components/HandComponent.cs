using CardKartClient.GUI;
using CardKartShared.GameState;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardKartClient.GUI.Components
{
    internal class HandComponent : GuiComponent
    {
        private float PaddingY = 0.02f;

        private Pile Hand;
        private List<CardComponent> CardComponents = new List<CardComponent>();
        private object CardComponentsLock = new object();

        public delegate void CardClickedHandler(CardComponent cardComponent);
        public event CardClickedHandler CardClicked;

        public HandComponent(Pile hand)
        {
            Width = 0.7f;
            Height = 0.56f;

            Hand = hand;
            Hand.PileChanged += Layout;

            Layout();
        }

        private void Layout()
        {
            lock (CardComponentsLock)
            {
                var cards = Hand.ToArray();

                CardComponents.Clear();

                for (int i = 0; i < cards.Length; i++)
                {
                    var card = cards[i];
                    var cardComponent = new CardComponent(card);
                    CardComponents.Add(cardComponent);

                    cardComponent.X = X + i * cardComponent.Width;
                    cardComponent.Y = Y + PaddingY;
                }
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            lock (CardComponentsLock)
            {
                if (CardComponents == null) { return; }

                drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.SaddleBrown);

                foreach (var cardComponent in CardComponents)
                {
                    cardComponent.Draw(drawAdapter);
                }
            }
        }

        protected override void HandleClickInternal(GLCoordinate location)
        {
            foreach (var cardComponent in CardComponents)
            {
                if (cardComponent.ComponentRectangleContains(location))
                {
                    CardClicked?.Invoke(cardComponent);
                }
            }
        }
    }
}
