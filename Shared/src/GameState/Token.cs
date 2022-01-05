﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CardKartShared.GameState
{
    public class Token : GameObject
    {
        public Card TokenOf { get; }

        public Player Controller;

        private bool ExhaustedInternal;
        public bool Exhausted
        {
            get { return 
                    ExhaustedInternal || 
                    (SummoningSick && !KeywordAbilities[KeywordAbilityNames.Bloodlust]); }
            set { ExhaustedInternal = value; }
        }
        public bool SummoningSick;

        public int DamageTaken;
        public int Attack => 
            TokenOf.Attack + AuraModifiers.Attack;
        public int MaxHealth => TokenOf.Health + AuraModifiers.Health;
        public int CurrentHealth => 
             MaxHealth - DamageTaken;

        public bool IsHero => TokenOf.IsHero;
        public bool IsCreature => TokenOf.Type == CardTypes.Creature;
        public bool CanAttack => !Exhausted;
        public bool CanBlock => !Exhausted;

        public TriggeredAbility[] TriggeredAbilities;
        private KeywordAbilityContainer KeywordAbilities;
        public List<Aura> Auras;

        public AuraModifier AuraModifiers { get; } = new AuraModifier();

        public Token(Card card)
        {
            TokenOf = card;

            TriggeredAbilities = TokenOf.TriggeredAbilities.ToArray();
            KeywordAbilities = new KeywordAbilityContainer(TokenOf.KeywordAbilities);
            Auras = TokenOf.Auras.ToList();

            Controller = card.Owner;
        }

        public bool CanBlockToken(Token other) 
        {
            return true;
        }

        public bool HasKeywordAbility(KeywordAbilityNames keyword)
        {
            return KeywordAbilities[keyword] || AuraModifiers.Keywords[keyword];
        }
    }

    public class AuraModifier
    {
        public int Attack;
        public int Health;
        public KeywordAbilityContainer Keywords { get; } = new KeywordAbilityContainer();

        public void Reset()
        {
            Attack = 0;
            Health = 0;
            Keywords.ZeroOut();
        }
    }
}
