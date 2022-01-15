using CardKartClient.GUI.Components;
using CardKartShared.GameState;
using CardKartShared.Util;
using OpenTK.Input;
using SGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CardKartClient.GUI.Scenes
{
    internal class RipPacksScene : Scene
    {
        public CardInfoPanel TopCard;
        
        private List<Card> RippedCards = new List<Card>();
        private int RippedIndex = -1;
        private bool ThereIsAnOpenPack => RippedIndex != -1;

        public SmartTextPanel RipPackButton;
        public SmartTextPanel BackButton;


        public RipPacksScene()
        {
            RipPackButton = new SmartTextPanel();
            RipPackButton.Text = "Rip Pack";
            RipPackButton.Clicked += RipNewPack;
            Components.Add(RipPackButton);

            TopCard = new CardInfoPanel();
            TopCard.Clicked += RevealNextCard;
            Components.Add(TopCard);

            BackButton = new SmartTextPanel();
            BackButton.Text = "Main Menu";
            BackButton.X = -0.95f;
            BackButton.Y = -0.85f;
            BackButton.Height = 0.1f;
            BackButton.BackgroundImage = Textures.Button1;
            BackButton.Alignment = QuickFont.QFontAlignment.Centre;
            BackButton.Layout();
            BackButton.Clicked += () => CardKartClient.Controller.ToMainMenu();
            Components.Add(BackButton);

            Layout();
        }

        public void Layout()
        {
            RipPackButton.X = Constants.GUI.A;
            RipPackButton.Y = Constants.GUI.B;
            RipPackButton.BackgroundImage = Textures.Button1;
            RipPackButton.Alignment = QuickFont.QFontAlignment.Centre;
            RipPackButton.TextOffsetY = Constants.GUI.E;
            RipPackButton.Layout();

            TopCard.X = Constants.GUI.C;
            TopCard.Y = Constants.GUI.D;
        }

        public override void HandleKeyDown(KeyboardKeyEventArgs e)
        {
            base.HandleKeyDown(e);

            if (e.Key == Key.R)
            {
                Layout();
            }
        }

        private void RipNewPack()
        {
            if (ThereIsAnOpenPack) { return; }

            RippedCards = CardKartClient.Server.RipPack().Templates.Select(template => new Card(template)).ToList();
            RippedIndex = 0;

            RipPackButton.Visible = false;
            RevealNextCard();
        }

        private void RevealNextCard()
        {
            if (!ThereIsAnOpenPack) { return; }

            if (RippedIndex >= RippedCards.Count)
            {
                RippedIndex = -1;
                TopCard.SetCard(null);
                RipPackButton.Visible = true;
                return;
            }

            TopCard.SetCard(RippedCards[RippedIndex]);
            RippedIndex++;
        }
    }
}
