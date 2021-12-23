using QuickFont;
using QuickFont.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CardKartClient.GUI
{
    static class Fonts
    {
        public static QFont MainFont10 { get; private set; }
        public static QFont MainFont6 { get; private set; }

        public static QFontRenderOptions MainRenderOptions { get; } =
            new QFontRenderOptions
            {
                DropShadowActive = false,
                Colour = Color.Black,
                WordSpacing = 0.5f
            };

        static Fonts()
        {
            var builderConfig = new QFontBuilderConfiguration(true)
            {
                ShadowConfig =
                {
                    blurRadius = 0,
                    blurPasses = 0,
                    Type = ShadowType.Blurred
                },
                TextGenerationRenderHint = TextGenerationRenderHint.ClearTypeGridFit,
                Characters = CharacterSet.General
            };

            MainFont10 = new QFont("Fonts/times.ttf", 10, builderConfig);
            MainFont6 = new QFont("Fonts/times.ttf", 6, builderConfig);
        }
    }
}
