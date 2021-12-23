using CardKartShared.GameState;
using CardKartShared.Util;
using SGL;
using System.Drawing;
using System.Linq;

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
        private Color[] ManaCostOrbColors;

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
                PaletteColor = Constants.PaletteColor(Card.Colour);
                ManaCostOrbColors = 
                    Card.CastingCost.ToColourArray()
                    .Select(colour => Constants.PaletteColor(colour)).ToArray();
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

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {

            drawAdapter.DrawSprite(
                X + ImageInsetX,
                Y + ImageInsetY,
                X + ImageInsetX + ImageWidth,
                Y + ImageInsetY + ImageHeight,
                PortraitTexture);
            
            drawAdapter.DrawSprite(
                X,
                Y,
                X + Width,
                Y + Height,
                Textures.Frame1_Monster,
                PaletteColor);

            drawAdapter.DrawText(Card.Name, X + NameInsetX, Y + NameInsetY);
            if (Card.BreadText != null)
            {
                drawAdapter.DrawText(Card.BreadText, X + 0.012f, Y + 0.17f);
            }

            if (Card.Type == CardTypes.Creature)
            {
                drawAdapter.DrawText(
                    Card.Attack.ToString(), 
                    X + AttackInsetX, 
                    Y + AttackInsetY);

                drawAdapter.DrawText(
                    Card.Health.ToString(), 
                    X + DefenceInsetX, 
                    Y + DefenceInsetY);
            }

            var orbX0 = X + Width / 2 + 0.011f + (ManaCostOrbColors.Length * -0.011f);
            var orbY = Y + 0.207f;
            var radius = 0.01f;

            for (int i = 0; i < ManaCostOrbColors.Length; i++)
            {
                var color = ManaCostOrbColors[i];
                var orbX = orbX0 + (i * 0.023f);

                drawAdapter.FillCircle(
                    orbX, 
                    orbY, 
                    radius, 
                    color);

                drawAdapter.DrawCircle(
                    orbX,
                    orbY,
                    radius,
                    Color.Black);
            }
        }
    }
}
