using CardKartShared.Util;
using OpenTK.Input;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CardKartClient.GUI.Components
{
    internal class TextInputBox : GuiComponent
    {
        public bool HideBehindStars;

        private SmartTextPanel InternalTextPanel;

        public string Text { get; private set; }

        public delegate void SomethingHappenedHandler();
        public event SomethingHappenedHandler Done;
        public event SomethingHappenedHandler TextChanged;

        public TextInputBox()
        {
            InternalTextPanel = new SmartTextPanel();
            InternalTextPanel.Clicked += () =>
            {
                RequestFocus();
            };
            Components.Add(InternalTextPanel);
        }

        public void SetText(string text)
        {
            if (text == null) { return; }

            Text = text;
            InternalTextPanel.Text = text;
            Layout();

            TextChanged?.Invoke();
        }

        public void ClearText()
        {
            Text = "";
            InternalTextPanel.Text = Text;
            Layout();

            TextChanged?.Invoke();
        }

        public void Layout()
        {
            InternalTextPanel.X = X;
            InternalTextPanel.Y = Y;
            InternalTextPanel.Width = Width;
            InternalTextPanel.Height = Height;
            InternalTextPanel.Font = Fonts.MainFont14;
            InternalTextPanel.Layout();
        }

        public override void HandleKeyboardEvent(KeyboardKeyEventArgs args)
        {
            // Hack to make TextChanged work in a sane manner.
            var originalTextLength = Text != null ? Text.Length : 0;

            if (args.Key >= Key.A && args.Key <= Key.Z)
            {
                string c = args.Key.ToString();
                if (args.Shift) { c = c.ToUpper(); }
                else { c = c.ToLower(); }
             
                Text += c;
            }
            else if (args.Key >= Key.Number0 && args.Key <= Key.Number9)
            {
                string c = args.Key.ToString();
                c = "" + c[c.Length - 1]; // 'Number4' -> '4'
                if (args.Shift) { c = c.ToUpper(); }
                else { c = c.ToLower(); }

                Text += c;
            }
            else if (args.Key == Key.Space)
            {
                Text += ' ';
            }
            else if (args.Key == Key.BackSpace)
            {
                if (Text.Length > 0)
                {
                    Text = Text.Remove(Text.Length - 1, 1);
                }
            }
            else if (args.Key == Key.Enter)
            {
                Done?.Invoke();
            }
            else { return; }

            if (HideBehindStars)
            {
                InternalTextPanel.Text = new string(Text.ToCharArray().SelectMany(c => new[] { '*', ' ' }).ToArray());
            }
            else
            {
                InternalTextPanel.Text = Text;
            }

            Layout();

            if (Text.Length != originalTextLength)
            {
                TextChanged?.Invoke();
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.FillRectangle(X, Y, X + Width, Y + Height, 
                IsFocused ? Color.LightGray : Color.White);
        }
    }
}
