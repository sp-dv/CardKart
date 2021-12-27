using CardKartShared.GameState;
using CardKartShared.Util;
using SGL;
using System;
using System.Drawing;

namespace CardKartClient.GUI.Components
{
    internal class ManaBallButton : GuiComponent
    {
        public ManaColour Colour;
        public string Text;

        private float Radius;
        private Color FillColor;

        public ManaBallButton(ManaColour colour)
        {
            Colour = colour;
            FillColor = Constants.PaletteColor(Colour);

            Width = 0.05f;
            Height = 0.05f;

            Radius = Width / 2;
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            if (Colour == ManaColour.None) { return; }

            drawAdapter.FillCircle(X + Radius, Y + Radius, Radius, FillColor);
            drawAdapter.DrawCircle(X + Radius, Y + Radius, Radius, Color.Black);
            
            if (Text != null)
            {
                // Single digit
                //drawAdapter.DrawText(Text, X + 0.022f, Y + 0.045f);

                // X/Y format
                drawAdapter.DrawText(Text, X + 0.008f, Y + 0.059f, Fonts.MainFont10, Fonts.MainRenderOptions);
            }
        }
    }
}
