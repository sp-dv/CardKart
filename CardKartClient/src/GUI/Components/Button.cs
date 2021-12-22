using SGL;
using System.Drawing;

namespace CardKartClient.GUI.Components
{
    internal class Button : GuiComponent
    {
        public Color Colour { get; set; } = Color.Pink;
        public string Text { get; set; }

        public delegate void ClickedHandler();
        public event ClickedHandler Clicked;

        public Button()
        {
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Colour);
            drawAdapter.DrawRectange(X, Y, X + Width,Y + Height, Colour);
            drawAdapter.DrawText(Text, X + 0.02f, Y + Height/2 + 0.02f);
        }

        protected override bool HandleClickInternal(GLCoordinate location)
        {
            Clicked?.Invoke();
            return true;
        }
    }
}
