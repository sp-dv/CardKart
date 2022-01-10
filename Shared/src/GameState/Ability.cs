using CardKartShared.Network.Messages;
using System;
using System.Linq;
using System.Text;

namespace CardKartShared.GameState
{
    public class Ability
    {
        public Card Card;

        public bool MoveToStackOnCast { get; set; }
        public string BreadText { get; set; }

        public Func<AbilityCastingContext, bool> IsCastable { get; set; } = context =>
        {
            // Demand explicit conditions for casting.
            return false;
        };

        public Func<AbilityCastingContext, bool> MakeCastChoices { get; set; } = context =>
        {
            // If we don't do anything in cast choices; it makes no sense to implement it just to return true.
            return true;
        };

        public Action<AbilityCastingContext> EnactCastChoices { get; set; } = context =>
        {
        };

        public Action<AbilityCastingContext> MakeResolveChoicesCastingPlayer { get; set; } = context =>
        {
        };
        public Action<AbilityCastingContext> MakeResolveChoicesNonCastingPlayer { get; set; } = context =>
        {
        };
        public Action<AbilityCastingContext> Resolve { get; set; } = context =>
        {
        };


    }
    public class AbilityCastingContext
    {
        private const string CastingPlayerKey = "_CastingPlayer";
        private const string CardKey = "_Card";
        private const string AbilityKey = "_Ability";

        public GameChoice Choices;
        public ChoiceHelper ChoiceHelper;
        public GameState GameState;
        public Player Hero;

        public bool IsValid => HasCastingPlayer && HasCard && HasAbility;

#region Synced

        // Synched by putting it in Choices.

        public Player CastingPlayer
        {
            get => !HasCastingPlayer ? null : 
                GameState.GetByID(Choices.Singletons[CastingPlayerKey]) as Player;
            set => Choices.Singletons[CastingPlayerKey] = value.ID;
        }
        private bool HasCastingPlayer => 
            Choices.Singletons.ContainsKey(CastingPlayerKey);

        public Card Card
        {
            get => !HasCard ? null : GameState.GetByID(Choices.Singletons[CardKey]) as Card;
            set => Choices.Singletons[CardKey] = value.ID;
        }
        private bool HasCard =>
            Choices.Singletons.ContainsKey(CardKey);

        public Ability Ability
        {
            get => !HasAbility ? null : Card.Abilities[Choices.Singletons[AbilityKey]];
            set => Choices.Singletons[AbilityKey] = value.Card.IndexOfAbility(value);
        }
        private bool HasAbility =>
            HasCard && Choices.Singletons.ContainsKey(AbilityKey);

#endregion

        
        public void SetCard(string key, Card card)
        {
            Choices.Singletons[key] = card.ID;
        }

        public Card GetCard(string key)
        {
            return GameState.GetByID(Choices.Singletons[key]) as Card;
        }

        public void SetToken(string key, Token token)
        {
            Choices.Singletons[key] = token.ID;
        }

        public Token GetToken(string key)
        {
            return GameState.GetByID(Choices.Singletons[key]) as Token;
        }

        public void SetPlayer(string key, Player player)
        {
            Choices.Singletons[key] = player.ID;
        }

        public Player GetPlayer(string key)
        {
            return GameState.GetByID(Choices.Singletons[key]) as Player;
        }

        public void SetManaSet(string key, ManaSet set)
        {
            Choices.Arrays[key] = set.ToInts();
        }

        public bool HasSingleton(string key)
        {
            return Choices.Singletons.ContainsKey(key);
        }

        public ManaSet GetManaSet(string key)
        {
            var set = new ManaSet();
            set.FromInts(Choices.Arrays[key]);
            return set;
        }
    }
}
