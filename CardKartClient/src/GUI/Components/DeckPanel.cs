using CardKartShared.GameState;
using CardKartShared.Util;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using static CardKartClient.GUI.Scenes.GameScene;

namespace CardKartClient.GUI.Components
{
    internal class DeckPanel : GuiComponent
    {
        public event RequestInfoDisplayHandler RequestInfoDisplay;

        private SmartTextPanel CardCountPanel;
        private SmartTextPanel HeroNamePanel;

        private Card HeroCard;
        private IEnumerable<DeckSubComponent> SubComponents => 
            Components.Where(component => component is DeckSubComponent).Cast<DeckSubComponent>();

        public DeckPanel()
        {
            Width = Constants.GUI.DeckPanelWidth;
            Height = Constants.GUI.DeckPanelHeight;

            CardCountPanel = new SmartTextPanel();
            CardCountPanel.Font = Fonts.MainFont14;
            Components.Add(CardCountPanel);

            HeroNamePanel = new SmartTextPanel();
            HeroNamePanel.MouseEnteredEvent += () => RequestInfoDisplay?.Invoke(HeroCard);
            HeroNamePanel.MouseExitedEvent += () => RequestInfoDisplay?.Invoke(null);
            Components.Add(HeroNamePanel);
        }

        public void AddCard(Card card)
        {
            if (card.Type == CardTypes.Hero)
            {
                HeroCard = card;
            }
            else
            {
                var existingSubComponent = SubComponents.FirstOrDefault(sc => sc.Card.Template == card.Template);

                if (existingSubComponent == null)
                {
                    var newSubComponent = new DeckSubComponent(card.Template, 1);
                    if (newSubComponent.Count == 0) { return; }

                    newSubComponent.Clicked += () => RemoveCard(card);
                    newSubComponent.MouseEnteredEvent += () => RequestInfoDisplay?.Invoke(card);
                    newSubComponent.MouseExitedEvent += () => RequestInfoDisplay?.Invoke(null);

                    lock (Components)
                    {
                        Components.Add(newSubComponent);
                    }

                }
                else
                {
                    existingSubComponent.AdjustCount(1);
                }
            }
            LayoutComponents();
        }

        public void RemoveCard(Card card)
        {
            var existingSubComponent = SubComponents.FirstOrDefault(sc => sc.Card.Template == card.Template);

            if (existingSubComponent != null)
            {
                existingSubComponent.AdjustCount(-1);

                if (existingSubComponent.Count == 0)
                {
                    lock (Components)
                    {
                        Components.Remove(existingSubComponent);
                    }
                }
            }
            else 
            {
                Logging.Log(LogLevel.Warning, "Tried to remove a card from deck when deck doesn't contain such a card.");
            }

            LayoutComponents();
        }

        public Deck GetDeck()
        {
            var templates = new List<CardTemplates>();
            lock (Components) 
            { 
                foreach (var component in SubComponents)
                {
                    for (int i = 0; i < component.Count; i++)
                    {
                        templates.Add(component.Card.Template);
                    }
                }
            }

            return new Deck(HeroCard.Template, templates.ToArray());
        }

        public void LoadDeck(Deck deck)
        {
            lock (Components)
            {
                Components.Clear();

                HeroCard = new Card(deck.HeroCardTemplate);
                var templates = deck.DeckTemplates;

                var templateCounts = new Dictionary<CardTemplates, int>();
                foreach (var template in templates)
                {
                    if (!templateCounts.ContainsKey(template)) { templateCounts[template] = 0; }
                    templateCounts[template]++;
                }
                var subComponents =
                    templateCounts
                    .Select(kvp => {
                        var sc = new DeckSubComponent(kvp.Key, kvp.Value);
                        var card = new Card(kvp.Key);

                        sc.Clicked += () => RemoveCard(card);
                        sc.MouseEnteredEvent += () => RequestInfoDisplay?.Invoke(card);
                        sc.MouseExitedEvent += () => RequestInfoDisplay?.Invoke(null);

                        return sc;
                    })
                    .Where(sc => sc.Count > 0).ToArray();

                Components.AddRange(subComponents);
                Components.Add(CardCountPanel);
                Components.Add(HeroNamePanel);
            }

            LayoutComponents();
        }

        public void LayoutComponents()
        {
            int deckSize = 0;
            lock (Components)
            {
                var sortedSubComponents = 
                    SubComponents.OrderBy(sc => sc.Card.Name).ToArray();

                float X0 = X + Constants.GUI.DeckPanelSubcomponentX0;
                float Y0 = Y + Constants.GUI.DeckPanelSubcomponentY0;

                for (int i = 0; i < sortedSubComponents.Length; i++)
                {
                    var sc = sortedSubComponents[i];
                    sc.X = X0;
                    sc.Y = Y0 - i * Constants.GUI.DeckPanelSubcomponentYOffset;
                    deckSize += sc.Count;
                }
            }

            CardCountPanel.Text = $"{deckSize}/30";
            CardCountPanel.X = X + 0.02f;
            CardCountPanel.Y = Y + 1.46f;
            CardCountPanel.Width = 0.07f;
            CardCountPanel.Height = 0.06f;
            CardCountPanel.BackgroundColor = null;
            CardCountPanel.RenderOptions = deckSize == 30 ? Fonts.MainRenderOptions : Fonts.RedRenderOptions;
            CardCountPanel.Layout();

            if (HeroCard != null) { HeroNamePanel.Text = $"Hero: {HeroCard.Name}"; }
            HeroNamePanel.X = X + 0.09f;
            HeroNamePanel.Y = Y + 1.44f;
            HeroNamePanel.Font = Fonts.MainFont10;
            HeroNamePanel.Width = 0.25f;
            HeroNamePanel.Height = 0.06f;
            HeroNamePanel.BackgroundColor = null;
            HeroNamePanel.Alignment = QuickFont.QFontAlignment.Centre;
            HeroNamePanel.Layout();
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.SlateGray);
        }
    }

    internal class DeckSubComponent : GuiComponent
    {
        public int Count { get; private set; }
        private string CardName { get; }
        private int MaxCount { get; }

        public Card Card { get; }

        public DeckSubComponent(CardTemplates template, int initialCount)
        {
            Width = Constants.GUI.DeckSubcomponentWidth;
            Height = Constants.GUI.DeckSubcomponentHeight;

            Card = new Card(template);
            CardName = Card.Name;
            MaxCount = Constants.MaxCountByRarity(Card.Rarity);

            AdjustCount(initialCount);
        }

        public void AdjustCount(int amount)
        {
            Count += amount;
            if (Count < 0) { Count = 0; }
            if (Count > MaxCount) { Count = MaxCount; }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.DrawRectangle(X, Y, X + Width, Y + Height, Color.Black);

            drawAdapter.DrawText(
                Count.ToString(),
                X + Constants.GUI.DeckPanelCountOffsetX, 
                Y + Constants.GUI.DeckPanelCountOffsetY,
                Fonts.MainFont14,
                Fonts.BigRenderOptions,
                QuickFont.QFontAlignment.Left);

            drawAdapter.DrawText(
                CardName,
                X + Constants.GUI.DeckPanelCardNameOffsetX,
                Y + Constants.GUI.DeckPanelCardNameOffsetY,
                Fonts.MainFont10,
                Fonts.BigRenderOptions,
                QuickFont.QFontAlignment.Centre);
        }
    }
}
