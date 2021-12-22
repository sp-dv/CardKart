using SGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardKartClient.GUI
{
    internal abstract class GuiComponent
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public bool Visible { get; set; } = true;

        public void Draw(DrawAdapter drawAdapter)
        {
            if (Visible)
            {
                DrawInternal(drawAdapter);
            }
        }

        protected abstract void DrawInternal(DrawAdapter drawAdapter);

        public bool HandleClick(GLCoordinate location)
        {
            if (!Visible) { return false; }
            if (!ComponentRectangleContains(location)) { return false; }

            return HandleClickInternal(location);
        }

        protected virtual bool HandleClickInternal(GLCoordinate location)
        {
            return false;
        }

        public bool ComponentRectangleContains(GLCoordinate location)
        {
            return location.InBounds(X, Y, X + Width, Y + Height);
        }
    }
}
