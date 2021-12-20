using CardKartClient.GUI.Components;
using CardKartShared.GameState;
using SGL;
using System.Drawing;

namespace CardKartClient.GUI.Scenes
{
    internal class MainMenuScene : Scene
    {
        Button StartGameButton;

        public MainMenuScene()
        {
            StartGameButton = new Button();
            StartGameButton.Width = 0.15f;
            StartGameButton.Height = 0.1f;
            StartGameButton.Text = "Start Game";
            StartGameButton.Colour = Color.DarkOrange;
            StartGameButton.Clicked += () =>
            {
                CardKartClient.Controller.StartGame();
            };
            Components.Add(StartGameButton);
        }
    }
}
