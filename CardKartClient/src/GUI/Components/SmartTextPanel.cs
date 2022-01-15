using QuickFont;
using SGL;
using System.Drawing;

namespace CardKartClient.GUI.Components
{
    internal class SmartTextPanel : GuiComponent
    {
        public string Text { get; set; }
        ProcessedText ProcessedText;

        private float TextX0;
        private float TextY0;

        public float TextOffsetY { get; set; }

        public QFont Font { get; set; }
        public QFontRenderOptions RenderOptions { get; set; }

        public QFontAlignment Alignment = QFontAlignment.Left;

        public Color? BackgroundColor { get; set; }
        public Texture BackgroundImage { get; set; }

        public SmartTextPanel()
        {
            Width = 0.2f;
            Height = 0.2f;

            Font = Fonts.MainFont10;
            RenderOptions = Fonts.MainRenderOptions;
        }

        public void Layout()
        {
            if (Text == null ||Font == null || RenderOptions == null) { return; }

            var sw = (Width / 2) * CardKartClient.GUI.WindowWidth; // Width in terms of pixels.
            var sh = (Height / 2) * CardKartClient.GUI.WindowHeight; // Height in terms of pixels

            ProcessedText = QFontDrawingPimitive.ProcessText(Font, RenderOptions, Text, new SizeF(sw, sh), Alignment);
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            if (BackgroundColor.HasValue)
            {
                drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, BackgroundColor.Value);
            }

            if (BackgroundImage != null)
            {
                drawAdapter.DrawSprite(X, Y, X + Width, Y + Height, BackgroundImage);
            }

            if (ProcessedText != null)
            {
                // I hate this.
                var sw = (Width / 2) * CardKartClient.GUI.WindowWidth; // Width in terms of pixels.
                var sh = (Height / 2) * CardKartClient.GUI.WindowHeight; // Height in terms of pixels
                TextX0 = (X / 2 + 0.5f) * CardKartClient.GUI.WindowWidth;
                TextY0 = (Y / 2 + 0.5f) * CardKartClient.GUI.WindowHeight + sh + TextOffsetY;

                if (Alignment == QFontAlignment.Centre)
                {
                    TextX0 += sw / 2;
                }

                drawAdapter.DrawTextSmart(ProcessedText, TextX0, TextY0, Font, RenderOptions);
            }
        }
    }
}
