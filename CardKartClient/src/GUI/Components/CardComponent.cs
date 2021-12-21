using CardKartShared.GameState;
using CardKartShared.Util;
using SGL;
using System.Drawing;

namespace CardKartClient.GUI.Components
{
    internal class CardComponent : GuiComponent
    {
        private float ImageInsetX;
        private float ImageInsetY;
        private float ImageWidth;
        private float ImageHeight;
        private float NameInsetX;
        private float NameInsetY;
        private float AttackInsetY;
        private float AttackInsetX;
        private float DefenceInsetY;
        private float DefenceInsetX;

        private Texture PortraitTexture;
        private Color PaletteColor;

        public Card Card;



        public CardComponent(Card card)
        {
            Card = card;

            SetScale(1);

            Layout();
        }

        private void Layout()
        {
            if (Card != null)
            {
                PortraitTexture = Textures.Portraits(Card.Template);
                PaletteColor = Constants.PaletteColors(Card.Colour);
            }
        }

        public void SetScale(float scale)
        {
            Width = 0.2f * scale;
            Height = 0.52f * scale;

            ImageInsetX = 0.009f * scale;
            ImageInsetY = 0.23f * scale;

            ImageWidth = 0.182f * scale;
            ImageHeight = 0.23f * scale;

            // Text doesn't really scale.
            // Yet.

            NameInsetX = 0.01f * scale;
            NameInsetY = 0.51f * scale;

            AttackInsetX = 0.015f;
            AttackInsetY = 0.06f;

            DefenceInsetX = 0.175f;
            DefenceInsetY = AttackInsetY;
        }

        public override void Draw(DrawAdapter drawAdapter)
        {
            drawAdapter.DrawSprite(
                X,
                Y,
                X + Width,
                Y + Height,
                Textures.Frame1_Monster,
                PaletteColor);

            drawAdapter.DrawSprite(
                X + ImageInsetX,
                Y + ImageInsetY,
                X + ImageInsetX + ImageWidth,
                Y + ImageInsetY + ImageHeight,
                PortraitTexture);

            drawAdapter.DrawText(Card.Name, X + NameInsetX, Y + NameInsetY);

            if (Card.Type == CardTypes.Monster)
            {
                drawAdapter.DrawText(
                    Card.Attack.ToString(), 
                    X + AttackInsetX, 
                    Y + AttackInsetY);

                drawAdapter.DrawText(
                    Card.Defence.ToString(), 
                    X + DefenceInsetX, 
                    Y + DefenceInsetY);
            }
        }
    }
}
