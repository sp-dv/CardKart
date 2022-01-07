using CardKartClient.GUI.Components;
using SGL;

namespace CardKartClient.GUI.Scenes
{
    internal class GameEndedScene : Scene
    {
        private SmartTextPanel XD;

        public GameEndedScene()
        {
            XD = new SmartTextPanel();
            XD.Text = "Game ogre.";
            XD.Layout();
            Components.Add(XD);
        }
    }
}
