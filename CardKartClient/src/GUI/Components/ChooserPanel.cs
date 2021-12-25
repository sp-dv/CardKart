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

        public SmartTextPanel PassButton;
        public SmartTextPanel OkButton;
        public SmartTextPanel CancelButton;

        public SmartTextPanel MainText;
        public ManaButtonBar ManaChoices;

        public CardChoicePanel CardChoicePanel;

        public delegate void OptionClickedHandler(OptionChoice optionChoice);
        public event OptionClickedHandler OptionClicked;

        public ChooserPanel(ChoiceHelper choiceHelper)
        {
            Width = 0.4f;
            Height = 0.4f;

            PassButton = new SmartTextPanel();
            PassButton.Text = "Pass Turn";
            PassButton.Clicked += () => OptionClicked?.Invoke(OptionChoice.Pass);

            OkButton = new SmartTextPanel();
            OkButton.Text = "OK";
            OkButton.Clicked += () => OptionClicked?.Invoke(OptionChoice.Ok);

            CancelButton = new SmartTextPanel();
            CancelButton.Text = "Cancel";
            CancelButton.Clicked += () => OptionClicked?.Invoke(OptionChoice.Cancel);

            MainText = new SmartTextPanel();

            ManaChoices = new ManaButtonBar();
            ManaChoices.ColourClicked += 
                (colour) => OptionClicked?.Invoke(colour);

            ChoiceHelper = choiceHelper;
            ChoiceHelper.RequestGUIUpdate += Update;
        }

        private void Update()
        {
            MainText.Text = ChoiceHelper.Text;

            PassButton.Visible = ChoiceHelper.ShowPass;
            OkButton.Visible = ChoiceHelper.ShowOk;
            CancelButton.Visible = ChoiceHelper.ShowCancel;

            ManaChoices.Visible = ChoiceHelper.ShowManaChoices;

            CardChoicePanel.Update(ChoiceHelper.CardChoices);
            CardChoicePanel.Visible = ChoiceHelper.CardChoices != null;
        }

        public void Layout()
        {
            PassButton.X = X + 0.1f;
            PassButton.Y = Y + 0.15f;
            PassButton.Width = 0.12f;
            PassButton.Height = 0.07f;
            PassButton.Layout();

            OkButton.X = X + 0.06f;
            OkButton.Y = Y + 0.15f;
            OkButton.Width = 0.12f;
            OkButton.Height = 0.07f;
            OkButton.Layout();

            CancelButton.X = X + 0.2f;
            CancelButton.Y = Y + 0.15f;
            CancelButton.Width = 0.12f;
            CancelButton.Height = 0.07f;
            CancelButton.Layout();

            MainText.X = X + 0.02f;
            MainText.Y = Y + Height - 0.05f;
            MainText.Width = Width - 0.04f;
            MainText.Height = 0.08f;
            MainText.Layout();

            ManaChoices.X = X + 0.01f;
            ManaChoices.Y = Y + 0.015f;
            ManaChoices.Layout();
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, Color.Silver);

            PassButton.Draw(drawAdapter);
            OkButton.Draw(drawAdapter);
            CancelButton.Draw(drawAdapter);

            MainText.Draw(drawAdapter);
            ManaChoices.Draw(drawAdapter);
        }

        protected override void HandleClickInternal(GLCoordinate location)
        {
            if (PassButton.HandleClick(location)) { }
            else if (OkButton.HandleClick(location)) { }
            else if (CancelButton.HandleClick(location)) { }
            else if (ManaChoices.HandleClick(location)) { }
            else if (ManaChoices.HandleClick(location)) { }
        }
    }
}
