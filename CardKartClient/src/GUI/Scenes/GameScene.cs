using CardKartClient.GUI.Components;
using CardKartShared.GameState;
using SGL;

namespace CardKartClient.GUI.Scenes
{
    internal class GameScene : Scene
    {
        public GameController GameController;

        public HandComponent HeroHandPanel;
        public BattlefieldComponent HeroBattlefieldPanel;
        public BattlefieldComponent VillainBattlefieldPanel;

        public GameScene(GameController gameController)
        {
            GameController = gameController;

            var hero = GameController.Hero;
            var villain = GameController.Villain;

            HeroHandPanel = new HandComponent(hero.Hand);
            HeroHandPanel.X = -0.4f;
            HeroHandPanel.Y = -0.9f;
            HeroHandPanel.CardClicked += (cardComponent) =>
            {
                if (cardComponent.Card != null) 
                { 
                    GameObjectClicked(cardComponent.Card);
                }
            };
            Components.Add(HeroHandPanel);

            HeroBattlefieldPanel = new BattlefieldComponent(hero.Battlefield);
            HeroBattlefieldPanel.X = -0.3f;
            HeroBattlefieldPanel.Y = -0.2f;
            Components.Add(HeroBattlefieldPanel);


            VillainBattlefieldPanel = new BattlefieldComponent(villain.Battlefield);
            VillainBattlefieldPanel.X = -0.3f;
            VillainBattlefieldPanel.Y = 0.4f;
            Components.Add(VillainBattlefieldPanel);
        }

        private void GameObjectClicked(GameObject gameObject)
        {
            GameController.GameObjectSaxophone.Play(gameObject);
        }
    }
}
