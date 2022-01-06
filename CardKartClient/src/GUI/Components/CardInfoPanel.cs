using CardKartShared.GameState;
using CardKartShared.Util;
using SGL;
using System.Drawing;
using System.Linq;

namespace CardKartClient.GUI.Components
{
    internal class CardInfoPanel : GuiComponent
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

        private string CardName = "";
        private string AttackString = "";
        private string HealthString = "";

        private Texture PortraitTexture;
        private Color PaletteColor;
        private Color[] ManaCostOrbColors;

        private string TypeString;
        private Color RarityColor;

        private SmartTextPanel BreadTextPanel;

        private Texture FrameTexture;

        public CardInfoPanel()
        {
            BreadTextPanel = new SmartTextPanel();
            BreadTextPanel.Font = Fonts.BreadTextFont14;
            BreadTextPanel.Width = 0.38f;
            BreadTextPanel.Height = 0.34f;
            BreadTextPanel.Alignment = QuickFont.QFontAlignment.Centre;
            BreadTextPanel.RenderOptions = Fonts.BigRenderOptions;

            Layout(null);
        }

        public void SetCard(Card card)
        {
            Layout(card);
        }

        public void ForceBreadText(string breadText)
        {
            BreadTextPanel.Text = breadText;
            BreadTextPanel.Layout();
        }

        private void Layout(Card card)
        {
            Visible = card != null;
            if (card == null) { return; }

            CardName = card.Name;

            FrameTexture = Textures.Frames(card.Type);
            PortraitTexture = Textures.Portraits(card.Template);
            PaletteColor = Constants.PaletteColor(card.Colour);
            ManaCostOrbColors =
                card.CastingCost.ToColourArray()
                .Select(colour => Constants.PaletteColor(colour)).ToArray();
            RarityColor = Constants.RarityColor(card.Rarity);

            if (card.Type == CardTypes.Creature)
            {
                TypeString = card.CreatureType.ToString();
                AttackString = card.Attack.ToString();
                HealthString = card.Health.ToString();
            }
            else
            {
                TypeString = card.Type.ToString();
                AttackString = HealthString = null;
            }

            BreadTextPanel.Text = card.BreadTextLong;
            BreadTextPanel.Layout();


            Width = 0.4f;
            Height = 1.04f;

            ImageInsetX = 0.025f;
            ImageInsetY = 0.47f;

            ImageWidth = 0.355f;
            ImageHeight = 0.45f;

            NameInsetX = 0.19f;
            NameInsetY = 1.005f;

            AttackInsetX = 0.045f;
            AttackInsetY = 0.6f;

            DefenceInsetX = 0.345f;
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
                X + 0.013f,
                Y + 0.867f,
                X + 0.045f,
                Y + 0.915f,
                Textures.Logo1stEdition,
                RarityColor);

            drawAdapter.DrawText(
                CardName,
                X + NameInsetX,
                Y + NameInsetY,
                Fonts.CardFont14,
                Fonts.MainRenderOptions,
                QuickFont.QFontAlignment.Centre);

            drawAdapter.DrawText(
                TypeString,
                X + 0.2f,
                Y + 0.51f,
                Fonts.CardFont14,
                Fonts.MainRenderOptions,
                QuickFont.QFontAlignment.Centre);

            BreadTextPanel.Draw(drawAdapter);

            if (AttackString != null && HealthString != null)
            {
                drawAdapter.DrawText(
                    AttackString,
                    X + AttackInsetX,
                    Y + AttackInsetY,
                    Fonts.CardFont20,
                    Fonts.MainRenderOptions,
                    QuickFont.QFontAlignment.Centre);

                drawAdapter.DrawText(
                    HealthString,
                    X + DefenceInsetX,
                    Y + DefenceInsetY,
                    Fonts.CardFont20,
                    Fonts.MainRenderOptions,
                    QuickFont.QFontAlignment.Centre);
            }

            var orbX0 = X + Width / 2 + 0.02f + (ManaCostOrbColors.Length * -0.02f);
            var orbY = Y + 0.412f;
            var radius = 0.02f;

            for (int i = 0; i < ManaCostOrbColors.Length; i++)
            {
                var color = ManaCostOrbColors[i];
                var orbX = orbX0 + (i * 0.043f);

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
