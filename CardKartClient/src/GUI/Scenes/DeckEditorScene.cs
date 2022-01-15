using CardKartClient.GUI;
using CardKartClient.GUI.Components;
using CardKartShared.GameState;
using CardKartShared.Util;
using Newtonsoft.Json;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace CardKartClient.GUI.Scenes
{
    internal class DeckEditorScene : Scene
    {
        private DeckPanel DeckPanel;

        private DeckCardComponent[] CardComponents;
        private Card[] AllCards;

        private SmartTextPanel BackButton;
        private SmartTextPanel SaveButton;
        private SmartTextPanel LoadButton;


        private SmartTextPanel NextButton;
        private SmartTextPanel PreviousButton;

        private CardInfoPanel CardInfoPanel;

        private int CurrentPage = 0;

        private const int CardsPerRow = 4;
        private const int CardsPerColumn = 2;
        private const int CardsPerPage = CardsPerRow * CardsPerColumn;

        private Dictionary<CardTemplates, int> OwnedCards;

        public DeckEditorScene(Dictionary<CardTemplates, int> ownedCards)
        {
            OwnedCards = ownedCards;

            CardInfoPanel = new CardInfoPanel();
            CardInfoPanel.X = 0.5f;
            CardInfoPanel.Y = -0.5f;
            Components.Add(CardInfoPanel);


            SaveButton = new SmartTextPanel();
            SaveButton.Text = "Save Deck";
            SaveButton.X = -0.35f;
            SaveButton.Y = 0.72f;
            SaveButton.Height = 0.1f;
            SaveButton.BackgroundColor = Color.LightGray;
            SaveButton.Alignment = QuickFont.QFontAlignment.Centre;
            SaveButton.Layout();
            SaveButton.Clicked += SaveDeck;
            Components.Add(SaveButton);

            LoadButton = new SmartTextPanel();
            LoadButton.Text = "Load Deck";
            LoadButton.X = 0.05f;
            LoadButton.Y = 0.72f;
            LoadButton.Height = 0.1f;
            LoadButton.BackgroundColor = Color.LightGray;
            LoadButton.Alignment = QuickFont.QFontAlignment.Centre;
            LoadButton.Layout();
            LoadButton.Clicked += LoadDeck;
            Components.Add(LoadButton);

            BackButton = new SmartTextPanel();
            BackButton.Text = "Main Menu";
            BackButton.X = -0.95f;
            BackButton.Y = -0.85f;
            BackButton.Height = 0.1f;
            BackButton.BackgroundColor = Color.LightGray;
            BackButton.Layout();
            BackButton.Clicked += () => CardKartClient.Controller.ToMainMenu();
            Components.Add(BackButton);

            AllCards = Enum.GetValues(typeof(CardTemplates)).Cast<CardTemplates>()
                .Where(template => template != CardTemplates.None)
                .Select(template => new Card(template))
                .OrderBy(card => card.Colour)
                .ThenBy(card => card.CastingCost.Size)
                .ThenBy(card => card.Name)
                .Where(card => !card.IsTokenCard)
                .ToArray();

            DeckPanel = new DeckPanel(ownedCards);
            DeckPanel.X = Constants.GUI.DeckPanelX;
            DeckPanel.Y = Constants.GUI.DeckPanelY;
            DeckPanel.RequestInfoDisplay += card => CardInfoPanel.SetCard(card);
            Components.Add(DeckPanel);

            var cardComponentList = new List<DeckCardComponent>();

            for (int j = 0; j < CardsPerColumn; j++)
            {
                for (int i = 0; i < CardsPerRow; i++)
                {
                    var ix = i + (j * CardsPerRow);
                    var mc = new DeckCardComponent();
                    mc.X = -0.485f + (i * 0.23f);
                    mc.Y = -0.62f + ((CardsPerColumn - j - 1) * 0.60f); // Hack to get them to draw top to bottom...
                    mc.Clicked += () => AddCardToDeck(AllCards[ix + CurrentPage * CardsPerPage].Template);
                    mc.MouseEnteredEvent += () =>
                    {
                        CardInfoPanel.SetCard(AllCards.ElementAtOrDefault(ix + CurrentPage * CardsPerPage));
                    };
                    mc.MouseExitedEvent += () =>
                    {
                        CardInfoPanel.SetCard(null);
                    };
                    cardComponentList.Add(mc);
                    Components.Add(mc);
                }
            }

            CardComponents = cardComponentList.ToArray();

            NextButton = new SmartTextPanel();
            NextButton.Width = 0.1f;
            NextButton.Height = 0.1f;
            NextButton.X = 0.33f;
            NextButton.Y = 0.7f;
            NextButton.BackgroundImage = Textures.ButtonNext1;
            NextButton.Clicked += () => {
                if ((CurrentPage + 1) * CardsPerPage <= AllCards.Count())
                {
                    CurrentPage++;
                    UpdateCards();
                }
            };
            Components.Add(NextButton);

            PreviousButton = new SmartTextPanel();
            PreviousButton.Width = 0.1f;
            PreviousButton.Height = 0.1f;
            PreviousButton.X = -0.5f;
            PreviousButton.Y = 0.7f;
            PreviousButton.BackgroundImage = Textures.ButtonPrev1;
            PreviousButton.Clicked += () => { 
                if (CurrentPage > 0)
                {
                    CurrentPage--;
                    UpdateCards();
                }
            };
            Components.Add(PreviousButton);

            UpdateCards();
        }

        private void SaveDeck()
        {
            User.SaveDeck(DeckPanel.GetDeck());
        }

        private void LoadDeck()
        {
            var deck = User.LoadDeck();
            if (deck == null) { return; }

            DeckPanel.LoadDeck(deck);
        }

        public void UpdateCards()
        {
            int i = CurrentPage * CardsPerPage;
            foreach (var cc in CardComponents)
            {
                if (i >= AllCards.Length)
                {
                    cc.Visible = false;
                }
                else
                {
                    var card = AllCards[i];
                    var template = card.Template;
                    var ownedCount = OwnedCards.ContainsKey(template) ? OwnedCards[template] : 0;
                    
                    cc.SetCard(AllCards[i], ownedCount);
                    cc.Visible = true;
                }
                i++;
            }
        }

        private void AddCardToDeck(CardTemplates template)
        {
            DeckPanel.AddCard(new Card(template));
        }

        protected override void PreDraw(DrawAdapter drawAdapter)
        {
            drawAdapter.DrawSprite(-1, -1, 1, 1, Textures.DeckEditorBG1);
        }
    }

    internal class DeckCardComponent : GuiComponent
    {
        private CardComponent Inner;
        private string OwnedCountString;


        public void SetCard(Card card, int ownedCount)
        {
            Inner = new CardComponent(card);
            Inner.X = X;
            Inner.Y = Y;
            Width = Inner.Width;
            Height = Inner.Height;

            if (ownedCount == 0)
            {
                OwnedCountString = "";
            }
            else
            {
                OwnedCountString = $"x {ownedCount}";
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            if (Inner != null)
            {
                Inner.Draw(drawAdapter);

                drawAdapter.DrawText(OwnedCountString, X + 0.1f, Y + 0.61f, Fonts.MainFont14, Fonts.MainRenderOptions, QuickFont.QFontAlignment.Centre);
            }    
        }
    }
}
