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

        public override void Draw(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectange(X, Y, X + Width, Y + Height, Colour);
            drawAdapter.DrawRectange(X, Y, X + Width,Y + Height, Colour);
            drawAdapter.DrawText(Text, X, Y + Height/2);
        }

        protected override bool HandleClickInternal(GLCoordinate location)
        {
            Clicked?.Invoke();
            return true;
        }
    }
}
