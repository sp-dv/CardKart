using CardKartClient.GUI.Components;
using SGL;

namespace CardKartClient.GUI.Scenes
{
    internal class GameEndedScene : Scene
    {
        private SmartTextPanel XD;
        private SmartTextPanel XD2;

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
        }
    }
}
