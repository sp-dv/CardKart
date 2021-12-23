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
        public int Defence;

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

        public ManaSet CastingCost;

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
                        Defence = 1;

                        Abilities = new Ability[] {
                            new GenericCreatureCast(),
                        };
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
                        Defence = 4;

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

                        Defence = 27;
                    } break;

                case CardTemplates.Testcard:
                    {
                        Name = "Testcard";
                        Type = CardTypes.Creature;
                        Colour = ManaColour.Blue;
                        CastingCost = new ManaSet(ManaColour.Blue);

                        Attack = 1;
                        Defence = 1;

                        Abilities = new Ability[] {
                            new GenericCreatureCast(),
                            new TestTriggeredAbility()
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

        Testcard,

        HeroTest,
    }
}
