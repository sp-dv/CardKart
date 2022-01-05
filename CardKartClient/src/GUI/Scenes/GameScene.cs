﻿using CardKartClient.GUI.Components;
using CardKartShared.GameState;
using SGL;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

        public PileDisplayPanel HeroGraveyardDisplay;
        public PileDisplayPanel VillainGraveyardDisplay;

        private GLCoordinate StackFrom;
        private GLCoordinate[] StackTo;

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

            StackPanel = new StackPanel(gameController.GameState.CastingStack);
            StackPanel.X = -0.95f;
            StackPanel.Y = -0.5f;
            StackPanel.TargetsUpdated += UpdateStackTargetLines;
            StackPanel.AbilityClicked += CastingContextClicked;
            Components.Add(StackPanel);

            CardChoicePanel = new CardChoicePanel();
            CardChoicePanel.CardClicked += (cardComponent) =>
            {
                if (cardComponent.Card != null)
                {
                    GameObjectClicked(cardComponent.Card);
                }
            };
            CardChoicePanel.X = StackPanel.X;
            CardChoicePanel.Y = StackPanel.Y;
            Components.Add(CardChoicePanel);

            ChooserPanel = new ChooserPanel(gameController.ChoiceHelper);
            ChooserPanel.X = -0.7f;
            ChooserPanel.Y = -0.15f;
            ChooserPanel.Layout();
            ChooserPanel.OptionClicked += OptionChoiceClicked;
            ChooserPanel.AbilityClicked += AbilityChoiceClicked;
            ChooserPanel.CardChoicePanel = CardChoicePanel;
            Components.Add(ChooserPanel);

            HeroPanel = new PlayerPanel(hero);
            HeroPanel.X = -0.95f;
            HeroPanel.Y = -0.8f;
            HeroPanel.PlayerPortraitClicked += player => GameObjectClicked(player);
            HeroPanel.Layout();
            Components.Add(HeroPanel);

            VillainPanel = new PlayerPanel(villain);
            VillainPanel.X = -0.95f;
            VillainPanel.Y = 0.6f;
            VillainPanel.PlayerPortraitClicked += player => GameObjectClicked(player);
            VillainPanel.Layout();
            Components.Add(VillainPanel);

            HeroGraveyardDisplay = new PileDisplayPanel(hero.Graveyard);
            HeroPanel.GraveyardButton.Clicked += () => { HeroGraveyardDisplay.Visible ^= true; };
            HeroGraveyardDisplay.Visible = false;
            HeroGraveyardDisplay.CardClicked += cardComponent => GameObjectClicked(cardComponent.Card);
            Components.Add(HeroGraveyardDisplay);

            VillainGraveyardDisplay = new PileDisplayPanel(villain.Graveyard);
            VillainPanel.GraveyardButton.Clicked += () => { VillainGraveyardDisplay.Visible ^= true; };
            VillainGraveyardDisplay.Visible = false;
            VillainGraveyardDisplay.CardClicked += cardComponent => GameObjectClicked(cardComponent.Card);
            Components.Add(VillainGraveyardDisplay);

            CombatAnimator = new CombatAnimator(
                HeroBattlefieldPanel,
                VillainBattlefieldPanel,
                gameController);
            Components.Add(CombatAnimator);

            gameController.ChoiceHelper.RequestShowCards += ShowCards;
        }

        private void ShowCards(IEnumerable<Card> cards)
        {
            var showCardsPanel = new ShowCardsPanel(cards.Select(card => card.Template));
            lock (Components)
            {
                Components.Add(showCardsPanel);
            }
        }

        private void UpdateStackTargetLines(CardComponent caster, System.Collections.Generic.IEnumerable<int> targetIDs)
        {
            if (caster == null || targetIDs == null || targetIDs.Count() == 0) 
            {
                StackFrom = null;
                StackTo = null;
                return; 
            }

            var toLocations = targetIDs
                .Select(id => GetComponentOf(id))
                .Where(component => component != null)
                .Select(component => component.GuiLocation1)
                .ToArray();
            StackFrom = caster.GuiLocation1;
            StackTo = toLocations;
        }

        private GuiComponent GetComponentOf(int id)
        {
            if (HeroPanel.Player.ID == id ||
                HeroPanel.Player.HeroCard.ID == id ||
                HeroPanel.Player.HeroCard.Token.ID == id)
            {
                return HeroPanel.PlayerPortrait;
            }
            if (VillainPanel.Player.ID == id ||
                VillainPanel.Player.HeroCard.ID == id ||
                VillainPanel.Player.HeroCard.Token.ID == id)
            {
                return VillainPanel.PlayerPortrait;
            }

            var heroToken = HeroBattlefieldPanel.GetComponent(id);
            if (heroToken != null) { return heroToken; }

            var villainToken = VillainBattlefieldPanel.GetComponent(id);
            if (villainToken != null) { return villainToken; }

            return null;
        }

        private void GameObjectClicked(GameObject gameObject)
        {
            GameController.ChoiceHelper.PlayerChoiceSaxophone.Play(new PlayerChoiceStruct(gameObject));
        }

        private void OptionChoiceClicked(OptionChoice optionChoice)
        {
            GameController.ChoiceHelper.PlayerChoiceSaxophone.Play(new PlayerChoiceStruct(optionChoice));
        }

        private void AbilityChoiceClicked(Ability ability)
        {
            GameController.ChoiceHelper.PlayerChoiceSaxophone.Play(new PlayerChoiceStruct(ability));
        }

        private void CastingContextClicked(AbilityCastingContext context)
        {
            GameController.ChoiceHelper.PlayerChoiceSaxophone.Play(new PlayerChoiceStruct(context));
        }

        protected override void DrawPost(DrawAdapter drawAdapter)
        {
            if (StackFrom != null && StackTo != null)
            {
                foreach (var location in StackTo)
                {
                    drawAdapter.DrawLine(StackFrom.X, StackFrom.Y, location.X, location.Y, Color.Black);
                }
            }
        }
    }
}
