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

        private SmartTextPanel BreadTextPanel;

        private Texture FrameTexture;

        public CardComponent(Card card)
        {
            Card = card;

            BreadTextPanel = new SmartTextPanel();
            BreadTextPanel.Font = Fonts.CardFont8;
            BreadTextPanel.Width = 0.18f;
            BreadTextPanel.Height = 0.16f;
            BreadTextPanel.Alignment = QuickFont.QFontAlignment.Centre;

            Layout();
        }

        private void Layout()
        {
            if (Card != null)
            {
                FrameTexture = Textures.Frames(Card.Type);
                PortraitTexture = Textures.Portraits(Card.Template);
                PaletteColor = Constants.PaletteColor(Card.Colour);
                ManaCostOrbColors = 
                    Card.CastingCost.ToColourArray()
                    .Select(colour => Constants.PaletteColor(colour)).ToArray();

                BreadTextPanel.Text = Card.BreadText;
                BreadTextPanel.Layout();
            }


            Width = 0.2f;
            Height = 0.52f;

            ImageInsetX = 0.009f;
            ImageInsetY = 0.23f;

            ImageWidth = 0.182f;
            ImageHeight = 0.23f;

            NameInsetX = 0.01f;
            NameInsetY = 0.51f;

            AttackInsetX = 0.0155f;
            AttackInsetY = 0.29f;

            DefenceInsetX = 0.165f;
            DefenceInsetY = AttackInsetY;
        }

        public void SetScale()
        {
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            BreadTextPanel.X = X + 0.01f;
            BreadTextPanel.Y = Y + 0.03f;


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
                FrameTexture,
                PaletteColor);

            drawAdapter.DrawText(Card.Name, X + NameInsetX, Y + NameInsetY, Fonts.CardFont10, Fonts.MainRenderOptions);

            BreadTextPanel.Draw(drawAdapter);

            if (Card.Type == CardTypes.Creature)
            {
                drawAdapter.DrawText(
                    Card.Attack.ToString(), 
                    X + AttackInsetX, 
                    Y + AttackInsetY, 
                    Fonts.CardFont14, 
                    Fonts.MainRenderOptions);

                drawAdapter.DrawText(
                    Card.Health.ToString(), 
                    X + DefenceInsetX, 
                    Y + DefenceInsetY, 
                    Fonts.CardFont14, 
                    Fonts.MainRenderOptions);
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

            if (MouseIsInComponent)
            {
                drawAdapter.DrawRectange(X, Y, X + Width, Y + Height, Color.White);
            }
        }

        protected override void MouseEntered(GLCoordinate location)
        {
        }
    }
}
