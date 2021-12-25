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
        public static QFont CardFont10 { get; set; }
        public static QFont CardFont8 { get; set; }
        public static QFont MainFont10 { get; private set; }
        public static QFont MainFont6 { get; private set; }

        public static QFontRenderOptions MainRenderOptions { get; } =
            new QFontRenderOptions
            {
                DropShadowActive = false,
                Colour = Color.Black,
                WordSpacing = 0.2f,
                CharacterSpacing = 0.0f,
                
            };

        public static void LoadFonts()
        {
            var kerning = new QFontKerningConfiguration();

            var builderConfig = new QFontBuilderConfiguration(false)
            {
                Characters = CharacterSet.General,
                KerningConfig = kerning,
            };
            CardFont10 = new QFont("./gamefonts/cooper.ttf", 10, builderConfig);
            CardFont8 = new QFont("./gamefonts/cooper.ttf", 8, builderConfig);

            MainFont10 = new QFont("./gamefonts/ep1.ttf", 10, builderConfig);
            MainFont6 = new QFont("Fonts/times.ttf", 6, builderConfig);
        }
    }
}
