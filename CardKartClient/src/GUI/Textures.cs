using CardKartShared.GameState;
using SGL;
using System;

namespace CardKartClient.GUI
{
    internal static class Textures
    {
        public static Texture Frame1;
        public static Texture Frame1_Monster;
        public static Texture Goblin1;
        public static Texture Token1;
        public static Texture Zap1;
        public static Texture Zombie1;

        public static void LoadTextures()
        {
            Frame1 = TextureLoader.CreateTexture(Properties.Resources.frame1);
            Frame1_Monster = TextureLoader.CreateTexture(Properties.Resources.frame1_monster);
            Goblin1 = TextureLoader.CreateTexture(Properties.Resources.goblin1);
            Token1 = TextureLoader.CreateTexture(Properties.Resources.token1);
            Zap1 = TextureLoader.CreateTexture(Properties.Resources.zap1);
            Zombie1 = TextureLoader.CreateTexture(Properties.Resources.zombie1);
        }

        public static Texture Portraits(CardTemplates template)
        {
            switch (template)
            {
                case CardTemplates.AngryGoblin: { return Textures.Goblin1; }
                case CardTemplates.ArmoredZombie: { return Textures.Zombie1; }
                case CardTemplates.Zap: { return Textures.Zap1; }
                default: throw new Exception();
            }
        }
    }
}
