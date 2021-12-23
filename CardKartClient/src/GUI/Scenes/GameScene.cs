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
        public StackPanel StackPanel;
        public CombatAnimator CombatAnimator;
        public CardChoicePanel CardChoicePanel;



        public GameScene(GameController gameController)
        {
            GameController = gameController;

            var hero = GameController.Hero;
            var villain = GameController.Villain;

            HeroHandPanel = new HandComponent(hero.Hand);
            HeroHandPanel.X = -0.2f;
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
            HeroBattlefieldPanel.X = -0.2f;
            HeroBattlefieldPanel.Y = -0.2f;
            HeroBattlefieldPanel.TokenClicked +=
                token => GameObjectClicked(token);
            Components.Add(HeroBattlefieldPanel);


            VillainBattlefieldPanel = new BattlefieldComponent(villain.Battlefield);
            VillainBattlefieldPanel.X = -0.2f;
            VillainBattlefieldPanel.Y = 0.4f;
            VillainBattlefieldPanel.TokenClicked += 
                token => GameObjectClicked(token);
            Components.Add(VillainBattlefieldPanel);


            CardChoicePanel = new CardChoicePanel();
            CardChoicePanel.CardClicked += (cardComponent) =>
            {
                if (cardComponent.Card != null)
                {
                    GameObjectClicked(cardComponent.Card);
                }
            };
            Components.Add(CardChoicePanel);

            ChooserPanel = new ChooserPanel(gameController.ChoiceHelper);
            ChooserPanel.X = -0.7f;
            ChooserPanel.Y = -0.15f;
            ChooserPanel.Layout();
            ChooserPanel.OptionClicked += OptionChoiceClicked;
            ChooserPanel.CardChoicePanel = CardChoicePanel;
            Components.Add(ChooserPanel);

            HeroPanel = new PlayerPanel(hero);
            HeroPanel.X = -0.95f;
            HeroPanel.Y = -0.8f;
            HeroPanel.Layout();
            Components.Add(HeroPanel);

            VillainPanel = new PlayerPanel(villain);
            VillainPanel.X = -0.95f;
            VillainPanel.Y = 0.6f;
            VillainPanel.Layout();
            Components.Add(VillainPanel);

            StackPanel = new StackPanel(gameController.GameState.CastingStack);
            StackPanel.X = -0.95f;
            StackPanel.Y = -0.5f;
            Components.Add(StackPanel);


            CombatAnimator = new CombatAnimator(
                HeroBattlefieldPanel,
                VillainBattlefieldPanel,
                gameController);
            Components.Add(CombatAnimator);
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
