using CardKartShared.GameState;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CardKartClient.GUI.Components
{
    internal class ChooserPanel : GuiComponent
    {
        private ChoiceHelper ChoiceHelper;

        public Button PassButton;
        public TextPanel MainText;

        public delegate void OptionClickedHandler(OptionChoice optionChoice);
        public event OptionClickedHandler OptionClicked;

        public ChooserPanel(ChoiceHelper choiceHelper)
        {
            Width = 0.3f;
            Height = 0.3f;

            PassButton = new Button();
            PassButton.Text = "Pass";

            MainText = new TextPanel();
            MainText.Text = "Text here lorep ipscum.";

            ChoiceHelper = choiceHelper;
            ChoiceHelper.RequestGUIUpdate += Update;
        }

        private void Update()
        {
            MainText.Text = ChoiceHelper.Text;
            PassButton.Visible = ChoiceHelper.ShowPass;
        }

        public void Layout()
        {
            PassButton.X = X + 0.1f;
            PassButton.Y = Y + 0.05f;
            PassButton.Width = 0.12f;
            PassButton.Height = 0.07f;

            MainText.X = X + 0.02f;
            MainText.Y = Y + Height - 0.05f;
            MainText.Width = Width - 0.04f;
            MainText.Height = 0.08f;
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.GreenYellow);

            PassButton.Draw(drawAdapter);
            MainText.Draw(drawAdapter);
        }

        protected override bool HandleClickInternal(GLCoordinate location)
        {
            if (PassButton.ComponentRectangleContains(location))
            {
                OptionClicked?.Invoke(OptionChoice.Pass);
                return true;
            }
            return false;
        }
    }
}
