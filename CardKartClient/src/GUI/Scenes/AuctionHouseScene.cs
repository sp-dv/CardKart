using CardKartClient.GUI.Components;
using CardKartShared.GameState;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CardKartClient.GUI.Scenes
{
    internal class AuctionHouseScene : Scene
    {
        public SmartTextPanel QuoteDisplay;
        public TextInputBox NameInput;
        public SmartTextPanel BackButton;
        public SmartTextPanel TopResultsDisplay;

        private List<(string, CardTemplates)> TemplateWords;

        public CardInfoPanel CurrentCardInfo;


        public AuctionHouseScene()
        {
            TemplateWords = new List<(string, CardTemplates)>();
            foreach (var card in Card.AllRealCards)
            {
                if (!card.IsInPacks) { continue; }
                TemplateWords.Add((card.Name.ToLower(), card.Template));
            }

            QuoteDisplay = new SmartTextPanel();
            QuoteDisplay.Width = 0.4f;
            QuoteDisplay.Height = 0.1f;
            QuoteDisplay.Font = Fonts.MainFont14;
            QuoteDisplay.Y = 0.4f;
            Components.Add(QuoteDisplay);

            TopResultsDisplay= new SmartTextPanel();
            TopResultsDisplay.Width = 0.4f;
            TopResultsDisplay.Height = 0.1f;
            TopResultsDisplay.BackgroundColor = Color.Orange;
            TopResultsDisplay.Font = Fonts.MainFont14;
            TopResultsDisplay.X = -0.6f;
            TopResultsDisplay.Y = 0.7f;
            Components.Add(TopResultsDisplay);


            NameInput = new TextInputBox();
            NameInput.Width = 0.4f;
            NameInput.Height = 0.1f;
            NameInput.X = -0.6f;
            NameInput.Y = 0.8f;
            NameInput.Done += () => {
                var inputWord = NameInput.Text;
                if (inputWord == null || inputWord.Length == 0) { return; }

                var pairs =
                    TemplateWords
                    .Select(pair => (StringDistance(pair.Item1, inputWord.ToLower()) - pair.Item1.Length, pair.Item2, pair.Item1))
                    .OrderBy(pair => pair.Item1)
                    .ToList();

                var template = pairs[0].Item2;
                var name = pairs[0].Item3;

                CurrentCardInfo.SetCard(new Card(template));
                NameInput.ClearText();
                TopResultsDisplay.Text = "";
                TopResultsDisplay.Layout();

                var quote = CardKartClient.Server.GetQuote(template);
                QuoteDisplay.Text = $"Bid: {quote.Quote?.Bid}\nAsk: {quote.Quote?.Ask}";
                QuoteDisplay.Layout();

            };
            NameInput.TextChanged += () => { 
                var inputWord = NameInput.Text;
                if (inputWord == null || inputWord.Length == 0) { return; }

                var pairs = 
                    TemplateWords
                    .Select(pair => (StringDistance(pair.Item1, inputWord.ToLower()) - pair.Item1.Length, pair.Item2, pair.Item1))
                    .OrderBy(pair => pair.Item1)
                    .ToList();

                var template = pairs[0].Item2;
                var name = pairs[0].Item3;

                TopResultsDisplay.Text = $"{pairs[0].Item3}";
                TopResultsDisplay.Layout();

                //var quote = CardKartClient.Server.GetQuote(template).Quote;
                //Quote.Text = $"{name}: {quote}";
                //Quote.Layout();
                //NameInput.ClearText();
            };
            NameInput.Layout();
            Components.Add(NameInput);


            BackButton = new SmartTextPanel();
            BackButton.Text = "Main Menu";
            BackButton.X = -0.95f;
            BackButton.Y = -0.85f;
            BackButton.Height = 0.1f;
            BackButton.BackgroundColor = Color.LightGray;
            BackButton.Layout();
            BackButton.Clicked += () => CardKartClient.Controller.ToMainMenu();
            Components.Add(BackButton);

            CurrentCardInfo = new CardInfoPanel();
            CurrentCardInfo.X = -0.6f; 
            CurrentCardInfo.Y = -0.8f; 
            Components.Add(CurrentCardInfo);
        }

        public static int StringDistance(string stringA, string stringB)
        {
            int lengthA = stringA.Length;
            int lengthB = stringB.Length;
            int[,] d = new int[lengthA + 1, lengthB + 1];

            if (lengthA == 0)
            {
                return lengthB;
            }

            if (lengthB == 0)
            {
                return lengthA;
            }

            for (int i = 0; i <= lengthA; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= lengthB; d[0, j] = j++)
            {
            }

            for (int i = 1; i <= lengthA; i++)
            {
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = (stringB[j - 1] == stringA[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[lengthA, lengthB];
        }
    }
}
