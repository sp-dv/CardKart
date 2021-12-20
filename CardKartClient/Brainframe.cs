using CardKartShared.GameState;
using CardKartShared.Util;

namespace CardKartClient
{
    internal class Brainframe
    {
        public GameController ActiveGame;

        public void Startup()
        {
            Logging.Log(
                LogLevel.Info,
                $"Starting CardKart version {Constants.Version}");

            CardKartClient.GUI.OpenWindow();

            CardKartClient.GUI.ToMainMenu();
        }

        public void StartGame()
        {
            if (ActiveGame != null)
            {
                Logging.Log(
                    LogLevel.Debug, 
                    "Starting game with a game already active.");
            }

            ActiveGame = new GameController();
            CardKartClient.GUI.ToGame(ActiveGame);
            ActiveGame.Start();
        }
    }
}
