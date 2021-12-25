﻿using CardKartClient.GUI.Scenes;
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
            Logging.Log(
                LogLevel.Info,
                $"Starting CardKart version {Constants.Version}");

            CardKartClient.GUI.OpenWindow();

            CardKartClient.GUI.TransitionToScene(new LoginScene());
        }

        public void StartGame(int gameID, int heroIndex)
        {
            if (ActiveGame != null)
            {
                Logging.Log(
                    LogLevel.Debug, 
                    "Starting game with a game already active.");
            }

            ActiveGame = new GameController(
                gameID, 
                heroIndex, 
                CardKartClient.Server.CreateGameChoiceSynchronizer(gameID));
            CardKartClient.GUI.TransitionToScene(new GameScene(ActiveGame));
            ActiveGame.Start();
        }

        public void StartFakeGame()
        {
            ActiveGame = new GameController(
                0,
                1,
                null);
            CardKartClient.GUI.TransitionToScene(new GameScene(ActiveGame));
            ActiveGame.Start();
        }

        public void HandleWindowClosed()
        {
            Environment.Exit(0);
        }

        public string Login(string username, string password)
        {
            var response = CardKartClient.Server.LogIn(username, password);
            if (response.Code == GenericResponseMessage.Codes.OK)
            {
                User.Username = username;
                CardKartClient.GUI.TransitionToScene(new MainMenuScene());
            }

            return response.Info;
        }
    }
}
