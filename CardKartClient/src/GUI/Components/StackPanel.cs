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
    internal class StackPanel : GuiComponent
    {
        // Absolutely repulsive hack to get card to draw on top.
        private CardComponent Top;

        private CastingStack Stack;

        public delegate void TargetsUpdatedHandler(CardComponent caster, IEnumerable<int> targetIDs);
        public event TargetsUpdatedHandler TargetsUpdated;

        public delegate void AbilityClickedHandler(AbilityCastingContext context);
        public event AbilityClickedHandler AbilityClicked;

        public event RequestInfoDisplayHandler RequestInfoDisplay;

        public StackPanel(CastingStack stack)
        {
            Width = 0.24f;
            Height = 1f;

            Stack = stack;
            Stack.StackChanged += Update;

        }

        private void Update(AbilityCastingContext[] contexts)
        {
            var dummy = new CardComponent(null);
            var cardHeight = dummy.Height;
            var cardWidth = dummy.Width;

            var y0 = Y + Height - cardHeight;

            Top = null;
            TargetsUpdated?.Invoke(null, null);

            var cards = contexts.Select(context => context.Card).ToArray();

            lock (Components)
            {
                Components.Clear();
                for (int i = 0; i < cards.Length; i++)
                {
                    int stackIndex = i;

                    var component =
                        new CardComponent(cards[stackIndex]);
                    Components.Add(component);
                    component.X = X + 0.01f;
                    component.Y = y0 - i * 0.05f;
                    component.Clicked += () => { 
                        AbilityClicked?.Invoke(contexts[stackIndex]);
                    };
                    component.MouseEnteredEvent += () =>
                    {
                        var targets = Stack.GetTargetIDs(stackIndex);
                        TargetsUpdated?.Invoke(component, targets);
                        RequestInfoDisplay?.Invoke(component.Card);
                        Top = component;
                    };
                    component.MouseExitedEvent += () =>
                    {
                        Top = null;
                        RequestInfoDisplay?.Invoke(null);
                        TargetsUpdated?.Invoke(null, null);
                    };
                }
            }
        }

        public override void Draw(DrawAdapter drawAdapter)
        {
            if (Visible)
            {
                DrawInternal(drawAdapter);

                lock (Components)
                {
                    foreach (var child in Components)
                    {
                        if (child != Top) { child.Draw(drawAdapter); }
                    }
                    if (Top != null)
                    {
                        Top.Draw(drawAdapter);
                    }
                }
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.Silver);
        }
    }
}
