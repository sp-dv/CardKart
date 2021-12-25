using CardKartClient.GUI.Components;
using CardKartShared.GameState;
using SGL;
using System.Drawing;

namespace CardKartClient.GUI.Scenes
{
    internal class MainMenuScene : Scene
    {
        SmartTextPanel StartGameButton;
        SmartTextPanel JoinQueueButton;

        public MainMenuScene()
        {
            SmartTextPanel debug = new SmartTextPanel();
            debug.Text = "Long string of text to test shit out with. I sure hope this works. I sure hope future me has fixed text overflow xd.\nThis should be a new line bud.";
            debug.X = -0.8f;
            debug.Y = 0.4f;
            debug.Layout();
            Components.Add(debug);


#if true
            StartGameButton = new SmartTextPanel();
            StartGameButton.X = 0.1f;
            StartGameButton.Y = 0.0f;
            StartGameButton.Width = 0.15f;
            StartGameButton.Height = 0.09f;
            StartGameButton.Text = "Start Game";
            StartGameButton.Layout();
            StartGameButton.BackgroundColor = Color.DarkOrange;
            StartGameButton.Clicked += () =>
            {
                CardKartClient.Controller.StartFakeGame();
            };
            Components.Add(StartGameButton);
#endif

            JoinQueueButton = new SmartTextPanel();
            JoinQueueButton.X = 0.1f;
            JoinQueueButton.Y = 0.09f;
            JoinQueueButton.Width = 0.15f;
            JoinQueueButton.Height = 0.1f;
            JoinQueueButton.Text = "Join Queue";
            JoinQueueButton.Layout();
            JoinQueueButton.BackgroundColor = Color.DarkOrange;
            JoinQueueButton.Clicked += () =>
            {
                CardKartClient.Server.JoinQueue();
                JoinQueueButton.X = 10;
            };
            Components.Add(JoinQueueButton);
        }
    }
}
