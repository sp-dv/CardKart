using CardKartClient.GUI.Components;
using SGL;
using System.Drawing;

namespace CardKartClient.GUI.Scenes
{
    internal class GameEndedScene : Scene
    {
        private SmartTextPanel XD;
        private SmartTextPanel XD2;
        private SmartTextPanel XD3;

        public GameEndedScene(string s)
        {
            XD = new SmartTextPanel();
            XD.Text = "Game ogre.";
            XD.Layout();
            Components.Add(XD);

            XD2 = new SmartTextPanel();
            XD2.Text = s;
            XD2.Y = -0.2f;
            XD2.Layout();
            Components.Add(XD2);

            XD3 = new SmartTextPanel();
            XD3.Text = "To Main Menu";
            XD3.BackgroundColor = Color.Silver;
            XD3.Y = -0.4f;
            XD3.Layout();
            XD3.Clicked += () => CardKartClient.Controller.ToMainMenu();
            Components.Add(XD3);

        }
    }
}
