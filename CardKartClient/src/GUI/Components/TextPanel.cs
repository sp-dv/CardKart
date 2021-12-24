using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CardKartClient.GUI.Components
{
    internal class TextPanel : GuiComponent
    {
        public String Text;
        public Color? BackgroundColor { get; set; }
        public Texture BackgroundImage { get; set; }

        public float TextInsetX { get; set; }
        public float TextInsetY { get; set; }

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
            if (Text != null)
            {
                drawAdapter.DrawText(Text, X + TextInsetX, Y + TextInsetY);
            }
        }
    }
}
