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
            User.SaveConfig();
            Environment.Exit(0);
        }

        public void ToMainMenu()
        {
            CardKartClient.GUI.TransitionToScene(new MainMenuScene());
        }

        public void ToDeckEditor()
        {
            // Surely this never takes more than like a second right. No need to maybe stop if
            // the server shits the bed because we believe in trust.
            var collection = CardKartClient.Server.GetCollection();

            CardKartClient.GUI.TransitionToScene(new DeckEditorScene(collection.OwnedCards));
        }
        public void ToRipPacks()
        {
            CardKartClient.GUI.TransitionToScene(new RipPacksScene());
        }

        public void ToAuctionHouse()
        {
            CardKartClient.GUI.TransitionToScene(new AuctionHouseScene());
        }
    }
}
