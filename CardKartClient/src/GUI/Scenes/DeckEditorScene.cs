using CardKartClient.GUI;
using CardKartClient.GUI.Components;
using CardKartShared.GameState;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CardKartClient.GUI.Scenes
{
    internal class DeckEditorScene : Scene
    {
        private ModifiableCardComponent[] CardComponents;
        private Card[] AllCards;

        private SmartTextPanel NextButton;
        private SmartTextPanel PreviousButton;

        private int CurrentPage = 0;

        private const int CardsPerRow = 4;
        private const int CardsPerColumn = 3;
        private const int CardsPerPage = CardsPerRow * CardsPerColumn;  


        public DeckEditorScene()
        {
            AllCards = Enum.GetValues(typeof(CardTemplates)).Cast<CardTemplates>().Select(template => new Card(template)).ToArray();

            var cardComponentList = new List<ModifiableCardComponent>();

            for (int i = 0; i < CardsPerRow; i++)
            {
                for (int j = 0; j < CardsPerColumn; j++)
                {
                    var mc = new ModifiableCardComponent();
                    mc.X = -0.5f + (i * 0.3f);
                    mc.Y = -0.8f + (j * 0.52f);
                    cardComponentList.Add(mc);
                    Components.Add(mc);
                }
            }

            CardComponents = cardComponentList.ToArray();

            NextButton = new SmartTextPanel();
            NextButton.Width = 0.1f;
            NextButton.Height = 0.1f;
            NextButton.X = 0.7f;
            NextButton.BackgroundColor = Color.Green;
            NextButton.Clicked += () => {
                CurrentPage++;
                UpdateCards();
            };
            Components.Add(NextButton);

            PreviousButton = new SmartTextPanel();
            PreviousButton.Width = 0.1f;
            PreviousButton.Height = 0.1f;
            PreviousButton.X = -0.7f;
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
