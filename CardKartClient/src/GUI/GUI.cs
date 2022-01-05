using CardKartClient.GUI;
using CardKartClient.GUI.Scenes;
using CardKartShared.GameState;
using SGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CardKartClient.GUI
{
    internal class GUIController
    {
        private SGLWindow Window;

        public int WindowHeight => Window.Height;
        public int WindowWidth => Window.Width;

        public void OpenWindow()
        {
            Textures.LoadTextures();

            ManualResetEventSlim re = new ManualResetEventSlim();
            new Thread(() =>
            {
                // The window has to be created on the same thread that ends
                // up calling Run which means it all has to go in a single thread.

                Window = new SGLWindow();
                Window.Load += (_, __) => re.Set();
                Window.Closed += (_, __) =>
                    { CardKartClient.Controller.HandleWindowClosed(); };
                Window.Run(100, 30);
            }).Start();
            re.Wait();
        }

        public void TransitionToScene(Scene scene)
        {
            Window.CurrentScene = scene;
        }
    }
}
