using CardKartShared.GameState;
using SGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardKartClient.GUI.Components
{
    internal class ManaButtonBar : GuiComponent
    {
        public ManaBallButton RedButton;
        public ManaBallButton GreenButton;
        public ManaBallButton WhiteButton;
        public ManaBallButton BlackButton;
        public ManaBallButton PurpleButton;
        public ManaBallButton BlueButton;
        public ManaBallButton ColourlessButton;

        public delegate void ColourClickedHandler(OptionChoice colour);
        public event ColourClickedHandler ColourClicked;

        public ManaButtonBar()
        {
            Height = 0.05f;
            Width = 0.4f;

            RedButton = new ManaBallButton(ManaColour.Red);
            RedButton.Clicked += () => ColourClicked?.Invoke(OptionChoice.Red);
            Components.Add(RedButton);

            GreenButton = new ManaBallButton(ManaColour.Green);
            GreenButton.Clicked += () => ColourClicked?.Invoke(OptionChoice.Green);
            Components.Add(GreenButton);

            WhiteButton = new ManaBallButton(ManaColour.White);
            WhiteButton.Clicked += () => ColourClicked?.Invoke(OptionChoice.White);
            Components.Add(WhiteButton);

            BlackButton = new ManaBallButton(ManaColour.Black);
            BlackButton.Clicked += () => ColourClicked?.Invoke(OptionChoice.Black);
            Components.Add(BlackButton);

            PurpleButton = new ManaBallButton(ManaColour.Purple);
            PurpleButton.Clicked += () => ColourClicked?.Invoke(OptionChoice.Purple);
            Components.Add(PurpleButton);

            BlueButton = new ManaBallButton(ManaColour.Blue);
            BlueButton.Clicked += () => ColourClicked?.Invoke(OptionChoice.Blue);
            Components.Add(BlueButton);

            ColourlessButton = new ManaBallButton(ManaColour.Colourless);
            ColourlessButton.Clicked += () => ColourClicked?.Invoke(OptionChoice.Colourless);
            Components.Add(ColourlessButton);
        }


        public void Update(ManaSet manaSet)
        {
            RedButton.Text = manaSet.Red.ToString();
            GreenButton.Text = manaSet.Green.ToString();
            WhiteButton.Text = manaSet.White.ToString();
            BlackButton.Text = manaSet.Black.ToString();
            PurpleButton.Text = manaSet.Purple.ToString();
            BlueButton.Text = manaSet.Blue.ToString();
            ColourlessButton.Text = manaSet.Colourless.ToString();
        }

        public void Update(ManaSet currentMana, ManaSet maxMana)
        {
            RedButton.Text = currentMana.Red.ToString() + "/" + maxMana.Red.ToString();
            GreenButton.Text = currentMana.Green.ToString() + "/" + maxMana.Green.ToString();
            WhiteButton.Text = currentMana.White.ToString() + "/" + maxMana.White.ToString();
            BlackButton.Text = currentMana.Black.ToString() + "/" + maxMana.Black.ToString();
            PurpleButton.Text = currentMana.Purple.ToString() + "/" + maxMana.Purple.ToString();
            BlueButton.Text = currentMana.Blue.ToString() + "/" + maxMana.Blue.ToString();
            ColourlessButton.Text = currentMana.Colourless.ToString() + "/" + maxMana.Colourless.ToString();
        }

        public void Layout()
        {
            float xpadding = RedButton.Width + 0.004f;

            RedButton.X = X + (xpadding * 0);
            RedButton.Y = Y;

            GreenButton.X = X + (xpadding * 2);
            GreenButton.Y = Y;

            PurpleButton.X = X + (xpadding * 3);
            PurpleButton.Y = Y;

            WhiteButton.X = X + (xpadding * 4);
            WhiteButton.Y = Y;

            BlackButton.X = X + (xpadding * 1);
            BlackButton.Y = Y;

            BlueButton.X = X + (xpadding * 5);
            BlueButton.Y = Y;

            ColourlessButton.X = X + (xpadding * 6);
            ColourlessButton.Y = Y;
        }


        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
        }
    }
}
