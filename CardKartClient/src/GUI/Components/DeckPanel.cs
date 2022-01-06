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

        private IEnumerable<DeckSubComponent> SubComponents => 
            Components.Where(component => component is DeckSubComponent).Cast<DeckSubComponent>();

        public DeckPanel()
        {
            Width = Constants.GUI.DeckPanelWidth;
            Height = Constants.GUI.DeckPanelHeight;
        }

        public void AddCard(Card card)
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

                // This must be outside lock or we deadlock.
                LayoutComponents();
            }
            else
            {
                existingSubComponent.AdjustCount(1);
            }
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

                    // This must be outside lock or we deadlock.
                    LayoutComponents();
                }
            }
            else 
            {
                Logging.Log(LogLevel.Warning, "Tried to remove a card from deck when deck doesn't contain such a card.");
            }
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
            return new Deck(templates.ToArray());
        }

        public void LoadDeck(Deck deck)
        {
            lock (Components)
            {
                Components.Clear();

                var templates = deck.CardTemplates;
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
            }

            LayoutComponents();
        }

        public void LayoutComponents()
        {
            lock (Components)
            {
                var sortedSubComponents = 
                    SubComponents.OrderBy(sc => sc.Card.Name).ToArray();

                float X0 = X + Constants.GUI.C;
                float Y0 = Y + Constants.GUI.D;

                for (int i = 0; i < sortedSubComponents.Length; i++)
                {
                    var sc = sortedSubComponents[i];
                    sc.X = X0;
                    sc.Y = Y0 - i * Constants.GUI.E;
                }
            }
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
            Width = Constants.GUI.H;
            Height = Constants.GUI.I;

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
                X + Constants.GUI.A, 
                Y + Constants.GUI.B,
                Fonts.MainFont14,
                Fonts.BigRenderOptions,
                QuickFont.QFontAlignment.Left);

            drawAdapter.DrawText(
                CardName,
                X + Constants.GUI.F,
                Y + Constants.GUI.G,
                Fonts.MainFont10,
                Fonts.BigRenderOptions,
                QuickFont.QFontAlignment.Centre);
        }
    }
}
