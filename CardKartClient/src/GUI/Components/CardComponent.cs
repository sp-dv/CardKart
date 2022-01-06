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

        private string TypeString;
        private Color RarityColor;

        private SmartTextPanel BreadTextPanel;

        private Texture FrameTexture;

        public CardComponent(Card card)
        {
            Card = card;

            BreadTextPanel = new SmartTextPanel();
            BreadTextPanel.Font = Fonts.BreadTextFont10;
            BreadTextPanel.Width = 0.18f;
            BreadTextPanel.Height = 0.16f;
            BreadTextPanel.Alignment = QuickFont.QFontAlignment.Centre;

            Layout();
        }

        public void ForceBreadText(string breadText)
        {
            BreadTextPanel.Text = breadText;
            BreadTextPanel.Layout();
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
                RarityColor = Constants.RarityColor(Card.Rarity);

                if (Card.Type == CardTypes.Creature)
                {
                    TypeString = Card.CreatureType.ToString();
                }
                else
                {
                    TypeString = Card.Type.ToString();
                }

                BreadTextPanel.Text = Card.BreadText;
                if (Card.BreadText.Length < 80)
                {
                    BreadTextPanel.Font = Fonts.BreadTextFont10;
                }
                else
                {
                    BreadTextPanel.Font = Fonts.BreadTextFont8;
                }
                BreadTextPanel.Layout();
            }


            Width = 0.2f;
            Height = 0.52f;

            ImageInsetX = 0.009f;
            ImageInsetY = 0.23f;

            ImageWidth = 0.182f;
            ImageHeight = 0.23f;

            NameInsetX = 0.095f;
            NameInsetY = 0.51f;

            AttackInsetX = 0.0155f;
            AttackInsetY = 0.29f;

            DefenceInsetX = 0.165f;
            DefenceInsetY = AttackInsetY;
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

            drawAdapter.DrawSprite(
                X + 0.005f,
                Y + 0.433f,
                X + 0.025f,
                Y + 0.4562f,
                Textures.Logo1stEdition,
                RarityColor);

            drawAdapter.DrawText(
                Card.Name, 
                X + NameInsetX, 
                Y + NameInsetY, 
                Card.Name.Length < 100 ? Fonts.CardFont10 : Fonts.CardFont8, 
                Fonts.MainRenderOptions,
                QuickFont.QFontAlignment.Centre);

            drawAdapter.DrawText(
                TypeString,
                X + 0.1f,
                Y + 0.265f,
                Fonts.CardFont10,
                Fonts.MainRenderOptions,
                QuickFont.QFontAlignment.Centre);

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
                drawAdapter.DrawRectangle(X, Y, X + Width, Y + Height, Color.White);
            }
        }
    }
}
