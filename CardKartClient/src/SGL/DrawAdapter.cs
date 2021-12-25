using System;
using System.Collections.Generic;
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

        private static float[] CircleSins;
        private static float[] CircleCoss;

        static DrawAdapter()
        {
            var sins = new List<float>();
            var coss = new List<float>();
            // Add 0.09 because of some dumb floating math error. This
            // ends up being the entire circle. Or maybe this is because
            // we have to close the loop so it's the circle plus the first
            // point twice. Who knows.
            for (double angle = 0; angle <= Math.PI*2 + 0.09; angle += 0.1)
            {
                sins.Add((float)Math.Sin(angle));
                coss.Add((float)Math.Cos(angle));
            }
            CircleSins = sins.ToArray();
            CircleCoss = coss.ToArray();
        }

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
            GL.Color4(c);
            GL.Begin(PrimitiveType.Lines);

            GL.Vertex2(x0, y0);
            GL.Vertex2(x1, y1);

            GL.End();
        }

        public void FillRectangle(float x0, float y0, float x1, float y1, Color c)
        {
            GL.Color4(c);

            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(x0, y0);
            GL.Vertex2(x1, y0);
            GL.Vertex2(x1, y1);
            GL.Vertex2(x0, y1);
            GL.End();
        }

        public void DrawRectange(float x0, float y0, float x1, float y1, Color c)
        {
            GL.Color4(c);

            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex2(x0, y0);
            GL.Vertex2(x1, y0);
            GL.Vertex2(x1, y1);
            GL.Vertex2(x0, y1);
            GL.End();
        }

        public void FillCircle(float x0, float y0, float radius, Color color)
        {
            float x1;
            float y1;


            GL.Begin(PrimitiveType.TriangleFan);
            GL.Color4(color);

            GL.Vertex2(x0, y0);

            for (int i = 0; i < CircleSins.Length; i++)
            {
                x1 = CircleSins[i] * radius;
                y1 = CircleCoss[i] * radius;
                GL.Vertex2(x0 + x1, y0 + y1);
            }

            GL.End();
        }

        public void DrawCircle(float x0, float y0, float radius, Color color)
        {
            float x1;
            float y1;


            GL.Begin(PrimitiveType.LineLoop);
            GL.Color4(color);

            for (int i = 0; i < CircleSins.Length; i++)
            {
                x1 = CircleSins[i] * radius;
                y1 = CircleCoss[i] * radius;
                GL.Vertex2(x0 + x1, y0 + y1);
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
            FillRectangle(x0, y0, x1, y1, back);
            pct = Math.Min(pct, 1);
            pct = Math.Max(pct, 0);
            var barWidth = (x1-x0) * pct;
            FillRectangle(x0, y0, x0 + barWidth, y1, bar);
        }

        public void DrawText(string text, float X0, float Y0)
        {
            var w = (X0 / 2 + 0.5f) * CardKartClient.CardKartClient.GUI.WindowWidth;
            var h = (Y0 / 2 + 0.5f) * CardKartClient.CardKartClient.GUI.WindowHeight;

            FontDrawing.Print(
                DefaultFont, 
                text, 
                new OpenTK.Vector3(w, h, 0),
                QFontAlignment.Left, 
                RenderOptions);


            FontDrawing.RefreshBuffers();
            FontDrawing.Draw();
            FontDrawing.DrawingPimitiveses.Clear();
            GL.UseProgram(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void DrawTextSmart(
            ProcessedText pt, 
            float w, 
            float h, 
            QFont font,
            QFontRenderOptions renderOptions)
        {
            FontDrawing.Print(font, pt, new OpenTK.Vector3(w, h, 0), renderOptions);

            FontDrawing.RefreshBuffers();
            FontDrawing.Draw();
            FontDrawing.DrawingPimitiveses.Clear();
            GL.UseProgram(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
