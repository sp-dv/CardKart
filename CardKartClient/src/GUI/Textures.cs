using CardKartShared.GameState;
using SGL;
using System;

namespace CardKartClient.GUI
{
    internal static class Textures
    {
        public static Texture Frame1_Spell;
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
        public static Texture AlterFate1;
        public static Texture Graveyard1;
        public static Texture Button1;
        public static Texture Enlarge1;
        public static Texture GoblinBombsmith1;
        public static Texture MindProbe1;
        public static Texture StandardBearer1;
        public static Texture MindFlay1;
        public static Texture VolcanicHatchling1;
        public static Texture RegeneratingZombie1;
        public static Texture Counterspell1;
        public static Texture MindSlip1;
        public static Texture SuckerPunch1;
        public static Texture ScribeMagi1;
        public static Texture Unmake1;
        public static Texture HorsemanOfDeath1;
        public static Texture Logo1stEdition;
        public static Texture CallToArms1;
        public static Texture SquireToken1;
        public static Texture InsectToken1;
        public static Texture ExiledScientist1;
        public static Texture Eliminate1;
        public static Texture SilvervenomSpider1;
        public static Texture Inspiration1;
        public static Texture CourtInformant1;
        public static Texture BattlehardenedMage1;
        public static Texture ArcticWatchman1;
        public static Texture HawkToken1;
        public static Texture ButtonNext1;
        public static Texture ButtonPrev1;
        public static Texture DeckEditorBG1;


        public static void LoadTextures()
        {
            Frame1_Spell = TextureLoader.CreateTexture(Properties.Resources.frame1_spell);
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
            AlterFate1 = TextureLoader.CreateTexture(Properties.Resources.alterFate1);
            Graveyard1 = TextureLoader.CreateTexture(Properties.Resources.graveyard1);
            Button1 = TextureLoader.CreateTexture(Properties.Resources.button1);
            Enlarge1 = TextureLoader.CreateTexture(Properties.Resources.enlarge1);
            GoblinBombsmith1 = TextureLoader.CreateTexture(Properties.Resources.goblinbombsmith1);
            MindProbe1 = TextureLoader.CreateTexture(Properties.Resources.mindprobe1);
            StandardBearer1 = TextureLoader.CreateTexture(Properties.Resources.standardbearer1);
            MindFlay1 = TextureLoader.CreateTexture(Properties.Resources.mindflay1);
            VolcanicHatchling1 = TextureLoader.CreateTexture(Properties.Resources.volcanichatchling1);
            RegeneratingZombie1 = TextureLoader.CreateTexture(Properties.Resources.regeneratingzombie1);
            Counterspell1 = TextureLoader.CreateTexture(Properties.Resources.counterspell1);
            MindSlip1 = TextureLoader.CreateTexture(Properties.Resources.mindslip1);
            SuckerPunch1 = TextureLoader.CreateTexture(Properties.Resources.suckerpunch1);
            ScribeMagi1 = TextureLoader.CreateTexture(Properties.Resources.scribemagi1);
            Unmake1 = TextureLoader.CreateTexture(Properties.Resources.unmake1);
            HorsemanOfDeath1 = TextureLoader.CreateTexture(Properties.Resources.horsemanofdeath1);
            Logo1stEdition = TextureLoader.CreateTexture(Properties.Resources.logo1e);
            CallToArms1 = TextureLoader.CreateTexture(Properties.Resources.calltoarms1);
            SquireToken1 = TextureLoader.CreateTexture(Properties.Resources.squiretoken1);
            ExiledScientist1 = TextureLoader.CreateTexture(Properties.Resources.exiledscientist1);
            InsectToken1 = TextureLoader.CreateTexture(Properties.Resources.insect1);
            Eliminate1 = TextureLoader.CreateTexture(Properties.Resources.eliminate1);
            SilvervenomSpider1 = TextureLoader.CreateTexture(Properties.Resources.silvervenomspider1);
            Inspiration1 = TextureLoader.CreateTexture(Properties.Resources.inspiration1);
            CourtInformant1 = TextureLoader.CreateTexture(Properties.Resources.courtinformant1);
            BattlehardenedMage1 = TextureLoader.CreateTexture(Properties.Resources.battlehardenedmage1);
            ArcticWatchman1 = TextureLoader.CreateTexture(Properties.Resources.arctichunter1);
            HawkToken1 = TextureLoader.CreateTexture(Properties.Resources.hawktoken1);
            ButtonNext1 = TextureLoader.CreateTexture(Properties.Resources.buttonnext1);
            ButtonPrev1 = TextureLoader.CreateTexture(Properties.Resources.buttonprev1);
            DeckEditorBG1 = TextureLoader.CreateTexture(Properties.Resources.deckeditorbg1);
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
                case CardTemplates.AlterFate: { return AlterFate1; }
                case CardTemplates.Enlarge: { return Enlarge1; }
                case CardTemplates.GolbinBombsmith: { return GoblinBombsmith1; }
                case CardTemplates.MindProbe: { return MindProbe1; }
                case CardTemplates.StandardBearer: { return StandardBearer1; }
                case CardTemplates.MindFlay: { return MindFlay1; }
                case CardTemplates.CrystalizedGeyser: { return VolcanicHatchling1; }
                case CardTemplates.RegeneratingZombie: { return RegeneratingZombie1; }
                case CardTemplates.Counterspell: { return Counterspell1; }
                case CardTemplates.MindSlip: { return MindSlip1; }
                case CardTemplates.SuckerPunch: { return SuckerPunch1; }
                case CardTemplates.ScribeMagi: { return ScribeMagi1; }
                case CardTemplates.Unmake: { return Unmake1; }
                case CardTemplates.HorsemanOfDeath: { return HorsemanOfDeath1; }
                case CardTemplates.CallToArms: { return CallToArms1; }
                case CardTemplates.SquireToken1: { return SquireToken1; }
                case CardTemplates.ExiledScientist: { return ExiledScientist1; }
                case CardTemplates.InsectToken1: { return InsectToken1; }
                case CardTemplates.Eliminate: { return Eliminate1; }
                case CardTemplates.SilvervenomSpider: { return SilvervenomSpider1; }
                case CardTemplates.Inspiration: { return Inspiration1; }
                case CardTemplates.CourtInformant: { return CourtInformant1; }
                case CardTemplates.BattlehardenedMage: { return BattlehardenedMage1; }
                case CardTemplates.ArcticWatchman: { return ArcticWatchman1; }
                case CardTemplates.HawkToken1: { return HawkToken1; }
                default: { return NoPortait; }
            }
        }

        public static Texture Frames(CardTypes cardType)
        {
            switch (cardType)
            {
                case CardTypes.Creature: { return Frame1_Monster; }
                case CardTypes.Scroll:
                case CardTypes.Channel:
                    { return Frame1_Spell; }
                default: { return Frame1_Spell; }
            }
        }
    }
}
