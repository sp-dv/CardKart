using CardKartShared.GameState;
using CardKartShared.Util;
using SGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CardKartClient.GUI.Components
{
    internal class TokenComponent : GuiComponent
    {
        public Card Card;

        private Texture PortraitTexture;
        private Color PaletteColor;
        public Color? HighlightColor;

        public TokenComponent(Card card)
        {
            Card = card;

            Width = 0.2f;
            Height = 0.2f;

            Layout();
        }

        private void Layout()
        {
            if (Card != null)
            {
                PortraitTexture = Textures.Portraits(Card.Template);
                PaletteColor = Constants.PaletteColor(Card.Colour);
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.DrawSprite(
                X + 0.05f,
                Y + 0.05f,
                X + 0.15f,
                Y + 0.18f,
                PortraitTexture);

            drawAdapter.DrawSprite(
                X, 
                Y, 
                X + Width, 
                Y + Height, 
                Textures.Token1, 
                PaletteColor);

            if (HighlightColor.HasValue)
            {
                drawAdapter.DrawRectange(X, Y, X+Width,Y+Height, HighlightColor.Value);
            }

            if (Card.Token != null)
            {
                var token = Card.Token;

                drawAdapter.DrawText(token.Attack.ToString(), X + 0.065f, Y + 0.065f, Fonts.MainFont10, Fonts.MainRenderOptions);
                drawAdapter.DrawText(token.CurrentHealth.ToString(), X + 0.125f, Y + 0.065f, Fonts.MainFont10, Fonts.MainRenderOptions);

                if (token.Exhausted)
                {
                    drawAdapter.DrawSprite(
                        X + Width/2, 
                        Y + Height - 0.05f, 
                        X + Width/2 + 0.05f, 
                        Y + Height, 
                        Textures.ZZZ);
                }
            }
        }
    }
}
