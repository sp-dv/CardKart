using System;
using System.Collections.Generic;
using System.Linq;

namespace CardKartShared.GameState
{
    public class Card : GameObject
    {
        public string Name;
        
        public int Attack;
        public int Defence;

        public CardTypes Type;
        public CardTemplates Template;
        public ManaColour Colour;

        public Player Owner;
        public Player Controller;

        public Pile Pile;
        public PileLocation Location => Pile.Location;

        public Token Token;

        public ActiveAbility[] ActiveAbilities;

        public Card(CardTemplates template)
        {
            Template = template;

            // Switch of death.
            switch (Template)
            {
                case CardTemplates.AngryGoblin:
                    {
                        Name = "Angry Goblin";
                        Type = CardTypes.Monster;
                        Colour = ManaColour.Red;

                        Attack = 2;
                        Defence = 1;

                        ActiveAbilities = new ActiveAbility[] {
                            new GenericCreatureCast()
                        };
                    } break;

                case CardTemplates.ArmoredZombie:
                    {
                        Name = "Armored Zombie";
                        Type = CardTypes.Monster;
                        Colour= ManaColour.Black;

                        Attack = 1;
                        Defence = 4;

                        ActiveAbilities = new ActiveAbility[] {
                            new GenericCreatureCast(),
                        };
                    } break;

                case CardTemplates.Zap:
                    {
                        Name = "Zap";
                        Type = CardTypes.Instant;
                        Colour = ManaColour.Red;
                    } break;

                default:
                    {
                        throw new Exception("Bad card template...");
                    }
            }

            if (ActiveAbilities == null)
            {
                ActiveAbilities = new ActiveAbility[0];
            }
            foreach (var ability in ActiveAbilities)
            {
                ability.Card = this;
            }
        }

        public ActiveAbility[] GetUsableAbilities(
            AbilityCastingContext context)
        {
            return ActiveAbilities
                .Where(ability => ability.Castable(context)).ToArray();
        }

        public int IndexOfActiveAbility(ActiveAbility ability)
        {
            for (int i = 0; i < ActiveAbilities.Length; i++)
            {
                if (ability == ActiveAbilities[i]) { return i; }
            }

            throw new NotImplementedException();
        }
    }

    public enum CardTypes
    {
        None,

        Monster,
        Instant,
        Channel,
        Relic,
    }

    public enum CardTemplates
    {
        AngryGoblin,
        ArmoredZombie,
        Zap,
    }
}
