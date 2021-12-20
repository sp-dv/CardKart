using SGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardKartClient.GUI
{
    internal static class Textures
    {
        public static Texture Frame1;
        public static Texture Frame1_Monster;
        public static Texture Goblin1;

        public static void LoadTextures()
        {
            Frame1 = TextureLoader.CreateTexture(Properties.Resources.frame1);
            Frame1_Monster = TextureLoader.CreateTexture(Properties.Resources.frame1_monster);
            Goblin1 = TextureLoader.CreateTexture(Properties.Resources.goblin1);
        }
    }
}
