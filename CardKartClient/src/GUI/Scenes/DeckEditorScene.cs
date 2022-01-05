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
        private Card HeroCard;
        private List<Card> DeckCards = new List<Card>();
        private CardChoicePanel DeckPanel;

        private ModifiableCardComponent[] CardComponents;
        private Card[] AllCards;

        private SmartTextPanel BackButton;
        private SmartTextPanel SaveButton;
        private SmartTextPanel LoadButton;


        private SmartTextPanel NextButton;
        private SmartTextPanel PreviousButton;

        private int CurrentPage = 0;

        private const int CardsPerRow = 4;
        private const int CardsPerColumn = 3;
        private const int CardsPerPage = CardsPerRow * CardsPerColumn;  


        public DeckEditorScene()
        {
            SaveButton = new SmartTextPanel();
            SaveButton.Text = "Save Deck";
            SaveButton.X = -0.95f;
            SaveButton.Y = 0.85f;
            SaveButton.Height = 0.1f;
            SaveButton.BackgroundColor = Color.LightGray;
            SaveButton.Layout();
            SaveButton.Clicked += SaveDeck;
            Components.Add(SaveButton);

            LoadButton = new SmartTextPanel();
            LoadButton.Text = "Load Deck";
            LoadButton.X = -0.95f;
            LoadButton.Y = 0.70f;
            LoadButton.Height = 0.1f;
            LoadButton.BackgroundColor = Color.LightGray;
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
                .Select(template => new Card(template)).ToArray();

            DeckPanel = new CardChoicePanel();
            DeckPanel.X = 0.7f;
            DeckPanel.Y = -0.5f;
            DeckPanel.CardClicked += (cardComponent) => RemoveCardFromDeck(cardComponent.Card.Template);
            Components.Add(DeckPanel);

            var cardComponentList = new List<ModifiableCardComponent>();

            for (int i = 0; i < CardsPerRow; i++)
            {
                for (int j = 0; j < CardsPerColumn; j++)
                {
                    var ix = (i * CardsPerColumn) + j;
                    var mc = new ModifiableCardComponent();
                    mc.X = -0.7f + (i * 0.3f);
                    mc.Y = -0.8f + (j * 0.52f);
                    mc.Clicked += () => AddCardToDeck(AllCards[ix + CurrentPage * CardsPerPage].Template);
                    cardComponentList.Add(mc);
                    Components.Add(mc);
                }
            }

            CardComponents = cardComponentList.ToArray();

            NextButton = new SmartTextPanel();
            NextButton.Width = 0.1f;
            NextButton.Height = 0.1f;
            NextButton.X = 0.5f;
            NextButton.BackgroundColor = Color.Green;
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
            PreviousButton.X = -0.9f;
            PreviousButton.BackgroundColor = Color.Blue;
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
            var deck = new Deck(DeckCards.Select(card => card.Template).ToArray());
            var deckString = JsonConvert.SerializeObject(deck);
            File.WriteAllText("./a.ckd", deckString);
        }

        private void LoadDeck()
        {
            if (!File.Exists("./a.ckd")) { return; }
            var deckString = File.ReadAllText("./a.ckd");
            var deck = JsonConvert.DeserializeObject<Deck>(deckString);

            DeckCards.Clear();
            DeckCards.AddRange(deck.CardTemplates.Select(template => new Card(template)));
            DeckPanel.Update(DeckCards);
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
                    cc.SetCard(AllCards[i]);
                    cc.Visible = true;
                }
                i++;
            }
        }

        private void AddCardToDeck(CardTemplates template)
        {
            var newCard = new Card(template);
            var alreadyInDeckCount = DeckCards.Where(card => card.Template == template).Count();
            int maxCount;
            switch (newCard.Rarity)
            {
                case CardRarities.Common: maxCount = 4; break;
                case CardRarities.Uncommon: maxCount = 3; break;
                case CardRarities.Rare: maxCount = 2; break;
                case CardRarities.Legendary: maxCount = 1; break;
                default: throw new NotImplementedException();
            }

            if (alreadyInDeckCount >= maxCount) { return; }

            DeckCards.Add(new Card(template));
            DeckPanel.Update(DeckCards);
        }

        private void RemoveCardFromDeck(CardTemplates template)
        {
            // This lookup will realistically never fail but we can never be too safe.
            var cardToRemove = DeckCards.FirstOrDefault(card => card.Template == template);
            if (cardToRemove == null) { return; }
            DeckCards.Remove(cardToRemove);
            DeckPanel.Update(DeckCards);
        }
    }

    internal class ModifiableCardComponent : GuiComponent
    {
        private CardComponent Inner;

        public void SetCard(Card card)
        {
            Inner = new CardComponent(card);
            Inner.X = X;
            Inner.Y = Y;
            Width = Inner.Width;
            Height = Inner.Height;
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            if (Inner != null)
            {
                Inner.Draw(drawAdapter);    
            }    
        }
    }
}
