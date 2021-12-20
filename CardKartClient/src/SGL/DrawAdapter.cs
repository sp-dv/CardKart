using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using QuickFont;

namespace SGL
{
    public class DrawAdapter
    {
        public QFontDrawing FontDrawing;
        public QFont DefaultFont;
        public QFontRenderOptions RenderOptions;

        public int ScreenWidth;
        public int ScreenHeight;

        public DrawAdapter()
        {
        }

        public void Translate(float x, float y)
        {
            GL.Translate(x, y, 0);
        }

        public void Rotate(float theta)
        {
            GL.Rotate(theta, 0, 0, 1);
        }

        public void PushMatrix()
        {
            GL.PushMatrix();
        }

        public void PopMatrix()
        {
            GL.PopMatrix();
        }

        public void DrawLine(float x0, float y0, float x1, float y1, Color c)
        {
            GL.LineWidth(4);
            GL.Color4(c);
            GL.Begin(BeginMode.Lines);

            GL.Vertex2(x0, y0);
            GL.Vertex2(x1, y1);

            GL.End();
        }

        public void FillRectange(float x0, float y0, float x1, float y1, Color c)
        {
            GL.Color4(c);

            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(x0, y0);
            GL.Vertex2(x1, y0);
            GL.Vertex2(x1, y1);
            GL.Vertex2(x0, y1);
            GL.End();
        }

        public void DrawRectange(float x0, float y0, float x1, float y1, Color c, int width = 1)
        {
            GL.Color4(c);
            GL.LineWidth(width);

            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex2(x0, y0);
            GL.Vertex2(x1, y0);
            GL.Vertex2(x1, y1);
            GL.Vertex2(x0, y1);
            GL.End();
        }

        public void FillCircle(float radius, Color color)
        {
            float x1 = 0;
            float y1 = 0;
            float x2;
            float y2;
            float angle;


            GL.Begin(BeginMode.TriangleFan);
            GL.Color3(color);

            GL.Vertex2(0, 0);

            for (angle = 1.0f; angle < 361.0f; angle += 3f)
            {
                x2 = (float)Math.Sin(angle) * radius;
                y2 = (float)Math.Cos(angle) * radius;
                GL.Vertex2(x1 + x2, y1 + y2);
            }

            GL.End();
        }

        public void DrawCircle(float radius, Color color)
        {
            float x0 = 0;
            float y0 = 0;
            int num_segments = 10;

            GL.Begin(BeginMode.LineLoop);
            GL.Color3(color);

            for (int i = 0; i < num_segments; i++)
            {
                var theta = 2.0f * Math.PI * i / num_segments;

                double x = radius * Math.Cos(theta);
                double y = radius * Math.Sin(theta);

                GL.Vertex2(x + x0, y + y0);

            }
            GL.End();
        }

        public void DrawSprite(float x0, float y0, float x1, float y1, Texture texture, Color? brushColor = null)
        {
            if (!texture.Loaded) { return; }

            GL.Enable(EnableCap.Texture2D);

            if (brushColor.HasValue)
            {
                GL.Color4(brushColor.Value);
            }
            else
            {
                GL.Color4(Color.White);
            }

            GL.BindTexture(TextureTarget.Texture2D, texture.ID);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 1);
            GL.Vertex2(x0, y0);

            GL.TexCoord2(0, 0);
            GL.Vertex2(x0, y1);

            GL.TexCoord2(1, 0);
            GL.Vertex2(x1, y1);

            GL.TexCoord2(1, 1);
            GL.Vertex2(x1, y0);

            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }

        public void DrawBar(float x0, float y0, float x1, float y1, float pct, Color back, Color bar)
        {
            FillRectange(x0, y0, x1, y1, back);
            pct = Math.Min(pct, 1);
            pct = Math.Max(pct, 0);
            var barWidth = (x1-x0) * pct;
            FillRectange(x0, y0, x0 + barWidth, y1, bar);
        }

        public void DrawText(string text, float X0, float Y0)
        {
            var w = (X0 / 2 + 0.5f) * ScreenWidth;
            var h = (Y0 / 2 + 0.5f) * ScreenHeight;
            FontDrawing.Print(
                DefaultFont, 
                text, 
                new OpenTK.Vector3(w, h, 0),
                QFontAlignment.Left, 
                RenderOptions);
        }
    }
}
