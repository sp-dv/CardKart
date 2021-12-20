using CardKartClient.GUI.Components;
using CardKartShared.GameState;
using SGL;

namespace CardKartClient.GUI.Scenes
{
    internal class GameScene : Scene
    {
        public GameController GameController;

        public HandComponent HeroHandPanel;

        public GameScene(GameController gameController)
        {
            GameController = gameController;

            // todo unhack
            var hero = GameController.GameState.PlayerA;

            HeroHandPanel = new HandComponent(hero.Hand);
            HeroHandPanel.CardClicked += (cardComponent) =>
            {
                if (cardComponent.Card != null) 
                { 
                    GameObjectClicked(cardComponent.Card);
                }
            };
            Components.Add(HeroHandPanel);
        }

        private void GameObjectClicked(GameObject gameObject)
        {
            GameController.GameObjectSaxophone.Play(gameObject);
        }
    }
}
