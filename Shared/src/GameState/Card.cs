using System;

namespace CardKartShared.GameState
{
    public class Card : GameObject
    {
        public string Name;
        
        public int Attack;
        public int Defence;

        public CardTypes Type;

        public Pile Pile;
        public PileLocation Location => Pile.Location;

        public Card(CardTemplates template)
        {
            switch (template)
            {
                case CardTemplates.AngryGoblin:
                    {
                        Name = "Angry Goblin";
                        Type = CardTypes.Monster;

                        Attack = 2;
                        Defence = 1;
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
    }
}
