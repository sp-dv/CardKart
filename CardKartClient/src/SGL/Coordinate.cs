using System;

namespace SGL
{
    public class GLCoordinate
    {
        public float X;
        public float Y;

        public GLCoordinate()
        {
        }

        public GLCoordinate(float x, float y)
        {
            X = x;
            Y = y;
        }

        public bool InBounds(float x0, float y0, float x1, float y1)
        {
            return
                X >= x0 &&
                X <= x1 &&
                Y >= y0 &&
                Y <= y1;
        }
    }

    public class ScreenCoordinate
    {
        public float X;
        public float Y;

        public GLCoordinate ToGL()
        {
            var rt = new GLCoordinate();
            rt.X = X * 2 - 1;
            rt.Y = Y * -2 + 1;
            return rt;
        }
    }
}
