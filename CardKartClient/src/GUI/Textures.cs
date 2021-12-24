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
        public static Texture NoPortait;
        public static Texture ZZZ;
        public static Texture Hound1;
        public static Texture Hero1;
        public static Texture Health1;

        public static void LoadTextures()
        {
            Frame1 = TextureLoader.CreateTexture(Properties.Resources.frame1);
            Frame1_Monster = TextureLoader.CreateTexture(Properties.Resources.frame1_monster);
            Goblin1 = TextureLoader.CreateTexture(Properties.Resources.goblin1);
            Token1 = TextureLoader.CreateTexture(Properties.Resources.token1);
            Zap1 = TextureLoader.CreateTexture(Properties.Resources.zap1);
            Zombie1 = TextureLoader.CreateTexture(Properties.Resources.zombie1);
            NoPortait = TextureLoader.CreateTexture(Properties.Resources.noportrait);
            ZZZ = TextureLoader.CreateTexture(Properties.Resources.zzz);
            Hound1 = TextureLoader.CreateTexture(Properties.Resources.hound1);
            Hero1 = TextureLoader.CreateTexture(Properties.Resources.hero1);
            Health1 = TextureLoader.CreateTexture(Properties.Resources.health1);
        }

        public static Texture Portraits(CardTemplates template)
        {
            switch (template)
            {
                case CardTemplates.AngryGoblin: { return Goblin1; }
                case CardTemplates.ArmoredZombie: { return Zombie1; }
                case CardTemplates.Zap: { return Zap1; }
                case CardTemplates.DepravedBloodhound: { return Hound1; }
                case CardTemplates.HeroTest: { return Hero1; }
                default: { return NoPortait; }
            }
        }
    }
}
