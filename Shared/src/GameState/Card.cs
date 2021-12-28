using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

                        Abilities = new[] {
                            GenericCreatureCast(),
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
                            GenericCreatureCast(),
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

                        Abilities = new[] {
                            new Ability {
                                MoveToStackOnCast = true,
                                BreadText = "Deal 2 damage to target creature.",

                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost, context),
                                MakeCastChoices = context => {
                                    var payment = MakePayManaChoice(CastingCost, context, context.CastingPlayer);
                                    if (payment == null) { return false; }

                                    context.ChoiceHelper.Text = "Choose a target for Zap.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(token => true);
                                    if (target == null) { return false; }

                                    context.SetManaSet("manacost", payment);
                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => GenericPayManaEnact(context),
                                EnactResolveChoices = context =>
                                {
                                    var target = context.GetToken("!target");
                                    context.GameState.DealDamage(this, target, 2);
                                }
                            }
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

                        Attack = 2;
                        Health = 3;

                        Abilities = new Ability[] {
                            GenericCreatureCast(),
                            new TriggeredAbility {
                                BreadText = "Whenever a player draws a card; deal 1 damage to that player.",

                                IsTriggeredBy = trigger =>
                                {
                                    if (trigger is DrawTrigger)
                                    {
                                        var drawTrigger = trigger as DrawTrigger;

                                        return true;
                                    }

                                    return false;
                                },
                                SaveTriggerInfo = (trigger, context) => {
                                    var drawTrigger = trigger as DrawTrigger;
                                    context.SetPlayer("player", drawTrigger.Player);
                                },
                                EnactResolveChoices = context => {
                                    var player = context.GetPlayer("player");
                                    var card = context.Card;
                                    context.GameState.DealDamage(card, player.HeroCard.Token, 1);
                                },
                            },
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
                            GenericCreatureCast(),
                        };
                        Auras = new Aura[] {
                            new Aura {
                                BreadText = "Other creatures you control get +1/+1.",
                                ApplyAura = (token, gameState) =>
                                {
                                    foreach (var otherToken in gameState.AllTokens)
                                    {
                                        if (otherToken != token && otherToken.Controller == token.Controller)
                                        {
                                            otherToken.AuraModifiers.Attack += 1;
                                            otherToken.AuraModifiers.Health += 1;
                                        }
                                    }
                                },
                            }
                        };
                    } break;

                case CardTemplates.Enlarge:
                    {
                        Name = "Enlarge";
                        Type = CardTypes.Instant;
                        Colour = ManaColour.Green;
                        CastingCost = new ManaSet(ManaColour.Green);

                        Abilities = new Ability[] {
                            new Ability {
                                MoveToStackOnCast = true,
                                BreadText = "Target creature gets +2/+2 until end of turn.",

                                IsCastable =  context =>
                                {
                                    return InHandAndOwned(context) &&
                                        ManaCostIsPayable(CastingCost, context);
                                },
                                MakeCastChoices = context =>
                                {
                                    var payment = MakePayManaChoice(CastingCost, context, context.CastingPlayer);
                                    if (payment == null) { return false; }

                                    context.ChoiceHelper.Text = "Choose a target for Enlarge.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(token => true);
                                    if (target == null) { return false; }

                                    context.SetManaSet("manacost", payment);
                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context =>
                                {
                                    context.GameState.SpendMana(
                                        context.CastingPlayer,
                                        context.GetManaSet("manacost"));
                                },
                                EnactResolveChoices = context =>
                                {
                                    var target = context.GetToken("!target");
                                    target.Auras.Add(new Aura
                                    {
                                        ApplyAura = (token, gameState) =>
                                        {
                                            token.AuraModifiers.Attack += 2;
                                            token.AuraModifiers.Health += 2;
                                        },

                                        IsCancelledBy = (trigger, token, gameState) =>
                                        {
                                            if (trigger is GameTimeTrigger)
                                            {
                                                var gameTime = trigger as GameTimeTrigger;

                                                return gameTime.Time == GameTime.EndOfTurn;
                                            }
                                            return false;
                                        }
                                    });
                                }
                            }
                        };
                    } break;

                case CardTemplates.AlterFate:
                    {
                        Name = "Alter Fate";
                        Type = CardTypes.Channel;
                        Colour = ManaColour.Blue;
                        CastingCost = new ManaSet(ManaColour.Blue);
                        

                        Abilities = new Ability[] {
                            new Ability
                            {
                                MoveToStackOnCast = true,
                                BreadText = "Look at the top three cards of your deck. Choose a card and put it into your hand. Shuffle the others into your deck.",
                                IsCastable = context =>
                                {
                                    return
                                        InHandAndOwned(context) &&
                                        ChannelsAreCastable(context) &&
                                        ManaCostIsPayable(CastingCost, context);

                                },
                                MakeCastChoices = context =>
                                {
                                    var payment = MakePayManaChoice(CastingCost, context, context.CastingPlayer);
                                    if (payment == null) { return false; }

                                    context.SetManaSet("manacost", payment);
                                    return true;
                                },
                                EnactCastChoices = context =>
                                {
                                    context.GameState.SpendMana(
                                        context.CastingPlayer,
                                        context.GetManaSet("manacost"));
                                },
                                MakeResolveChoicesCastingPlayer = context =>
                                {
                                    var choices = context.CastingPlayer.Deck.Peek(3);
                                    context.ChoiceHelper.CardChoices = choices;
                                    context.ChoiceHelper.Text = "Choose a card to draw.";
                                    var choice =
                                        context.ChoiceHelper.ChooseCard(card => choices.Contains(card));
                                    context.SetCard("target", choice);
                                },
                                EnactResolveChoices = context =>
                                {
                                    var card = context.GetCard("target");
                                    context.GameState.MoveCard(card, card.Owner.Hand);
                                }
                            }
                        };
                    } break;

                case CardTemplates.MindFlay:
                    {
                        Name = "Mind Flay";
                        Type = CardTypes.Channel;
                        Colour = ManaColour.Black;
                        CastingCost = new ManaSet(ManaColour.Black);

                        Abilities = new Ability[] {
                            new Ability
                            {
                                BreadText = "Your opponent chooses a card from their hand and discards it.",
                                MoveToStackOnCast = true,

                                IsCastable = context =>
                                {
                                    return InHandAndOwned(context) &&
                                        ChannelsAreCastable(context) &&
                                        ManaCostIsPayable(CastingCost, context);
                                },

                                MakeCastChoices = context =>
                                {
                                    return GenericPayManaChoice(CastingCost, context);
                                },

                                EnactCastChoices = context =>
                                {
                                    GenericPayManaEnact(context);
                                },

                                MakeResolveChoicesNonCastingPlayer = context =>
                                {
                                    var choices = context.CastingPlayer.Opponent.Hand;
                                    context.ChoiceHelper.CardChoices = choices;
                                    context.ChoiceHelper.Text = "Choose a card to discard.";
                                    var choice =
                                        context.ChoiceHelper.ChooseCard(card => choices.Contains(card));
                                    context.SetCard("target", choice);
                                },

                                EnactResolveChoices = context =>
                                {
                                    var card = context.GetCard("target");
                                    context.GameState.MoveCard(card, card.Owner.Graveyard);
                                }
                            }
                        };
                    } break;

                case CardTemplates.GolbinBombsmith:
                    {
                        Name = "Goblin Bombsmith";
                        Type = CardTypes.Creature;
                        Colour = ManaColour.Red;
                        CastingCost = new ManaSet(ManaColour.Red);

                        Attack = 1;
                        Health = 2;

                        Abilities = new[]{
                            GenericCreatureCast(),
                            new Ability {
                                BreadText = "Exhaust: Deal 1 damage to target creature or player.",
                                IsCastable = context => Location == PileLocation.Battlefield && !Token.Exhausted && context.CastingPlayer == Token.Controller,
                                MakeCastChoices = context => {
                                    context.ChoiceHelper.Text = "Choose a target for Throw Bomb.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(token => true);
                                    if (target == null) { return false; }

                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    Token.Exhausted = true;
                                },
                                EnactResolveChoices = context =>
                                {
                                    var target = context.GetToken("!target");
                                    context.GameState.DealDamage(this, target, 1);
                                }
                            }
                        };
                    } break;

                case CardTemplates.CrystalizedGeyser:
                    {
                        Name = "Crystalized Geyser";
                        Type = CardTypes.Creature;
                        Colour = ManaColour.Mixed;
                        CastingCost = new ManaSet(ManaColour.Red, ManaColour.Blue);

                        Attack = 1;
                        Health = 1;

                        var abilityAManacost = new ManaSet(ManaColour.Blue);
                        var abilityBManacost = new ManaSet(ManaColour.Red);

                        Abilities = new[] {
                            GenericCreatureCast(),
                            new Ability {
                                BreadText = "Exhaust, Pay 1 blue mana: Draw a card.\n",
                                IsCastable = context => 
                                    Location == PileLocation.Battlefield && 
                                    !Token.Exhausted && 
                                    context.CastingPlayer == Token.Controller &&
                                    ManaCostIsPayable(abilityAManacost, context),
                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(abilityAManacost, context);
                                },
                                EnactCastChoices = context => {
                                    GenericPayManaEnact(context);
                                    Token.Exhausted = true;
                                    context.SetPlayer("drawer", context.CastingPlayer);
                                },
                                EnactResolveChoices = context =>
                                {
                                    var drawer = context.GetPlayer("drawer");
                                    context.GameState.DrawCards(drawer, 1);
                                }
                            },
                            new Ability {
                                BreadText = "Exhaust, Pay 1 red mana: Deal 2 damage to target creature or player.",
                                IsCastable = context => 
                                    Location == PileLocation.Battlefield && 
                                    !Token.Exhausted && 
                                    context.CastingPlayer == Token.Controller
                                    && ManaCostIsPayable(abilityBManacost, context),
                                MakeCastChoices = context => {
                                    var payment = MakePayManaChoice(abilityBManacost, context, context.CastingPlayer);
                                    if (payment == null) { return false; }

                                    context.ChoiceHelper.Text = "Choose a target for Breathe Fire.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(token => true);
                                    if (target == null) { return false; }

                                    context.SetManaSet("manacost", payment);
                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    MakePayManaChoice(context.GetManaSet("manacost"), context, context.CastingPlayer);
                                    Token.Exhausted = true;
                                },
                                EnactResolveChoices = context =>
                                {
                                    var target = context.GetToken("!target");
                                    context.GameState.DealDamage(this, target, 2);
                                }
                            }
                        };
                    } break;

                case CardTemplates.RegeneratingZombie:
                    {
                        Name = "Regenerating Zombie";
                        Type = CardTypes.Creature;
                        Colour = ManaColour.Black;
                        CastingCost = new ManaSet(ManaColour.Black, ManaColour.Colourless);

                        Attack = 1;
                        Health = 2;

                        Abilities = new[] {
                            GenericCreatureCast(),
                            new Ability
                            {
                                BreadText = "You may cast Regenerating Zombie from your graveyard.",
                                MoveToStackOnCast = true,

                                IsCastable = context =>
                                {
                                    if (Location != PileLocation.Graveyard) { return false; }
                                    if (Owner != context.CastingPlayer) { return false; }

                                    return ManaCostIsPayable(CastingCost, context) && ChannelsAreCastable(context);
                                },

                                MakeCastChoices = context =>
                                {
                                    return GenericPayManaChoice(CastingCost, context);
                                },

                                EnactCastChoices = context =>
                                {
                                    GenericPayManaEnact(context);
                                },
                            }
                        };
                    } break;

                case CardTemplates.MindProbe:
                    {
                        Name = "Mind Probe";
                        Type = CardTypes.Instant;
                        Colour = ManaColour.Blue;
                        CastingCost = new ManaSet(ManaColour.Blue);

                        Abilities = new[] {
                            new Ability{
                                BreadText = "Look at target player's hand then draw a card.\n",
                                MoveToStackOnCast = true,
                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost, context),
                                MakeCastChoices = context => {
                                    var manacost = MakePayManaChoice(CastingCost, context, context.CastingPlayer);
                                    if (manacost == null) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for {Name}.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChoosePlayer();
                                    if (target == null) { return false; }

                                    context.SetManaSet("manacost", manacost);
                                    context.SetPlayer("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    EnactPayManaChoice(context.GetManaSet("manacost"), context, context.CastingPlayer);
                                },
                                EnactResolveChoices = context => { 
                                    if (context.CastingPlayer == context.Hero)
                                    {
                                        var target = context.GetPlayer("!target");
                                        context.ChoiceHelper.ShowCards(target.Hand);
                                    }
                                    context.GameState.DrawCards(context.CastingPlayer, 1);
                                }
                            },
                            new Ability{
                                BreadText = $"Instead of paying {Name}'s mana cost you may pay 2 health.",
                                MoveToStackOnCast = true,
                                IsCastable = context => InHandAndOwned(context) && context.CastingPlayer.CurrentHealth > 2,
                                MakeCastChoices = context => {
                                    context.ChoiceHelper.Text = $"Choose a target for {Name}.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChoosePlayer();
                                    if (target == null) { return false; }

                                    context.SetPlayer("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    context.GameState.DealDamage(this, context.CastingPlayer.HeroCard.Token, 2);
                                },
                                EnactResolveChoices = context => {
                                    if (context.CastingPlayer == context.Hero)
                                    {
                                        var target = context.GetPlayer("!target");
                                        context.ChoiceHelper.ShowCards(target.Hand);
                                    }
                                    context.GameState.DrawCards(context.CastingPlayer, 1);
                                }
                            }
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

            var breadTextBuilder = new StringBuilder();
            foreach (var ability in Abilities)
            {
                if (ability.BreadText != null) { breadTextBuilder.AppendLine(ability.BreadText); }
            }
            foreach (var aura in Auras)
            {
                if (aura.BreadText != null) { breadTextBuilder.Append(aura.BreadText); }
            }
            foreach (var keywordAbility in KeywordAbilities.GetAbilities())
            {
                breadTextBuilder.AppendLine(keywordAbility.ToString());
            }
            BreadText = breadTextBuilder.ToString();
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

#region Helper Functions

        private ManaSet MakePayManaChoice(ManaSet cost, AbilityCastingContext context, Player player)
        {
            if (!player.CurrentMana.Covers(cost)) { return null; }

            if (cost.Colourless == 0)
            {
                return new ManaSet(cost);
            }

            if (cost.Size == player.CurrentMana.Size)
            {
                return new ManaSet(player.CurrentMana);
            }

            var pool = new ManaSet(player.CurrentMana);
            var colourlessToPay = cost.Colourless;
            var payment = new ManaSet(cost);
            payment.Colourless = 0;

            while (colourlessToPay > 0)
            {

                string paymentString;
                var paymentColours = payment.ToColourArray();
                if (paymentColours.Count == 0)
                {
                    paymentString = "Nothing yet!";
                }
                else
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var colour in paymentColours)
                    {
                        stringBuilder.Append(colour.ToString());
                        stringBuilder.Append(", ");
                    }
                    stringBuilder.Length -= 2; // Trim the trailing ", ".
                    paymentString = stringBuilder.ToString();
                }

                context.ChoiceHelper.Text =
                    $"Paying with:\n{paymentString}";
                context.ChoiceHelper.ShowCancel = true;
                var choice = context.ChoiceHelper.ChooseColour(colour =>
                {
                    if (colour == ManaColour.Colourless) { return false; }
                    var paymentCopy = new ManaSet(payment);
                    paymentCopy.IncrementColour(colour);
                    return pool.Covers(paymentCopy);
                });

                if (choice == ManaColour.None) { return null; }

                payment.IncrementColour(choice);
                colourlessToPay--;
            }

            return payment;
        }
        
        private void EnactPayManaChoice(ManaSet cost, AbilityCastingContext context, Player player)
        {
            context.GameState.SpendMana(player, cost);
        }

        private bool InHandAndOwned(AbilityCastingContext context)
        {
            if (Location != PileLocation.Hand) { return false; }
            if (Owner != context.CastingPlayer) { return false; }

            return true;
        }
        private bool ChannelsAreCastable(AbilityCastingContext context)
        {
            if (context.GameState.ActivePlayer != context.CastingPlayer) { return false; }
            if (context.GameState.CastingStack.Count > 0) { return false; }

            return true;
        }

        private bool ManaCostIsPayable(ManaSet cost, AbilityCastingContext context)
        {
            return context.CastingPlayer.CurrentMana.Covers(cost);

        }

        private Ability GenericCreatureCast()
        {
            return new Ability
            {
                MoveToStackOnCast = true,

                IsCastable = context =>
                {
                    return InHandAndOwned(context) && ManaCostIsPayable(CastingCost, context) && ChannelsAreCastable(context);
                },

                MakeCastChoices = context =>
                {
                    return GenericPayManaChoice(CastingCost, context);
                },

                EnactCastChoices = context =>
                {
                    GenericPayManaEnact(context);
                },
            };
        }

        private bool GenericPayManaChoice(ManaSet cost, AbilityCastingContext context)
        {
            var payment = MakePayManaChoice(cost, context, context.CastingPlayer);
            if (payment == null) { return false; }

            context.SetManaSet("manacost", payment);
            return true;
        }

        private void GenericPayManaEnact(AbilityCastingContext context)
        {
            var payment = context.GetManaSet("manacost");
            context.GameState.SpendMana(context.CastingPlayer, payment);
        }

#endregion
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
        AlterFate,
        MindFlay, 
        GolbinBombsmith,
        RegeneratingZombie,
        CrystalizedGeyser,

        MindProbe,

        HeroTest,
    }
}
