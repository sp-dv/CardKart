using System;
using System.Collections.Generic;
using System.Linq;

namespace CardKartShared.GameState
{
    public class Card : GameObject
    {
        public string Name;

        public bool IsHero => Type == CardTypes.Hero;

        public int Attack;
        public int Health;

        public CardTypes Type;
        public CardTemplates Template;
        public ManaColour Colour;

        public Player Owner;
        public Player Controller => Owner;

        public Pile Pile;
        public PileLocation Location => Pile.Location;

        public Token Token;

        public Ability[] Abilities;
        public TriggeredAbility[] TriggeredAbilities;
        public KeywordAbilityContainer KeywordAbilities = new KeywordAbilityContainer();
        public Aura[] Auras;

        public ManaSet CastingCost;

        public string BreadText { get; }

        public Card(CardTemplates template)
        {
            Template = template;

            // Switch of death.
            switch (Template)
            {
                case CardTemplates.AngryGoblin:
                    {
                        Name = "Angry Goblin";
                        Type = CardTypes.Creature;
                        Colour = ManaColour.Red;
                        CastingCost = new ManaSet(
                            ManaColour.Red);

                        Attack = 2;
                        Health = 1;

                        Abilities = new Ability[] {
                            new GenericCreatureCast(),
                        };
                        KeywordAbilities[KeywordAbilityNames.Bloodlust] = true;
                    } break;

                case CardTemplates.ArmoredZombie:
                    {
                        Name = "Armored Zombie";
                        Type = CardTypes.Creature;
                        Colour= ManaColour.Black;
                        CastingCost = new ManaSet(
                            ManaColour.Black, 
                            ManaColour.Black);

                        Attack = 1;
                        Health = 4;

                        Abilities = new Ability[] {
                            new GenericCreatureCast(),
                        };
                    } break;

                case CardTemplates.Zap:
                    {
                        Name = "Zap";
                        Type = CardTypes.Instant;
                        Colour = ManaColour.Red;
                        CastingCost = new ManaSet(
                            ManaColour.Red
                            );
                        BreadText = "Deal 2 damage to\ntarget creature.";

                        Abilities = new Ability[] {
                            new ZapCast(),
                        };
                    } break;

                case CardTemplates.HeroTest:
                    {
                        Name = "Heroxd";
                        Type = CardTypes.Hero;
                        Colour = ManaColour.White;
                        CastingCost = new ManaSet();

                        Health = 27;
                    } break;

                case CardTemplates.DepravedBloodhound:
                    {
                        Name = "Depraved Bloodhound";
                        Type = CardTypes.Creature;
                        Colour = ManaColour.Black;
                        CastingCost = new ManaSet(ManaColour.Black, ManaColour.Black);
                        //BreadText = "Whenever a player draws\na card deal 1 damage to\nthat player.";

                        Attack = 2;
                        Health = 3;

                        Abilities = new Ability[] {
                            new GenericCreatureCast(),
                            new DepravedBloodhoundTrigger()
                        };
                    } break;

                case CardTemplates.StandardBearer:
                    {
                        Name = "Standard Bearer";
                        Type = CardTypes.Creature;
                        Colour = ManaColour.White;
                        CastingCost = new ManaSet(ManaColour.White);

                        Attack = 1;
                        Health = 1;

                        Abilities = new Ability[] {
                            new GenericCreatureCast(),
                        };
                        Auras = new Aura[] {
                            new StandardBearerAura(),
                        };
                    } break;

                case CardTemplates.Enlarge:
                    {
                        Name = "Enlarge";
                        Type = CardTypes.Instant;
                        Colour = ManaColour.Green;
                        CastingCost = new ManaSet(ManaColour.Green);

                        Abilities = new Ability[] {
                            new EnlargeCast(), 
                        };
                    } break;

                case CardTemplates.Test:
                    {
                        Name = "Test";
                        Type = CardTypes.Channel;
                        Colour = ManaColour.Black;
                        CastingCost = new ManaSet(ManaColour.Black);

                        Abilities = new Ability[] {
                            new TestCast(),
                        };
                    } break;

                default:
                    {
                        throw new Exception("Bad card template...");
                    }
            }

            if (Abilities == null)
            {
                Abilities = new Ability[0];
            }
            foreach (var ability in Abilities)
            {
                ability.Card = this;
            }

            TriggeredAbilities = Abilities
                .Where(ability => ability is TriggeredAbility)
                .Select(triggeredAbility => triggeredAbility as TriggeredAbility).ToArray();
            
            if (Auras == null)
            {
                Auras = new Aura[0];
            }
        }

        public Ability[] GetUsableAbilities(AbilityCastingContext context)
        {
            return Abilities
                .Where(ability => ability.IsCastable(context)).ToArray();
        }

        public int IndexOfAbility(Ability ability)
        {
            for (int i = 0; i < Abilities.Length; i++)
            {
                if (ability == Abilities[i]) { return i; }
            }

            throw new NotImplementedException();
        }
    }

    public enum CardTypes
    {
        None,

        Creature,
        Instant,
        Channel,
        Relic,
        Hero,
    }

    public enum CardTemplates
    {
        AngryGoblin,
        ArmoredZombie,
        Zap,
        DepravedBloodhound,
        StandardBearer,
        Enlarge,

        Test,

        HeroTest,
    }
}
