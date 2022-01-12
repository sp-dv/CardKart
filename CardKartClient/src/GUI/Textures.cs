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
        public static Texture SavingGrace1;
        public static Texture DeepSeaMermaid1;
        public static Texture Conflagrate1;
        public static Texture Rapture1;
        public static Texture Overcharge1;
        public static Texture ControlBoar1;
        public static Texture Seblastian1;
        public static Texture TokenRelic1;
        public static Texture HauntedChapel1;
        public static Texture GhostToken1;
        public static Texture PortalJumper1;
        public static Texture PalaceGuard1;
        public static Texture Darkness1;
        public static Texture MistFiend1;
        public static Texture HorsemanOfWar1;
        public static Texture HorsemanOfFamine1;
        public static Texture HorsemanOfPestilence1;
        public static Texture GiantSeagull1;
        public static Texture ClericMilitia1;
        public static Texture ClericSwordmaster1;
        public static Texture ClericChampion1;
        public static Texture RisenAbomination1;
        public static Texture Parthiax1;
        public static Texture DruidOfTalAal1;
        public static Texture MutatedLeech1;
        public static Texture FuriousRuby1;
        public static Texture Tantrum1;
        public static Texture UnsettlingMox1;
        public static Texture VibrantEmerald1;
        public static Texture RadiantDiamond1;
        public static Texture VolatileAmethyst1;
        public static Texture HarmoniousSapphire1;
        public static Texture AmalgamatedSlime1;
        public static Texture SlimeToken1;
        public static Texture RobotAnt1;
        public static Texture ViciousTaskmaster1;
        public static Texture Mechamancer1;
        public static Texture ZapperVigilante1;
        public static Texture KeeperOfCuriosities1;
        public static Texture BeaconKeeper1;
        public static Texture CCC1;
        public static Texture Knock1;
        public static Texture FrenziedVine1;
        public static Texture WaterDown1;
        public static Texture TrollRider1;
        public static Texture HunterOfTheNight1;
        public static Texture JunkyardInnovator1;
        public static Texture Medusa1;
        public static Texture Cockatrice1;
        public static Texture GardenAnt1;
        public static Texture FleshEatingAnt1;
        public static Texture AntToken1;
        public static Texture FireAnt1;
        public static Texture AntQueen1;
        public static Texture SoldierAnt1;
        public static Texture BlueHero1;
        public static Texture BlackHero1;
        public static Texture WhiteHero1;
        public static Texture GreenHero1;
        public static Texture RedHero1;
        public static Texture PurpleHero1;
        public static Texture Hand1;


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
            SavingGrace1 = TextureLoader.CreateTexture(Properties.Resources.savinggrace1);
            DeepSeaMermaid1 = TextureLoader.CreateTexture(Properties.Resources.deepseamermaid1);
            Conflagrate1 = TextureLoader.CreateTexture(Properties.Resources.conflagrate1);
            Rapture1 = TextureLoader.CreateTexture(Properties.Resources.rapture1);
            Overcharge1 = TextureLoader.CreateTexture(Properties.Resources.overcharge1);
            ControlBoar1 = TextureLoader.CreateTexture(Properties.Resources.controlboar1);
            Seblastian1 = TextureLoader.CreateTexture(Properties.Resources.seblastian1);
            TokenRelic1 = TextureLoader.CreateTexture(Properties.Resources.tokenrelic1);
            HauntedChapel1 = TextureLoader.CreateTexture(Properties.Resources.hauntedchapel1);
            GhostToken1 = TextureLoader.CreateTexture(Properties.Resources.ghosttoken1);
            PortalJumper1 = TextureLoader.CreateTexture(Properties.Resources.portaljumper1);
            PalaceGuard1 = TextureLoader.CreateTexture(Properties.Resources.palaceguard1);
            Darkness1 = TextureLoader.CreateTexture(Properties.Resources.darkness1);
            MistFiend1 = TextureLoader.CreateTexture(Properties.Resources.mistfiend1);
            HorsemanOfFamine1 = TextureLoader.CreateTexture(Properties.Resources.horsemanoffamine1);
            HorsemanOfPestilence1 = TextureLoader.CreateTexture(Properties.Resources.horsemanofpestilence1);
            HorsemanOfWar1 = TextureLoader.CreateTexture(Properties.Resources.horsemanofwar1);
            GiantSeagull1 = TextureLoader.CreateTexture(Properties.Resources.giantseagull1);
            ClericMilitia1 = TextureLoader.CreateTexture(Properties.Resources.clericmilitia1);
            ClericSwordmaster1 = TextureLoader.CreateTexture(Properties.Resources.clericswordmaster1);
            ClericChampion1 = TextureLoader.CreateTexture(Properties.Resources.clericchampion1);
            RisenAbomination1 = TextureLoader.CreateTexture(Properties.Resources.risenabomination);
            Parthiax1 = TextureLoader.CreateTexture(Properties.Resources.parthiax1);
            DruidOfTalAal1 = TextureLoader.CreateTexture(Properties.Resources.druidoftalaal1);
            MutatedLeech1 = TextureLoader.CreateTexture(Properties.Resources.mutatedleech1);
            FuriousRuby1 = TextureLoader.CreateTexture(Properties.Resources.furiousruby1);
            Tantrum1 = TextureLoader.CreateTexture(Properties.Resources.tantrum1);
            UnsettlingMox1 = TextureLoader.CreateTexture(Properties.Resources.unsettlingmox1);
            VibrantEmerald1 = TextureLoader.CreateTexture(Properties.Resources.vibrantemerald1);
            RadiantDiamond1 = TextureLoader.CreateTexture(Properties.Resources.radiantdiamond1);
            VolatileAmethyst1 = TextureLoader.CreateTexture(Properties.Resources.volatileamethyst1);
            HarmoniousSapphire1 = TextureLoader.CreateTexture(Properties.Resources.harmonioussapphire1);
            AmalgamatedSlime1 = TextureLoader.CreateTexture(Properties.Resources.amalgamatedslime1);
            SlimeToken1 = TextureLoader.CreateTexture(Properties.Resources.slimetoken1);
            RobotAnt1 = TextureLoader.CreateTexture(Properties.Resources.robotant1);
            ViciousTaskmaster1 = TextureLoader.CreateTexture(Properties.Resources.vicioustaskmaster1);
            Mechamancer1 = TextureLoader.CreateTexture(Properties.Resources.mechamancer1);
            ZapperVigilante1 = TextureLoader.CreateTexture(Properties.Resources.zappervigilante1);
            KeeperOfCuriosities1 = TextureLoader.CreateTexture(Properties.Resources.keeperofcuriosities1);
            BeaconKeeper1 = TextureLoader.CreateTexture(Properties.Resources.beaconkeeper1);
            CCC1 = TextureLoader.CreateTexture(Properties.Resources.ccc1);
            Knock1 = TextureLoader.CreateTexture(Properties.Resources.telekenist1);
            FrenziedVine1 = TextureLoader.CreateTexture(Properties.Resources.frenziedvine1);
            WaterDown1 = TextureLoader.CreateTexture(Properties.Resources.waterdown1);
            TrollRider1 = TextureLoader.CreateTexture(Properties.Resources.trollrider1);
            HunterOfTheNight1 = TextureLoader.CreateTexture(Properties.Resources.hunterofthenight1);
            JunkyardInnovator1 = TextureLoader.CreateTexture(Properties.Resources.junkyardinnovator1);
            Medusa1 = TextureLoader.CreateTexture(Properties.Resources.medusa1);
            Cockatrice1 = TextureLoader.CreateTexture(Properties.Resources.cockatrice1);
            GardenAnt1 = TextureLoader.CreateTexture(Properties.Resources.gardenant1);
            FleshEatingAnt1 = TextureLoader.CreateTexture(Properties.Resources.flesheatingant1);
            AntQueen1 = TextureLoader.CreateTexture(Properties.Resources.antqueen1);
            AntToken1 = TextureLoader.CreateTexture(Properties.Resources.anttoken1);
            SoldierAnt1 = TextureLoader.CreateTexture(Properties.Resources.soldierant1);
            FireAnt1 = TextureLoader.CreateTexture(Properties.Resources.fireant1);
            WhiteHero1 = TextureLoader.CreateTexture(Properties.Resources.whitehero1);
            BlackHero1 = TextureLoader.CreateTexture(Properties.Resources.blackhero1);
            GreenHero1 = TextureLoader.CreateTexture(Properties.Resources.greenhero1);
            BlueHero1 = TextureLoader.CreateTexture(Properties.Resources.bluehero1);
            PurpleHero1 = TextureLoader.CreateTexture(Properties.Resources.purplehero1);
            RedHero1 = TextureLoader.CreateTexture(Properties.Resources.redhero1);
            Hand1 = TextureLoader.CreateTexture(Properties.Resources.hand1);

            // = TextureLoader.CreateTexture(Properties.Resources.);
        }

        public static Texture Portraits(CardTemplates template)
        {
            switch (template)
            {
                case CardTemplates.AngryGoblin: { return Goblin1; }
                case CardTemplates.ArmoredZombie: { return Zombie1; }
                case CardTemplates.Zap: { return Zap1; }
                case CardTemplates.DepravedBloodhound: { return Hound1; }
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
                case CardTemplates.MechanicalToken1: { return InsectToken1; }
                case CardTemplates.Eliminate: { return Eliminate1; }
                case CardTemplates.SilvervenomSpider: { return SilvervenomSpider1; }
                case CardTemplates.Inspiration: { return Inspiration1; }
                case CardTemplates.CourtInformant: { return CourtInformant1; }
                case CardTemplates.BattlehardenedMage: { return BattlehardenedMage1; }
                case CardTemplates.ArcticWatchman: { return ArcticWatchman1; }
                case CardTemplates.HawkToken1: { return HawkToken1; }
                case CardTemplates.SavingGrace: { return SavingGrace1; }
                case CardTemplates.DeepSeaMermaid: { return DeepSeaMermaid1; }
                case CardTemplates.Conflagrate: { return Conflagrate1; }
                case CardTemplates.Rapture: { return Rapture1; }
                case CardTemplates.Overcharge: { return Overcharge1; }
                case CardTemplates.ControlBoar: { return ControlBoar1; }
                case CardTemplates.Seblastian: { return Seblastian1; }
                case CardTemplates.HauntedChapel: { return HauntedChapel1; }
                case CardTemplates.GhostToken1: { return GhostToken1; }
                case CardTemplates.PortalJumper: { return PortalJumper1; }
                case CardTemplates.PalaceGuard: { return PalaceGuard1; }
                case CardTemplates.Darkness: { return Darkness1; }
                case CardTemplates.MistFiend: { return MistFiend1; }
                case CardTemplates.HorsemanOfFamine: { return HorsemanOfFamine1; }
                case CardTemplates.HorsemanOfPestilence: { return HorsemanOfPestilence1; }
                case CardTemplates.HorsemanOfWar: { return HorsemanOfWar1; }
                case CardTemplates.GiantSeagull: { return GiantSeagull1; }
                case CardTemplates.ClericMilitia: { return ClericMilitia1; }
                case CardTemplates.ClericSwordmaster: { return ClericSwordmaster1; }
                case CardTemplates.ClericChampion: { return ClericChampion1; }
                case CardTemplates.RisenAbomination: { return RisenAbomination1; }
                case CardTemplates.Parthiax: { return Parthiax1; }
                case CardTemplates.DruidOfTalAal: { return DruidOfTalAal1; }
                case CardTemplates.MutatedLeech: { return MutatedLeech1; }
                case CardTemplates.Tantrum: { return Tantrum1; }
                case CardTemplates.FuriousRuby: { return FuriousRuby1; }
                case CardTemplates.HarmoniousSapphire: { return HarmoniousSapphire1; }
                case CardTemplates.VibrantEmerald: { return VibrantEmerald1; }
                case CardTemplates.UnsettlingOnyx: { return UnsettlingMox1; }
                case CardTemplates.RadiantDiamond: { return RadiantDiamond1; }
                case CardTemplates.VolatileAmethyst: { return VolatileAmethyst1; }
                case CardTemplates.AmalgamatedSlime: { return AmalgamatedSlime1; }
                case CardTemplates.SlimeToken1: { return SlimeToken1; }
                case CardTemplates.RobotAnt: { return RobotAnt1; }
                case CardTemplates.ViciousTaskmaster: { return ViciousTaskmaster1; }
                case CardTemplates.Mechamancer: { return Mechamancer1; }
                case CardTemplates.ZapperVigilante: { return ZapperVigilante1; }
                case CardTemplates.KeeperOfCuriosities: { return KeeperOfCuriosities1; }
                case CardTemplates.BeaconKeeper: { return BeaconKeeper1; }
                case CardTemplates.Knock: { return Knock1; }
                case CardTemplates.FrenziedVine: { return FrenziedVine1; }
                case CardTemplates.WaterDown: { return WaterDown1; }
                case CardTemplates.TrollRider: { return TrollRider1; }
                case CardTemplates.HunterOfTheNight: { return HunterOfTheNight1; }
                case CardTemplates.JunkyardInnovator: { return JunkyardInnovator1; }
                case CardTemplates.Medusa: { return Medusa1; }
                case CardTemplates.Cockatrice: { return Cockatrice1; }
                case CardTemplates.GardenAnt: { return GardenAnt1; }
                case CardTemplates.FleshEatingAnt: { return FleshEatingAnt1; }
                case CardTemplates.AntQueen: { return AntQueen1; }
                case CardTemplates.SoldierAnt: { return SoldierAnt1; }
                case CardTemplates.FireAnt: { return FireAnt1; }
                case CardTemplates.AntToken1: { return AntToken1; }
                case CardTemplates.BlueHero: { return BlueHero1; }
                case CardTemplates.GreenHero: { return GreenHero1; }
                case CardTemplates.BlackHero: { return BlackHero1; }
                case CardTemplates.WhiteHero: { return WhiteHero1; }
                case CardTemplates.PurpleHero: { return PurpleHero1; }
                case CardTemplates.RedHero: { return RedHero1; }
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
