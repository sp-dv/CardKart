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
            return false;
        };

        public Func<AbilityCastingContext, bool> MakeCastChoices { get; set; } = context =>
        {
            return false;
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
        public Action<AbilityCastingContext> EnactResolveChoices { get; set; } = context =>
        {
        };


    }
    public class AbilityCastingContext
    {
        public GameChoice Choices;
        public ChoiceHelper ChoiceHelper;
        public GameState GameState;

#region Synced

        // Synched by putting it in Choices.

        public Player CastingPlayer
        {
            get => !HasCastingPlayer ? null : 
                GameState.GetByID(Choices.Singletons["_CastingPlayer"]) as Player;
            set => Choices.Singletons["_CastingPlayer"] = value.ID;
        }
        private bool HasCastingPlayer => 
            Choices.Singletons.ContainsKey("_CastingPlayer");

        public Card Card
        {
            get => !HasCard ? null : GameState.GetByID(Choices.Singletons["_Card"]) as Card;
            set => Choices.Singletons["_Card"] = value.ID;
        }
        private bool HasCard =>
            Choices.Singletons.ContainsKey("_Card");

        public Ability Ability
        {
            get => !HasAbility ? null : Card.Abilities[Choices.Singletons["_Ability"]];
            set => Choices.Singletons["_Ability"] = value.Card.IndexOfAbility(value);
        }
        private bool HasAbility =>
            HasCard && Choices.Singletons.ContainsKey("_Ability");

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

        public ManaSet GetManaSet(string key)
        {
            var set = new ManaSet();
            set.FromInts(Choices.Arrays[key]);
            return set;
        }
    }
}
