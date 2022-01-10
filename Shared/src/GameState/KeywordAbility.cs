using System;
using System.Collections.Generic;
using System.Linq;

namespace CardKartShared.GameState
{
    public class KeywordAbilityContainer
    {
        private bool[] HasAbility;

        public bool this[KeywordAbilityNames keyword]
        {
            get { return HasAbility[(int)keyword]; }
            set { HasAbility[(int)keyword] = value; }
        }

        public KeywordAbilityContainer(KeywordAbilityContainer toClone)
        {
            HasAbility = toClone.HasAbility.ToArray();
        }

        public KeywordAbilityContainer()
        {
            HasAbility = new bool[Enum.GetValues(typeof(KeywordAbilityNames)).Length];
        }

        public List<KeywordAbilityNames> GetAbilities()
        {
            var rt = new List<KeywordAbilityNames>();
            for (int i = 0; i < HasAbility.Length; i++)
            {
                if (HasAbility[i])
                {
                    rt.Add((KeywordAbilityNames)i);
                }
            }
            return rt;
        }

        public void ZeroOut()
        {
            for (int i = 0; i < HasAbility.Length; i++)
            {
                HasAbility[i] = false;
            }
        }
    }
    
    public enum KeywordAbilityNames
    {
        None,

        Bloodlust,
        Vigilance, 
        Flying,
        Range,
        Reinforcement,
        Protected,
        Terrify,
    }
}
