using System;

namespace CardKartShared.GameState
{
    public class Card : GameObject
    {
        public string Name;
        
        public int Attack;
        public int Defence;

        public CardTypes Type;
        public CardTemplates Template;
        public GameColour Colour;

        public Pile Pile;
        public PileLocation Location => Pile.Location;

        public Token Token;

        public Card(CardTemplates template)
        {
            Template = template;

            switch (Template)
            {
                case CardTemplates.AngryGoblin:
                    {
                        Name = "Angry Goblin";
                        Type = CardTypes.Monster;
                        Colour = GameColour.Red;

                        Attack = 2;
                        Defence = 1;
                    } break;

                case CardTemplates.ArmoredZombie:
                    {
                        Name = "Armored Zombie";
                        Type = CardTypes.Monster;
                        Colour= GameColour.Black;

                        Attack = 1;
                        Defence = 4;
                    } break;

                case CardTemplates.Zap:
                    {
                        Name = "Zap";
                        Type = CardTypes.Instant;
                        Colour = GameColour.Red;
                    } break;

                default:
                    {
                        throw new Exception("Bad card template...");
                    }
            }
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
