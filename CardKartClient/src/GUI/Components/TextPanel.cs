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

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            if (BackgroundColor.HasValue)
            {
                drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, BackgroundColor.Value);
            }
            if (Text != null)
            {
                drawAdapter.DrawText(Text, X, Y);
            }
        }
    }
}
