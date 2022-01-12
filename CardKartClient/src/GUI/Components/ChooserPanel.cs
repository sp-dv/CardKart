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
        public SmartTextPanel YesButton;
        public SmartTextPanel NoButton;

        public SmartTextPanel MainText;
        public ManaButtonBar ManaChoices;

        public CardChoicePanel CardChoicePanel;
        public AbilityChoicePanel AbilityChoicePanel;

        public delegate void OptionClickedHandler(OptionChoice optionChoice);
        public event OptionClickedHandler OptionClicked;

        public event AbilityChoicePanel.AbilityClickedHandler AbilityClicked;


        public ChooserPanel(ChoiceHelper choiceHelper)
        {
            Width = 0.4f;
            Height = 0.4f;

            PassButton = new SmartTextPanel();
            PassButton.Text = "Pass Turn";
            PassButton.Alignment = QuickFont.QFontAlignment.Centre;
            PassButton.BackgroundImage = Textures.Button1;
            PassButton.Clicked += () => OptionClicked?.Invoke(OptionChoice.Pass);
            Components.Add(PassButton);

            OkButton = new SmartTextPanel();
            OkButton.Text = "OK";
            OkButton.BackgroundImage = Textures.Button1;
            OkButton.Alignment = QuickFont.QFontAlignment.Centre;
            OkButton.Clicked += () => OptionClicked?.Invoke(OptionChoice.Ok);
            Components.Add(OkButton);

            CancelButton = new SmartTextPanel();
            CancelButton.Text = "Cancel";
            CancelButton.BackgroundImage = Textures.Button1;
            CancelButton.Alignment = QuickFont.QFontAlignment.Centre;
            CancelButton.Clicked += () => OptionClicked?.Invoke(OptionChoice.Cancel);
            Components.Add(CancelButton);

            YesButton = new SmartTextPanel();
            YesButton.Text = "Yes";
            YesButton.BackgroundImage = Textures.Button1;
            YesButton.Alignment = QuickFont.QFontAlignment.Centre;
            YesButton.Clicked += () => OptionClicked?.Invoke(OptionChoice.Yes);
            Components.Add(YesButton);

            NoButton = new SmartTextPanel();
            NoButton.Text = "No";
            NoButton.BackgroundImage = Textures.Button1;
            NoButton.Alignment = QuickFont.QFontAlignment.Centre;
            NoButton.Clicked += () => OptionClicked?.Invoke(OptionChoice.No);
            Components.Add(NoButton);

            MainText = new SmartTextPanel();
            Components.Add(MainText);

            ManaChoices = new ManaButtonBar();
            ManaChoices.ColourClicked += 
                (colour) => OptionClicked?.Invoke(colour);
            Components.Add(ManaChoices);

            AbilityChoicePanel = new AbilityChoicePanel();
            AbilityChoicePanel.AbilityClicked += ability => AbilityClicked?.Invoke(ability);
            Components.Add(AbilityChoicePanel);

            ChoiceHelper = choiceHelper;
            ChoiceHelper.RequestGUIUpdate += Update;
        }

        private void Update()
        {
            MainText.Text = ChoiceHelper.Text;
            MainText.Layout();

            PassButton.Visible = ChoiceHelper.ShowPass;
            OkButton.Visible = ChoiceHelper.ShowOk;
            CancelButton.Visible = ChoiceHelper.ShowCancel;
            YesButton.Visible = ChoiceHelper.ShowYes;
            NoButton.Visible = ChoiceHelper.ShowNo;

            ManaChoices.Visible = ChoiceHelper.ShowManaChoices;

            CardChoicePanel.Update(ChoiceHelper.CardChoices);
            CardChoicePanel.Visible = ChoiceHelper.CardChoices != null;

            AbilityChoicePanel.SetChoices(ChoiceHelper.AbilityChoices);
            AbilityChoicePanel.Visible = ChoiceHelper.AbilityChoices != null;
        }

        public void Layout()
        {
            PassButton.X = X + 0.2f;
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

            YesButton.X = X + 0.05f;
            YesButton.Y = Y + 0.15f;
            YesButton.Width = 0.12f;
            YesButton.Height = 0.07f;
            YesButton.Layout();

            NoButton.X = X + 0.2f;
            NoButton.Y = Y + 0.15f;
            NoButton.Width = 0.12f;
            NoButton.Height = 0.07f;
            NoButton.Layout();

            MainText.X = X + 0.02f;
            MainText.Y = Y + Height - 0.1f;
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
        }
    }
}
