using CardKartShared.GameState;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CardKartClient.GUI.Components
{
    internal class AbilityChoicePanel : GuiComponent
    {
        private float PaddingY = 0.02f;

        public delegate void AbilityClickedHandler(Ability ability);
        public event AbilityClickedHandler AbilityClicked;

        private Ability[] Abilities;

        public AbilityChoicePanel()
        {
            Width = 0.5f;
            Height = 0.56f;

        }

        public void SetChoices(IEnumerable<Ability> abilities)
        {
            if (abilities == null)
            {
                Abilities = new Ability[0];
            }
            else
            {
                Abilities = abilities.ToArray();
            }

            Layout();
        }

        private void Layout()
        {
            lock (Components)
            {
                Components.Clear();

                for (int i = 0; i < Abilities.Length; i++)
                {
                    var ability = Abilities[i];
                    var card = ability.Card;
                    var cardComponent = new CardComponent(card);
                    cardComponent.ForceBreadText(ability.BreadText);
                    cardComponent.X = X + i * cardComponent.Width;
                    cardComponent.Y = Y + PaddingY;
                    cardComponent.Clicked += () =>
                    {
                        AbilityClicked?.Invoke(ability);
                    };

                    Components.Add(cardComponent);
                }
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.SaddleBrown);
        }
    }
}
