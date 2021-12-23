using CardKartClient.GUI;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using QuickFont;
using QuickFont.Configuration;
using System;
using System.Drawing;

namespace SGL
{
    internal class SGLWindow : GameWindow
    {
        private Matrix4 ProjectionMatrix;

        private QFontDrawing FondDrawing;

        public Scene CurrentScene;

        public SGLWindow() : base(1600, 900, new GraphicsMode(32, 24, 0, 32), "MLBall")
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusDstAlpha);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            base.OnLoad(e);

            FondDrawing = new QFontDrawing();

            GL.ClearColor(Color4.CornflowerBlue);

            TextureLoader.LoadTextures();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(ClientRectangle.X, ClientRectangle.Width, ClientRectangle.Y, ClientRectangle.Height, -1.0f, 1.0f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            FondDrawing.ProjectionMatrix = ProjectionMatrix;
            FondDrawing.DrawingPimitiveses.Clear();

            if (CurrentScene != null)
            {
                var da = new DrawAdapter();
                da.FontDrawing = FondDrawing;
                da.DefaultFont = Fonts.MainFont10;
                da.RenderOptions = Fonts.MainRenderOptions;
                da.ScreenHeight = Height;
                da.ScreenWidth = Width;

                CurrentScene.Draw(da);
            }

            FondDrawing.RefreshBuffers();
            FondDrawing.Draw();
            SwapBuffers();
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (CurrentScene != null)
            {
                var screenCoordinates = new ScreenCoordinate();
                screenCoordinates.X = (float)e.X / this.Width;
                screenCoordinates.Y = (float)e.Y / this.Height;
                var glCoordinates = screenCoordinates.ToGL();
                CurrentScene.HandleMouseMove(glCoordinates);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (CurrentScene != null)
            {
                CurrentScene.HandleMouseButtonUp(e);
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (CurrentScene != null)
            {
                var screenCoordinates = new ScreenCoordinate();
                screenCoordinates.X = (float)e.X / this.Width;
                screenCoordinates.Y = (float)e.Y / this.Height;
                var glCoordinates = screenCoordinates.ToGL();
                CurrentScene.HandleMouseButtonDown(e.Button, glCoordinates);
            }
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (CurrentScene != null)
            {
                CurrentScene.HandleKeyUp(e);
            }
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (CurrentScene != null)
            {
                CurrentScene.HandleKeyDown(e);
            }
        }
    }
}
