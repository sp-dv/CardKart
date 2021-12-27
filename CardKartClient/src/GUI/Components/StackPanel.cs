using CardKartShared.GameState;
using CardKartShared.Util;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CardKartClient.GUI.Components
{
    internal class StackPanel : GuiComponent
    {
        // Absolutely repulsive hack to get card to draw on top.
        private CardComponent Top;
        private CardComponent PrevTop;

        private CastingStack Stack;

        public delegate void TargetsUpdatedHandler(CardComponent caster, IEnumerable<int> targetIDs);
        public event TargetsUpdatedHandler TargetsUpdated;

        public StackPanel(CastingStack stack)
        {
            Width = 0.24f;
            Height = 1f;

            Stack = stack;
            Stack.StackChanged += Update;
        }

        private void Update(Card[] cards)
        {
            var dummy = new CardComponent(null);
            var cardHeight = dummy.Height;
            var cardWidth = dummy.Width;

            var y0 = Y + Height - cardHeight;

            Top = null;
            PrevTop = null;
            TargetsUpdated?.Invoke(null, null);

            lock (Components)
            {
                Components.Clear();
                for (int i = 0; i < cards.Length; i++)
                {
                    var component =
                        new CardComponent(cards[cards.Length - i - 1]);
                    Components.Add(component);
                    component.X = X + 0.01f;
                    component.Y = y0 - i * 0.05f;
                    int stackIndex = cards.Length - i - 1;
                    component.MouseMovedEvent += () =>
                    {
                        // This depends on components being iterated in reverse order and
                        // the bottom most card having the MouseMovedEvent fire first.
                        if (Top == null)
                        {
                            Top = component;
                            if (PrevTop != component)
                            {
                                // Synthetic mouse entered...
                                var targets = Stack.GetTargetIDs(stackIndex);
                                TargetsUpdated?.Invoke(component, targets);
                            }
                        }
                    };
                    component.MouseExitedEvent += () =>
                    {
                        if (PrevTop == component)
                        {
                            PrevTop = null;
                            From = null;
                            To = null;
                            TargetsUpdated?.Invoke(null, null);
                        }
                    };
                }
            }
        }

        private GLCoordinate From;
        private GLCoordinate[] To;

        public override void Draw(DrawAdapter drawAdapter)
        {
            if (Visible)
            {
                DrawInternal(drawAdapter);

                lock (Components)
                {
                    if (Top == null && PrevTop != null)
                    {
                        Top = PrevTop;
                    }
                    foreach (var child in Components)
                    {
                        if (child != Top) { child.Draw(drawAdapter); }
                    }
                    if (Top != null)
                    {
                        Top.Draw(drawAdapter);
                    }
                    PrevTop = Top;
                    Top = null;
                }
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.Silver);
        }
    }
}
