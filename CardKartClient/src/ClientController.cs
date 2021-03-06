using CardKartClient.GUI.Scenes;
using CardKartShared;
using CardKartShared.GameState;
using CardKartShared.Network.Messages;
using CardKartShared.Util;
using System;

namespace CardKartClient
{
    internal class ClientController
    {
        public GameController ActiveGame;

        public void Startup()
        {
            CardKartClient.GUI.OpenWindow();

            CardKartClient.GUI.TransitionToScene(new LoginScene());
        }

        public void StartGame(int gameID, int heroIndex, int rngSeed)
        {
            if (ActiveGame != null)
            {
                Logging.Log(
                    LogLevel.Warning, 
                    "Starting game with a game already active.");
            }

            ActiveGame = new GameController(
                gameID, 
                heroIndex,
                rngSeed,
                CardKartClient.Server.CreateGameChoiceSynchronizer(gameID));
            ActiveGame.LoadDeckDelegate = () => User.LoadDeck();
            CardKartClient.GUI.TransitionToScene(new GameScene(ActiveGame));
            ActiveGame.StartGame();
        }

        public void EndGame(int gameID, int winnerIndex, GameEndedReasons reason)
        {
            if (ActiveGame.GameID != gameID) { throw new ThisShouldNeverHappen(); }

            ActiveGame.EndGame(winnerIndex, reason);
            ActiveGame = null;

            CardKartClient.GUI.TransitionToScene(new GameEndedScene(reason.ToString()));
        }

        public void HandleWindowClosed()
        {
            Environment.Exit(0);
        }

        public void ToMainMenu()
        {
            CardKartClient.GUI.TransitionToScene(new MainMenuScene());
        }

        public void ToDeckEditor()
        {
            CardKartClient.GUI.TransitionToScene(new DeckEditorScene());
        }

        public string Login(string username, string password)
        {
            var response = CardKartClient.Server.LogIn(username, password);
            if (response.Code == GenericResponseMessage.Codes.OK)
            {
                User.Username = username;
                ToMainMenu();
            }

            return response.Info;
        }
    }
}
