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

        private Texture FrameTexture;

        public TokenComponent(Card card)
        {
            Card = card;

            Width = 0.14f;
            Height = 0.28f;

            Layout();
        }

        private void Layout()
        {
            if (Card != null)
            {
                PortraitTexture = Textures.Portraits(Card.Template);
                PaletteColor = Constants.PaletteColor(Card.Colour);
                FrameTexture = Card.Token.IsRelic ? Textures.TokenRelic1 : Textures.Token1;
            }
        }

        protected override void DrawInternal(DrawAdapter drawAdapter)
        {
            drawAdapter.DrawSprite(
                X + 0.031f,
                Y + 0.075f,
                X + 0.105f,
                Y + 0.248f,
                PortraitTexture);

            drawAdapter.DrawSprite(
                X, 
                Y, 
                X + Width, 
                Y + Height, 
                FrameTexture, 
                PaletteColor);

            if (HighlightColor.HasValue)
            {
                drawAdapter.DrawRectangle(X, Y, X+Width,Y+Height, HighlightColor.Value);
            }

            if (Card.Token != null)
            {
                var token = Card.Token;

                if (Card.Token.IsCreature)
                {
                    drawAdapter.DrawText(
                        token.Attack.ToString(), 
                        X + 0.05f, 
                        Y + 0.073f, 
                        Fonts.MainFont10, 
                        Fonts.MainRenderOptions, 
                        QuickFont.QFontAlignment.Centre);

                    drawAdapter.DrawText(
                        token.CurrentHealth.ToString(), 
                        X + 0.089f, 
                        Y + 0.073f, 
                        Fonts.MainFont10, 
                        Fonts.MainRenderOptions,
                        QuickFont.QFontAlignment.Centre);
                }

                if (token.Stunned)
                {
                    drawAdapter.DrawSprite(
                        X + Width / 2 - 0.05f,
                        Y + Height - 0.1f,
                        X + Width / 2 + 0.05f,
                        Y + Height + 0.0f,
                        Textures.CCC1);
                }
                else if (token.IsExhausted)
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
