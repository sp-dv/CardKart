using CardKartShared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CardKartShared.GameState
{
    public class Token : GameObject
    {
        public Card TokenOf;

        public Player Controller;

        private bool ExhaustedInternal;
        public bool IsExhausted
        {
            get { return 
                    ExhaustedInternal || 
                    (!IsRelic && SummoningSick && !KeywordAbilities[KeywordAbilityNames.Bloodlust]); }
            set { ExhaustedInternal = value; }
        }
        public bool SummoningSick;
        public bool Stunned;

        public int DamageTaken;
        public int Attack => 
            TokenOf.Attack + AuraModifiers.Attack;
        public int MaxHealth => TokenOf.Health + AuraModifiers.Health;
        public int CurrentHealth => 
             MaxHealth - DamageTaken;

        public CreatureTypes CreatureType => TokenOf.CreatureType;
        public bool IsHero => TokenOf.IsHero;
        public bool IsCreature => TokenOf.Type == CardTypes.Creature;
        public bool IsRelic => TokenOf.Type == CardTypes.Relic;
        public bool CanAttack => !IsExhausted;
        public bool CanBlock => !IsExhausted;
        public bool IsValid => TokenOf != null;
        public bool IsDead => IsCreature && CurrentHealth <= 0;

        public TriggeredAbility[] TriggeredAbilities;
        public KeywordAbilityContainer KeywordAbilities;
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
            if (other.HasKeywordAbility(KeywordAbilityNames.Terrify) && this.TokenOf.CastingCost.Size <= other.TokenOf.CastingCost.Size) { return false; }
            if (other.HasKeywordAbility(KeywordAbilityNames.Flying) && 
                !(this.HasKeywordAbility(KeywordAbilityNames.Range) || this.HasKeywordAbility(KeywordAbilityNames.Flying))) { return false; }
            return true;
        }

        public bool HasKeywordAbility(KeywordAbilityNames keyword)
        {
            return KeywordAbilities[keyword] || AuraModifiers.Keywords[keyword];
        }

        public string GenerateBreadText()
        {
            if (!IsValid) { return ""; }

            var breadTextBuilder = new StringBuilder();

            var allKeywordAbilities = new HashSet<KeywordAbilityNames>();

            foreach (var keywordAbility in KeywordAbilities.GetAbilities().Concat(AuraModifiers.Keywords.GetAbilities()))
            {
                allKeywordAbilities.Add(keywordAbility);
            }

            foreach (var keywordAbility in allKeywordAbilities)
            { 
                breadTextBuilder.Append(keywordAbility.ToString());
                breadTextBuilder.Append(" (");
                breadTextBuilder.Append(Constants.KeywordExplanation(keywordAbility));
                breadTextBuilder.AppendLine(")");
            }

            foreach (var ability in TokenOf.Abilities)
            {
                if (ability.BreadText != null)
                {
                    breadTextBuilder.AppendLine(ability.BreadText);
                }
            }
            foreach (var aura in Auras)
            {
                if (aura.BreadText != null)
                {
                    breadTextBuilder.AppendLine(aura.BreadText);
                }
            }

            if (Stunned)
            {
                breadTextBuilder.AppendLine("\nStunned (Doesn't unexhaust at the start of its controllers turn)");
            }

            return breadTextBuilder.ToString();
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
