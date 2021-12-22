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
        public PlayerPanel HeroPanel;
        public BattlefieldComponent VillainBattlefieldPanel;
        public PlayerPanel VillainPanel;
        public ChooserPanel ChooserPanel;

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
            HeroBattlefieldPanel.TokenClicked +=
                token => GameObjectClicked(token);
            Components.Add(HeroBattlefieldPanel);


            VillainBattlefieldPanel = new BattlefieldComponent(villain.Battlefield);
            VillainBattlefieldPanel.X = -0.3f;
            VillainBattlefieldPanel.Y = 0.4f;
            VillainBattlefieldPanel.TokenClicked += 
                token => GameObjectClicked(token);
            Components.Add(VillainBattlefieldPanel);

            ChooserPanel = new ChooserPanel(gameController.ChoiceHelper);
            ChooserPanel.X = -0.9f;
            ChooserPanel.Y = -0.15f;
            ChooserPanel.Layout();
            ChooserPanel.OptionClicked += OptionChoiceClicked;
            Components.Add(ChooserPanel);

            HeroPanel = new PlayerPanel(hero);
            HeroPanel.X = -0.9f;
            HeroPanel.Y = -0.8f;
            HeroPanel.Layout();
            Components.Add(HeroPanel);

            VillainPanel = new PlayerPanel(villain);
            VillainPanel.X = -0.9f;
            VillainPanel.Y = 0.6f;
            VillainPanel.Layout();
            Components.Add(VillainPanel);

        }

        private void GameObjectClicked(GameObject gameObject)
        {
            GameController.ChoiceHelper.PlayerChoiceSaxophone.Play(new PlayerChoiceStruct(gameObject));
        }

        private void OptionChoiceClicked(OptionChoice optionChoice)
        {
            GameController.ChoiceHelper.PlayerChoiceSaxophone.Play(new PlayerChoiceStruct(optionChoice));
        }
    }
}
