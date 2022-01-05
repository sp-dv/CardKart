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
        public CardRarities Rarity;

        public CreatureTypes CreatureType;

        public bool IsTokenCard => Rarity == CardRarities.Token;

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
                        CreatureType = CreatureTypes.Goblin;
                        Colour = ManaColour.Red;
                        Rarity = CardRarities.Common;
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
                        CreatureType = CreatureTypes.Zombie;
                        Colour= ManaColour.Black;
                        Rarity = CardRarities.Common;
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
                        Type = CardTypes.Scroll;
                        Colour = ManaColour.Red;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(
                            ManaColour.Red
                            );

                        Abilities = new[] {
                            new Ability {
                                MoveToStackOnCast = true,
                                BreadText = "Deal 2 damage to target creature or player.",

                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost, context),
                                MakeCastChoices = context => {
                                    var payment = MakePayManaChoice(CastingCost, context, context.CastingPlayer);
                                    if (payment == null) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for {Name}.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(token => token.IsCreature || token.IsHero);
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
                        Name = "Heromanx";
                        Type = CardTypes.Hero;
                        Colour = ManaColour.White;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet();

                        Health = 27;
                    } break;

                case CardTemplates.DepravedBloodhound:
                    {
                        Name = "Depraved Bloodhound";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Beast;
                        Colour = ManaColour.Black;
                        Rarity = CardRarities.Rare;
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
                        CreatureType = CreatureTypes.Warrior;
                        Colour = ManaColour.White;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.White, ManaColour.White, ManaColour.Colourless);

                        Attack = 2;
                        Health = 2;

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
                        Type = CardTypes.Scroll;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Common;
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

                                    context.ChoiceHelper.Text = $"Choose a target for {Name}.";
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
                        Rarity = CardRarities.Common;
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
                                    var options = context.CastingPlayer.Deck.Peek(3);
                                    context.ChoiceHelper.Text = "Choose a card to draw.";
                                    var choice = context.ChoiceHelper.ChooseCardFromOptions(options, card => true);
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
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Black);

                        Abilities = new Ability[] {
                            new Ability
                            {
                                BreadText = "Look at your opponents hand and choose a card.\nYour opponent discards the chosen card.",
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

                                MakeResolveChoicesCastingPlayer = context =>
                                {
                                    context.ChoiceHelper.Text = "Choose a card to discard.";
                                    var choice =
                                        context.ChoiceHelper.ChooseCardFromOptions(context.CastingPlayer.Opponent.Hand, card => true);
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
                        CreatureType = CreatureTypes.Goblin;
                        Colour = ManaColour.Red;
                        Rarity = CardRarities.Common;
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
                        CreatureType = CreatureTypes.Elemental;
                        Colour = ManaColour.Mixed;
                        Rarity = CardRarities.Rare;
                        CastingCost = new ManaSet(ManaColour.Red, ManaColour.Red, ManaColour.Blue, ManaColour.Blue, ManaColour.Colourless);

                        Attack = 3;
                        Health = 5;

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
                        CreatureType = CreatureTypes.Zombie;
                        Colour = ManaColour.Black;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Black, ManaColour.Colourless);

                        Attack = 1;
                        Health = 2;

                        Abilities = new[] {
                            GenericCreatureCast(),
                            new Ability
                            {
                                BreadText = $"You may cast {Name} from your graveyard.",
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
                        Type = CardTypes.Scroll;
                        Colour = ManaColour.Blue;
                        Rarity = CardRarities.Uncommon;
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

                case CardTemplates.Counterspell:
                    {
                        Name = "Counterspell";
                        Type = CardTypes.Scroll;
                        Colour = ManaColour.Blue;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Blue, ManaColour.Blue, ManaColour.Colourless);

                        Abilities = new[] {
                            new Ability{
                                BreadText = "Counter target spell.",
                                MoveToStackOnCast = true,
                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost, context),
                                MakeCastChoices = context => {
                                    var manacost = MakePayManaChoice(CastingCost, context, context.CastingPlayer);
                                    if (manacost == null) {return false; }

                                    context.ChoiceHelper.ShowCancel = true;
                                    var choice = context.ChoiceHelper.ChooseCastingContext(acc => acc.Ability.MoveToStackOnCast);
                                    if (choice == null) { return false; }
                                    var contextIndex = context.GameState.CastingStack.IndexOf(choice);

                                    context.SetManaSet("manacost", manacost);
                                    context.Choices.Singletons["contextIndex"] = contextIndex;
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    EnactPayManaChoice(context.GetManaSet("manacost"), context, context.CastingPlayer);
                                },
                                EnactResolveChoices = context => {
                                    var counteredContextIndex = context.Choices.Singletons["contextIndex"];
                                    var counteredContext = context.GameState.CastingStack.GetAtIndex(counteredContextIndex);
                                    context.GameState.Counterspell(this, counteredContext);
                                }
                            }
                        };

                    } break;

                case CardTemplates.MindSlip:
                    {
                        Name = "Mind Slip";
                        Type = CardTypes.Scroll;
                        Colour = ManaColour.Blue;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Blue, ManaColour.Blue);

                        Abilities = new[] {
                            new Ability{
                                BreadText = "Counter target spell unless it's owner pays 2 colorless mana.",
                                MoveToStackOnCast = true,
                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost, context),
                                MakeCastChoices = context => {
                                    var manacost = MakePayManaChoice(CastingCost, context, context.CastingPlayer);
                                    if (manacost == null) {return false; }

                                    context.ChoiceHelper.ShowCancel = true;
                                    var choice = context.ChoiceHelper.ChooseCastingContext(acc => acc.Ability.MoveToStackOnCast);
                                    if (choice == null) { return false; }
                                    var contextIndex = context.GameState.CastingStack.IndexOf(choice);

                                    context.SetManaSet("manacost", manacost);
                                    context.Choices.Singletons["contextIndex"] = contextIndex;
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    EnactPayManaChoice(context.GetManaSet("manacost"), context, context.CastingPlayer);
                                },
                                MakeResolveChoicesNonCastingPlayer = context => {
                                    var hostagePayment = MakePayManaChoice(new ManaSet(ManaColour.Colourless, ManaColour.Colourless), context, context.CastingPlayer.Opponent);
                                    if (hostagePayment != null) 
                                    {
                                        context.SetManaSet("hostagePayment", hostagePayment);
                                    }
                                },
                                EnactResolveChoices = context => {

                                    if (context.Choices.Arrays.ContainsKey("hostagePayment"))
                                    {
                                        var hostagePayment = context.GetManaSet("hostagePayment");
                                        EnactPayManaChoice(hostagePayment, context, context.CastingPlayer.Opponent);
                                    }
                                    else
                                    {
                                        var counteredContextIndex = context.Choices.Singletons["contextIndex"];
                                        var counteredContext = context.GameState.CastingStack.GetAtIndex(counteredContextIndex);
                                        context.GameState.Counterspell(this, counteredContext);
                                    }
                                }
                            }
                        };
                    } break;

                case CardTemplates.SuckerPunch:
                    {
                        Name = "Sucker Punch";
                        Type = CardTypes.Scroll;
                        Colour = ManaColour.Purple;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Purple);

                        Abilities = new[] {
                            new Ability {
                                MoveToStackOnCast = true,
                                BreadText = $"As an additional cost to casting {Name}; discard a card.\nDeal 4 damage to target creature.",

                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost, context),
                                MakeCastChoices = context => {
                                    var payment = MakePayManaChoice(CastingCost, context, context.CastingPlayer);
                                    if (payment == null) { return false; }

                                    context.ChoiceHelper.Text = "Choose a card to discard.";
                                    var discardCard = context.ChoiceHelper.ChooseCardFromOptions(context.CastingPlayer.Hand, card => card != this);
                                    if (discardCard == null) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for {Name}.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(token => !token.IsHero);
                                    if (target == null) { return false; }

                                    context.SetManaSet("manacost", payment);
                                    context.SetCard("discard", discardCard);
                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    GenericPayManaEnact(context);
                                    context.GameState.MoveCard(context.GetCard("discard"), context.CastingPlayer.Graveyard);
                                },
                                EnactResolveChoices = context =>
                                {
                                    var target = context.GetToken("!target");
                                    context.GameState.DealDamage(this, target, 4);
                                }
                            }
                        };
                    } break;

                case CardTemplates.ScribeMagi:
                    {
                        Name = "Scribe Magi";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Wizard;
                        Colour = ManaColour.Blue;
                        Rarity = CardRarities.Rare;
                        CastingCost = new ManaSet(ManaColour.Blue, ManaColour.Blue);

                        Attack = 2;
                        Health = 1;

                        Abilities = new[] {
                            GenericCreatureCast(),
                            new TriggeredAbility {
                                BreadText = $"When {Name} enters the battlefield; choose a Scroll or Channel from your graveyard and put it in your hand.",

                                IsTriggeredBy = trigger =>
                                {
                                    if (trigger is MoveTrigger)
                                    {
                                        var moveTrigger = trigger as MoveTrigger;
                                        return moveTrigger.Card == this && moveTrigger.To.Location == PileLocation.Battlefield;
                                    }

                                    return false;
                                },
                                MakeCastChoices = context => {
                                    context.ChoiceHelper.Text = "Choose a card to put in your hand.";
                                    var choice = context.ChoiceHelper.ChooseCardFromOptions(
                                        context.CastingPlayer.Graveyard, 
                                        card => card.Type == CardTypes.Scroll || card.Type == CardTypes.Channel);
                                    if (choice == null) { return false; }

                                    context.SetCard("!return", choice);
                                    return true;
                                },
                                EnactResolveChoices = context => {
                                    context.GameState.MoveCard(context.GetCard("!return"), context.CastingPlayer.Hand);
                                },
                            },
                        };
                    } break;

                case CardTemplates.Unmake:
                    {
                        Name = "Unmake";
                        Type = CardTypes.Scroll;
                        Colour = ManaColour.Blue;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Blue);

                        Abilities = new[] {
                            new Ability {
                                MoveToStackOnCast = true,
                                BreadText = "Return target creature to its owners hand.",

                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost, context),
                                MakeCastChoices = context => {
                                    var payment = MakePayManaChoice(CastingCost, context, context.CastingPlayer);
                                    if (payment == null) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for {Name}.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(token => token.IsCreature);
                                    if (target == null) { return false; }

                                    context.SetManaSet("manacost", payment);
                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => GenericPayManaEnact(context),
                                EnactResolveChoices = context =>
                                {
                                    var target = context.GetToken("!target");
                                    context.GameState.MoveCard(target.TokenOf, target.TokenOf.Owner.Hand);
                                }
                            }
                        };
                    } break;

                case CardTemplates.HorsemanOfDeath:
                    {
                        Name = "Horseman of Death";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Warrior;
                        Colour = ManaColour.Black;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet(ManaColour.Black, ManaColour.Black, ManaColour.Black, ManaColour.Colourless, ManaColour.Colourless);

                        Attack = 3;
                        Health = 6;

                        Abilities = new[] {
                            GenericCreatureCast(),
                            new TriggeredAbility {
                                BreadText = $"When {Name} enters the battlefield; destroy target creature your opponent controls.",

                                IsTriggeredBy = trigger =>
                                {
                                    if (trigger is MoveTrigger)
                                    {
                                        var moveTrigger = trigger as MoveTrigger;
                                        return moveTrigger.Card == this && moveTrigger.To.Location == PileLocation.Battlefield;
                                    }

                                    return false;
                                },
                                MakeCastChoices = context => {
                                    // Abort if opponent has no creature to destroy.
                                    if (context.CastingPlayer.Opponent.Battlefield.Where(card => card.Type == CardTypes.Creature).Count() == 0) { return false; }
                                    context.ChoiceHelper.Text = "Choose a creature to destroy.";
                                    var choice = context.ChoiceHelper.ChooseToken(token => token.IsCreature && token.Controller == context.CastingPlayer.Opponent);
                                    if (choice == null) { return false; }

                                    context.SetToken("!target", choice);
                                    return true;
                                },
                                EnactResolveChoices = context => {
                                    context.GameState.MoveCard(context.GetToken("!target").TokenOf, context.CastingPlayer.Graveyard);
                                },
                            },
                        };

                    } break;

                case CardTemplates.SquireToken1:
                    {
                        Name = "Squire";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Human;
                        Colour = ManaColour.White;
                        Rarity = CardRarities.Token;
                        CastingCost = new ManaSet();

                        Attack = 1;
                        Health = 1;

                    } break;

                case CardTemplates.CallToArms:
                    {
                        Name = "Call to Arms";
                        Type = CardTypes.Channel;
                        Colour = ManaColour.White;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.White);

                        Abilities = new[] {
                            new Ability{
                                BreadText = "Summon two 1/1 white Squire tokens.",
                                MoveToStackOnCast = true,

                                IsCastable = context => {
                                    return InHandAndOwned(context) &&
                                        ChannelsAreCastable(context) &&
                                        ManaCostIsPayable(CastingCost, context);
                                },
                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(CastingCost, context);
                                },
                                EnactCastChoices = context => {
                                    GenericPayManaEnact(context);
                                },
                                EnactResolveChoices = context => {
                                    context.GameState.SummonToken(CardTemplates.SquireToken1, context.CastingPlayer);
                                    context.GameState.SummonToken(CardTemplates.SquireToken1, context.CastingPlayer);
                                }
                            }
                        };
                    } break;

                case CardTemplates.ExiledScientist:
                    {
                        Name = "Hermit Scientist";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Artificer;
                        Colour = ManaColour.Purple;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Purple);

                        Attack = 1;
                        Health = 1;

                        Abilities = new[] {
                            GenericCreatureCast(),
                            new TriggeredAbility{
                                BreadText = $"At the start of your turn reveal the top card of your deck. If you reveal a scroll or a sorcery; destroy {Name} and summon a 3/2 Insect token with Flying.",
                                IsTriggeredBy = trigger => {
                                    if (trigger is GameTimeTrigger)
                                    {
                                        var gameTimeTrigger = trigger as GameTimeTrigger;
                                        if (Location == PileLocation.Battlefield && 
                                            gameTimeTrigger.Time == GameTime.StartOfTurn && 
                                            gameTimeTrigger.ActivePlayer == Token.Controller) { return true; }
                                    }

                                    return false;
                                },
                                
                                MakeCastChoices = context => {
                                    return true;
                                },

                                EnactResolveChoices = context => {
                                    var topCard = context.CastingPlayer.Deck.Peek(1).ElementAtOrDefault(0);
                                    if (topCard == null) { return; }

                                    context.ChoiceHelper.ShowCards(new []{topCard});

                                    if (topCard.Type == CardTypes.Scroll || topCard.Type == CardTypes.Channel)
                                    {
                                        context.GameState.SummonToken(CardTemplates.InsectToken1, Token.Controller);
                                        context.GameState.MoveCard(this, Owner.Graveyard);
                                    }
                                }
                                
                            }
                        };
                    } break;

                case CardTemplates.InsectToken1:
                    {
                        Name = "Insect";
                        Type = CardTypes.Creature;
                        Colour = ManaColour.Purple;
                        CreatureType = CreatureTypes.Insect;
                        Rarity = CardRarities.Token;
                        CastingCost = new ManaSet();

                        Attack = 3;
                        Health = 2;

                        KeywordAbilities[KeywordAbilityNames.Flying] = true;

                    } break;

                case CardTemplates.SilvervenomSpider:
                    {
                        Name = "Silvervenom Spider";
                        Type = CardTypes.Creature;
                        Colour = ManaColour.Green;
                        CreatureType = CreatureTypes.Spider;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Green, ManaColour.Green);

                        Attack = 2;
                        Health = 3;

                        KeywordAbilities[KeywordAbilityNames.Range] = true;

                        Abilities = new[] {
                            GenericCreatureCast(),
                        };
                    } break;

                case CardTemplates.Eliminate:
                    {
                        Name = "Eliminate";
                        Type = CardTypes.Scroll;
                        Colour = ManaColour.Black;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Black);

                        Abilities = new[] {
                            new Ability {
                                MoveToStackOnCast = true,
                                BreadText = "Destroy target creature.",

                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost, context),
                                MakeCastChoices = context => {
                                    var payment = MakePayManaChoice(CastingCost, context, context.CastingPlayer);
                                    if (payment == null) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for {Name}.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(token => token.IsCreature);
                                    if (target == null) { return false; }

                                    context.SetManaSet("manacost", payment);
                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => GenericPayManaEnact(context),
                                EnactResolveChoices = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }

                                    context.GameState.MoveCard(target.TokenOf, target.TokenOf.Owner.Graveyard);
                                }
                            }
                        };
                    } break;

                case CardTemplates.Inspiration:
                    {
                        Name = "Inspiration";
                        Type = CardTypes.Channel;
                        Colour = ManaColour.Purple;
                        Rarity = CardRarities.Rare;
                        CastingCost = new ManaSet(ManaColour.Purple);

                        Abilities = new[] {
                            new Ability {
                                MoveToStackOnCast = true,
                                BreadText = "Gain 3 Purple mana until the start of your next turn.",

                                IsCastable = context => ChannelsAreCastable(context) && 
                                    InHandAndOwned(context) && 
                                    ManaCostIsPayable(CastingCost, context),

                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(CastingCost, context);
                                },

                                EnactCastChoices = context => GenericPayManaEnact(context),
                                EnactResolveChoices = context =>
                                {
                                    context.GameState.GainTemporaryMana(context.CastingPlayer, ManaColour.Purple, 3);
                                }
                            }
                        };
                    } break;

                case CardTemplates.CourtInformant:
                    {
                        Name = "Court Informant";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Spy;
                        Colour = ManaColour.Purple;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Purple, ManaColour.Colourless);

                        Attack = 3;
                        Health = 2;

                        Abilities = new[] {
                            GenericCreatureCast(),
                            new TriggeredAbility {
                                BreadText = $"Whenever {Name} deals damage to a player; draw a card.",
                                IsTriggeredBy = trigger => {
                                    if (trigger is DamageDoneTrigger)
                                    {
                                        var damageDoneTrigger = trigger as DamageDoneTrigger;

                                        if (damageDoneTrigger.Source == this && damageDoneTrigger.Target.IsHero)
                                        {
                                            return true;
                                        }
                                    }
                                    
                                    return false;
                                },

                                EnactResolveChoices = context => {
                                    context.GameState.DrawCards(context.CastingPlayer, 1);
                                }
                            }
                        };

                    } break;

                case CardTemplates.BattlehardenedMage:
                    {
                        Name = "Battlehardened Mage";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Warrior;
                        Colour = ManaColour.Blue;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Blue, ManaColour.Colourless);

                        Attack = 1;
                        Health = 3;

                        Abilities = new[] {
                            GenericCreatureCast(),
                            new TriggeredAbility {
                                BreadText = $"Whenever {Name} takes damage; draw a card.",
                                IsTriggeredBy = trigger => {
                                    if (trigger is DamageDoneTrigger)
                                    {
                                        var damageDoneTrigger = trigger as DamageDoneTrigger;

                                        if (this.Token != null && damageDoneTrigger.Target == this.Token)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                },

                                EnactResolveChoices = context => {
                                    context.GameState.DrawCards(context.CastingPlayer, 1);
                                }
                            }
                        };
                    } break;

                case CardTemplates.ArcticWatchman:
                    {
                        Name = "Arctic Watchman";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Hunter;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Green);

                        Attack = 2;
                        Health = 3;

                        Abilities = new[] {
                            GenericCreatureCast(),
                            new TriggeredAbility {
                                BreadText = $"When {Name} enters the battlefield; summon a Green 2/1 Hawk token with flying.",
                                IsTriggeredBy = trigger => {
                                    if (trigger is MoveTrigger)
                                    {
                                        var moveTrigger = trigger as MoveTrigger;
                                        if (moveTrigger.Card == this && moveTrigger.To.Location == PileLocation.Battlefield)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                },
                                EnactResolveChoices = context => {
                                    context.GameState.SummonToken(CardTemplates.HawkToken1, Owner);
                                }
                            }
                        };

                    } break;

                case CardTemplates.HawkToken1:
                    {
                        Name = "Hawk";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Bird;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Token;
                        CastingCost = new ManaSet();

                        Attack = 2;
                        Health = 1;

                        KeywordAbilities[KeywordAbilityNames.Flying] = true;
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
        Scroll,
        Channel,
        Relic,
        Hero,
    }

    public enum CreatureTypes
    {
        None, 

        Zombie,
        Human,
        Insect,
        Spider,
        Goblin,
        Warrior,
        Elemental,
        Beast,
        Wizard,
        Artificer,
        Spy,
        Hunter,
        Bird,

    }

    public enum CardTemplates
    {
        None,

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
        Counterspell,
        MindSlip,
        SuckerPunch,
        ScribeMagi,
        Unmake,
        HorsemanOfDeath,
        SquireToken1,
        CallToArms,
        ExiledScientist,
        InsectToken1,
        SilvervenomSpider,
        Eliminate,
        Inspiration,
        CourtInformant,
        BattlehardenedMage,
        ArcticWatchman,
        HawkToken1,

        HeroTest,
    }

    public enum CardRarities
    {
        None, 

        Common,
        Uncommon,
        Rare,
        Legendary,

        Token,
    }
}
