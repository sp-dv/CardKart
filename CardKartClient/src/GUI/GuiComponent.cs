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

        public abstract void Draw(DrawAdapter drawAdapter);

        public bool HandleClick(GLCoordinate location)
        {
            if (!InComponentRectangle(location)) { return false; }

            return HandleClickInternal(location);
        }

        protected virtual bool HandleClickInternal(GLCoordinate location)
        {
            return false;
        }

        public bool InComponentRectangle(GLCoordinate location)
        {
            return location.InBounds(X, Y, X + Width, Y + Height);
        }
    }
}
