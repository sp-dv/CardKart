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
        public static QFont CardFont20 { get; private set; }
        public static QFont CardFont14 { get; private set; }
        public static QFont CardFont10 { get; private set; }
        public static QFont CardFont8 { get; private set; }
        public static QFont BreadTextFont14 { get; private set; }
        public static QFont BreadTextFont10 { get; private set; }
        public static QFont BreadTextFont8 { get; private set; }
        public static QFont MainFont14 { get; private set; }
        public static QFont MainFont10 { get; private set; }
        public static QFont MainFont8 { get; private set; }
        public static QFont MainFont6 { get; private set; }

        public static QFontRenderOptions MainRenderOptions { get; } =
            new QFontRenderOptions
            {
                DropShadowActive = false,
                Colour = Color.Black,
                WordSpacing = 0.2f,
                CharacterSpacing = 0.0f,
                
            };

        public static QFontRenderOptions RedRenderOptions { get; } =
            new QFontRenderOptions
            {
                DropShadowActive = false,
                Colour = Color.Red,
                WordSpacing = 0.2f,
                CharacterSpacing = 0.0f,

            };

        public static QFontRenderOptions BigRenderOptions { get; } =
            new QFontRenderOptions
            {
                DropShadowActive = false,
                Colour = Color.Black,
                WordSpacing = 0.4f,
                CharacterSpacing = 0.05f,

            };

        public static void LoadFonts()
        {
            var kerning = new QFontKerningConfiguration();

            var builderConfig = new QFontBuilderConfiguration(false)
            {
                Characters = CharacterSet.General,
                KerningConfig = kerning,
            };

            CardFont20 = new QFont("./gamefonts/matrixbold.ttf", 40, builderConfig);
            CardFont14 = new QFont("./gamefonts/matrixbold.ttf", 14, builderConfig);
            CardFont10 = new QFont("./gamefonts/matrixbold.ttf", 10, builderConfig);
            CardFont8 = new QFont("./gamefonts/matrixbold.ttf", 8, builderConfig);

            BreadTextFont14 = new QFont("./gamefonts/mplantin.ttf", 14, builderConfig);
            BreadTextFont10 = new QFont("./gamefonts/mplantin.ttf", 10, builderConfig);
            BreadTextFont8 = new QFont("./gamefonts/mplantin.ttf", 8, builderConfig);

            MainFont14 = new QFont("./gamefonts/ep1.ttf", 14, builderConfig);
            MainFont10 = new QFont("./gamefonts/ep1.ttf", 10, builderConfig);
            MainFont8 = new QFont("./gamefonts/ep1.ttf", 8, builderConfig);
            MainFont6 = new QFont("./gamefonts/ep1.ttf", 6, builderConfig);
        }
    }
}
