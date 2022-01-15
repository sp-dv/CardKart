using CardKartShared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CardKartShared.GameState
{
    public class Card : GameObject
    {
        public static IEnumerable<Card> AllRealCards = 
            Enum.GetValues(typeof(CardTemplates))
            .Cast<CardTemplates>()
            .Where(template => template != CardTemplates.None)
            .Select(template => new Card(template));

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
        public string BreadTextLong { get; }

        public CardSets CardSet;

        private bool IsUncollectible;
        public bool IsInPacks => !IsUncollectible && Rarity != CardRarities.Token;

        public Card(CardTemplates template)
        {
            Template = template;

            string flavourText = null;

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
                        CastingCost = new ManaSet(ManaColour.Red, ManaColour.Colourless);

                        Attack = 3;
                        Health = 2;
                        KeywordAbilities[KeywordAbilityNames.Bloodlust] = true;

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                        };
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

                        Attack = 2;
                        Health = 3;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility {
                                BreadText = $"Whenever a creature dies; {Name} gets +0/+1.",
                                IsTriggeredBy = AnyCreatureDies,
                                MakeCastChoices = context => {
                                    if (Token == null) { return false; } // Paranoia. 'Should' never happen.
                                    // Do this to ensure fizzling
                                    context.SetToken("t", Token);
                                    return true;
                                },
                                Resolve = context => {
                                    var token = context.GetToken("t");
                                    if (!token.IsValid) { return; }

                                    token.Auras.Add(new Aura
                                    {
                                        ApplyAura = (token, _) => token.AuraModifiers.Health += 1,
                                        IsCancelledBy = (_, token, __) => false
                                    });
                                }
                            }
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

                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost),
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
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }
                                    context.GameState.DealDamage(this, target, 2);
                                }
                            }
                        };
                    } break;

                

                case CardTemplates.DepravedBloodhound:
                    {
                        Name = "Depraved Bloodhound";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Beast;
                        Colour = ManaColour.Black;
                        Rarity = CardRarities.Rare;
                        CastingCost = new ManaSet(ManaColour.Black, ManaColour.Black, ManaColour.Black);

                        Attack = 3;
                        Health = 4;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility {
                                BreadText = "Whenever your opponent draws a card; deal 1 damage to them.",

                                IsTriggeredBy = trigger =>
                                {
                                    if (trigger is DrawTrigger)
                                    {
                                        var drawTrigger = trigger as DrawTrigger;

                                        if (drawTrigger.Player == Controller) { return true; }
                                    }

                                    return false;
                                },
                                SaveTriggerInfo = (trigger, context) => {
                                    var drawTrigger = trigger as DrawTrigger;
                                    context.SetPlayer("player", drawTrigger.Player);
                                },
                                Resolve = context => {
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
                        Rarity = CardRarities.Rare;
                        CastingCost = new ManaSet(ManaColour.White, ManaColour.White, ManaColour.Colourless);

                        Attack = 2;
                        Health = 2;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
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
                                BreadText = "Target creature gets +2/+2 and Range.",

                                IsCastable =  context =>
                                {
                                    return InHandAndOwned(context) &&
                                        ManaCostIsPayable(CastingCost);
                                },
                                MakeCastChoices = context =>
                                {
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
                                EnactCastChoices = context =>
                                {
                                    context.GameState.SpendMana(
                                        context.CastingPlayer,
                                        context.GetManaSet("manacost"));
                                },
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }

                                    target.Auras.Add(new Aura
                                    {
                                        ApplyAura = (token, gameState) =>
                                        {
                                            token.AuraModifiers.Attack += 2;
                                            token.AuraModifiers.Health += 2;
                                            token.AuraModifiers.Keywords[KeywordAbilityNames.Range] = true;
                                        },
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
                                BreadText = "Look at the top three cards of your deck then put them back in any order.\nYou may shuffle your deck.\nDraw a card.",
                                IsCastable = context =>
                                {
                                    return
                                        InHandAndOwned(context) &&
                                        ChannelsAreCastable(context) &&
                                        ManaCostIsPayable(CastingCost);

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
                                    var options = context.CastingPlayer.Deck.Peek(3).ToArray();
                                    var optionsCount = options.Length;

                                    var putback = new List<Card>();

                                    while (true)
                                    {
                                        if (putback.Count == optionsCount) { break; }

                                        if (putback.Count > 0) { context.ChoiceHelper.ShowCancel = true; }

                                        context.ChoiceHelper.Text = "Choose card to put on top of your deck.";
                                        var choice = context.ChoiceHelper.ChooseCardFromOptions(options, card => !putback.Contains(card));
                                        if (choice == null)
                                        {
                                            putback.Clear();
                                        }
                                        else
                                        {
                                            putback.Add(choice);
                                        }
                                    }

                                    context.ChoiceHelper.ShowYes = true;
                                    context.ChoiceHelper.ShowNo = true;
                                    context.ChoiceHelper.Text = "Shuffle your deck?";
                                    var shuffleChoice = context.ChoiceHelper.ChooseOption();

                                    if (shuffleChoice == OptionChoice.Yes)
                                    {
                                        context.SetArray("order", new int[0]);
                                    }
                                    else
                                    {
                                        var ids = putback.Select(card => card.ID).ToArray();
                                        context.SetArray("order", ids);
                                    }
                                },
                                Resolve = context =>
                                {
                                    var ids = context.GetArray("order");
                                    if (ids.Length == 0)
                                    {
                                        context.GameState.ShuffleDeck(context.CastingPlayer);
                                    }
                                    else
                                    {
                                        var cards = ids.Select(id => context.GameState.GetByID(id) as Card);

                                        foreach (var card in cards)
                                        {
                                            context.GameState.MoveCard(card, card.Owner.Banished);
                                            context.GameState.MoveCard(card, card.Owner.Deck);
                                        }
                                    }

                                    context.GameState.DrawCards(context.CastingPlayer, 1);
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
                                        ManaCostIsPayable(CastingCost);
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

                                Resolve = context =>
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
                            GenericCreatureOrRelicCast(),
                            new Ability {
                                BreadText = "Exhaust: Deal 1 damage to target creature or player.",
                                IsCastable = context => Location == PileLocation.Battlefield && !Token.IsExhausted && context.CastingPlayer == Token.Controller,
                                MakeCastChoices = context => {
                                    context.ChoiceHelper.Text = "Choose a target for Throw Bomb.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(token => true);
                                    if (target == null) { return false; }

                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    context.GameState.ExhaustToken(Token);
                                },
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }
                                    context.GameState.DealDamage(this, target, 1);
                                }
                            }
                        };

                        flavourText = "\"He makes the finest bombs I've seen in my life.\" -- Ali Arbas";
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
                            GenericCreatureOrRelicCast(),
                            new Ability {
                                BreadText = "Exhaust, Pay 1 blue mana: Draw a card.\n",
                                IsCastable = context => 
                                    Location == PileLocation.Battlefield && 
                                    !Token.IsExhausted && 
                                    context.CastingPlayer == Token.Controller &&
                                    ManaCostIsPayable(abilityAManacost),
                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(abilityAManacost, context);
                                },
                                EnactCastChoices = context => {
                                    GenericPayManaEnact(context);
                                    Token.IsExhausted = true;
                                    context.SetPlayer("drawer", context.CastingPlayer);
                                },
                                Resolve = context =>
                                {
                                    var drawer = context.GetPlayer("drawer");
                                    context.GameState.DrawCards(drawer, 1);
                                }
                            },
                            new Ability {
                                BreadText = "Exhaust, Pay 1 red mana: Deal 2 damage to target creature or player.",
                                IsCastable = context => 
                                    Location == PileLocation.Battlefield && 
                                    !Token.IsExhausted && 
                                    context.CastingPlayer == Token.Controller
                                    && ManaCostIsPayable(abilityBManacost),
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
                                    Token.IsExhausted = true;
                                },
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (target.IsValid)
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
                            GenericCreatureOrRelicCast(),
                            new Ability
                            {
                                BreadText = $"You may cast {Name} from your graveyard.",
                                MoveToStackOnCast = true,

                                IsCastable = context =>
                                {
                                    if (Location != PileLocation.Graveyard) { return false; }
                                    if (Owner != context.CastingPlayer) { return false; }

                                    return ManaCostIsPayable(CastingCost) && ChannelsAreCastable(context);
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
                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost),
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
                                Resolve = context => { 
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
                                Resolve = context => {
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
                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost),
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
                                Resolve = context => {
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
                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost),
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
                                Resolve = context => {

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

                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost),
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
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }
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
                            GenericCreatureOrRelicCast(),
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
                                Resolve = context => {
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

                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost),
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
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }
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
                            GenericCreatureOrRelicCast(),
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
                                Resolve = context => {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }
                                    context.GameState.MoveCard(target.TokenOf, context.CastingPlayer.Graveyard);
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
                                        ManaCostIsPayable(CastingCost);
                                },
                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(CastingCost, context);
                                },
                                EnactCastChoices = context => {
                                    GenericPayManaEnact(context);
                                },
                                Resolve = context => {
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
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility{
                                BreadText = $"At the start of your turn reveal the top card of your deck. If you reveal a scroll or a sorcery; destroy {Name} and summon a Purple 3/2 Mechanical token with Flying.",
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

                                Resolve = context => {
                                    if (context.CastingPlayer.Deck.Count == 0) { return; }
                                    var topCard = context.CastingPlayer.Deck.Peek(1).ElementAtOrDefault(0);

                                    context.ChoiceHelper.ShowCards(new []{topCard});

                                    if (topCard.Type == CardTypes.Scroll || topCard.Type == CardTypes.Channel)
                                    {
                                        context.GameState.SummonToken(CardTemplates.MechanicalToken1, Token.Controller);
                                        context.GameState.MoveCard(this, Owner.Graveyard);
                                    }
                                }
                                
                            }
                        };
                    } break;

                case CardTemplates.MechanicalToken1:
                    {
                        Name = "Mechanical";
                        Type = CardTypes.Creature;
                        Colour = ManaColour.Purple;
                        CreatureType = CreatureTypes.Mechanical;
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
                            GenericCreatureOrRelicCast(),
                        };
                    } break;

                case CardTemplates.Eliminate:
                    {
                        Name = "Eliminate";
                        Type = CardTypes.Scroll;
                        Colour = ManaColour.Black;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Black, ManaColour.Black, ManaColour.Colourless);

                        Abilities = new[] {
                            new Ability {
                                MoveToStackOnCast = true,
                                BreadText = "Destroy target creature.",

                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost),
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
                                Resolve = context =>
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
                                    ManaCostIsPayable(CastingCost),

                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(CastingCost, context);
                                },

                                EnactCastChoices = context => GenericPayManaEnact(context),
                                Resolve = context =>
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
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility {
                                BreadText = $"Whenever {Name} deals damage to a player; draw a card.",
                                IsTriggeredBy = ThisDealsDamageTrigger,
                                Resolve = context => {
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
                            GenericCreatureOrRelicCast(),
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

                                Resolve = context => {
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
                        CastingCost = new ManaSet(ManaColour.Green, ManaColour.Green, ManaColour.Colourless, ManaColour.Colourless);

                        Attack = 3;
                        Health = 4;

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility {
                                BreadText = $"When {Name} enters the battlefield; summon a Green 2/1 Hawk token with flying.",
                                IsTriggeredBy = BattlecryTrigger,
                                Resolve = context => {
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

                case CardTemplates.SavingGrace:
                    {
                        Name = "Saving Grace";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Angel;
                        Colour = ManaColour.White;
                        Rarity = CardRarities.Rare;
                        CastingCost = new ManaSet(ManaColour.White, ManaColour.White, ManaColour.White, ManaColour.Colourless);

                        Attack = 2;
                        Health = 4;

                        KeywordAbilities[KeywordAbilityNames.Reinforcement] = true;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility {
                                BreadText = $"When {Name} enters the battlefield; you may remove a non-Angel creature you control from the game then return it to the battlefield.",
                                IsTriggeredBy = BattlecryTrigger,
                                MakeCastChoices = context => {
                                    if (context.CastingPlayer.Battlefield.Where(card => card.CreatureType != CreatureTypes.Angel).Count() == 0) { return false; }

                                    context.ChoiceHelper.Text = "Choose a creature to bounce.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var choice = context.ChoiceHelper.ChooseToken(token => token.TokenOf.CreatureType != CreatureTypes.Angel);
                                    if (choice == null) { return false; }

                                    context.SetToken("!bounce", choice);
                                    return true;
                                },
                                Resolve = context => {
                                    if (!context.Choices.Singletons.ContainsKey("!bounce")) { return; }
                                    var bounce = context.GetToken("!bounce");
                                    var card = bounce.TokenOf;
                                    if (card == null) { return; }

                                    context.GameState.MoveCard(card, card.Owner.Banished);
                                    context.GameState.MoveCard(card, card.Owner.Battlefield);
                                }
                            }
                        };

                    } break;

                case CardTemplates.DeepSeaMermaid:
                    {
                        Name = "Deep Sea Mermaid";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Mermaid;
                        Colour = ManaColour.Blue;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Blue, ManaColour.Colourless);

                        Attack = 1;
                        Health = 1;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility{
                                BreadText = $"When {Name} enters the battlefield; draw a card.",
                                IsTriggeredBy = BattlecryTrigger,
                                Resolve = context => {
                                    context.GameState.DrawCards(context.CastingPlayer, 1);
                                }
                            }
                        };

                    } break;

                case CardTemplates.Conflagrate:
                    {
                        Name = "Conflagrate";
                        Type = CardTypes.Channel;
                        Colour = ManaColour.Red;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Red, ManaColour.Red);

                        Abilities = new Ability[] {
                            new Ability {
                                MoveToStackOnCast = true,
                                BreadText = "Deal 2 damage to all creatures.",
                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost) && ChannelsAreCastable(context),
                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(CastingCost, context);
                                },
                                EnactCastChoices = context => {
                                    GenericPayManaEnact(context);
                                },
                                Resolve = context => {
                                    var tokens = context.GameState.AllTokens.Where(token => token.IsCreature).ToArray();
                                    foreach (var token in tokens) 
                                    {
                                        context.GameState.DealDamage(this, token, 2);
                                    }
                                }
                                
                            }
                        };

                    } break;

                case CardTemplates.Rapture:
                    {
                        Name = "Rapture";
                        Type = CardTypes.Channel;
                        Colour = ManaColour.White;
                        Rarity = CardRarities.Rare;
                        CastingCost = new ManaSet(ManaColour.White, ManaColour.White, ManaColour.Colourless, ManaColour.Colourless);

                        Abilities = new Ability[] {
                            new Ability {
                                MoveToStackOnCast = true,
                                BreadText = "Remove all creatures and relics on the battlefield from the game.",
                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost) && ChannelsAreCastable(context),
                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(CastingCost, context);
                                },
                                EnactCastChoices = context => {
                                    GenericPayManaEnact(context);
                                },
                                Resolve = context => {
                                    var tokens = context.GameState.AllTokens.Where(token => token.IsCreature || token.IsRelic).ToArray();
                                    foreach (var token in tokens)
                                    {
                                        if (!token.IsValid) { continue; }
                                        context.GameState.MoveCard(token.TokenOf, token.TokenOf.Owner.Banished);
                                    }
                                }

                            }
                        };

                    } break;

                case CardTemplates.Overcharge:
                    {
                        Name = "Overcharge";
                        Type = CardTypes.Scroll;
                        Colour = ManaColour.Red;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Red, ManaColour.Colourless);

                        Abilities = new[] {
                            new Ability {
                                MoveToStackOnCast = true,
                                BreadText = "Deal 3 damage to target creature and 2 damage to its controller.",

                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost),
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
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }

                                    context.GameState.DealDamage(this, target, 3);
                                    context.GameState.DealDamage(this, target.Controller.HeroToken, 2);
                                }
                            }
                        };
                    } break;

                case CardTemplates.ControlBoar:
                    {
                        Name = "Control Boar";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Mechanical;
                        Colour = ManaColour.Colourless;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Colourless);

                        Attack = 1;
                        Health = 1;

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                        };
                        KeywordAbilities[KeywordAbilityNames.Bloodlust] = true;
                    } break;

                case CardTemplates.Seblastian:
                    {
                        Name = "Seblastian";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Human;
                        Colour = ManaColour.Red;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet(ManaColour.Red, ManaColour.Red, ManaColour.Red);

                        Attack = 4;
                        Health = 2;

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility {
                                BreadText = $"When {Name} dies; deal 4 damage to your opponent.",
                                IsTriggeredBy = DeathrattleTrigger,
                                Resolve = context => {
                                    context.GameState.DealDamage(this, context.CastingPlayer.Opponent.HeroToken, 4);
                                }
                            }
                        };
                    } break;

                case CardTemplates.HauntedChapel:
                    {
                        Name = "Haunted Chapel";
                        Type = CardTypes.Relic;
                        Colour = ManaColour.Mixed;
                        Rarity = CardRarities.Rare;
                        CastingCost = new ManaSet(ManaColour.Black, ManaColour.White, ManaColour.Colourless);

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility {
                                BreadText = "Whenever a creature dies; summon a Black 1/1 Ghost token with Flying.",
                                IsTriggeredBy = AnyCreatureDies,
                                Resolve = context => {
                                    context.GameState.SummonToken(CardTemplates.GhostToken1, context.CastingPlayer);
                                }
                            }
                        };
                    } break;

                case CardTemplates.GhostToken1:
                    {
                        Name = "Ghost";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Spirit;
                        Colour = ManaColour.Black;
                        Rarity = CardRarities.Token;
                        CastingCost = new ManaSet();

                        Attack = 1;
                        Health = 1;

                        KeywordAbilities[KeywordAbilityNames.Flying] = true;
                    } break;

                case CardTemplates.PortalJumper:
                    {
                        Name = "Portal Jumper";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Human;
                        Colour = ManaColour.Purple;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Purple, ManaColour.Purple);

                        Attack = 2;
                        Health = 1;

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility{
                                BreadText = $"When {Name} enters the battlefield; reveal the top card of your deck. If the revealed card is a Creature with a mana cost of 1: put it on the battlefield.",
                                IsTriggeredBy = BattlecryTrigger,
                                Resolve = context => {
                                    var topCard = context.CastingPlayer.Deck.TopCard();
                                    if (topCard == null) { return; }

                                    context.ChoiceHelper.ShowCards(new []{ topCard });
                                    if (topCard.Type == CardTypes.Creature && topCard.CastingCost.Size == 1)
                                    {
                                        context.GameState.MoveCard(topCard, topCard.Owner.Battlefield);
                                    }
                                }
                            }
                        };
                    } break;

                case CardTemplates.PalaceGuard:
                    {
                        Name = "Palace Guard";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Warrior;
                        Colour = ManaColour.White;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.White, ManaColour.White);

                        Attack = 2;
                        Health = 3;

                        KeywordAbilities[KeywordAbilityNames.Vigilance] = true;

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                        };

                    } break;

                case CardTemplates.Darkness:
                    {
                        Name = "Darkness";
                        Type = CardTypes.Channel;
                        Colour = ManaColour.Black;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Black, ManaColour.Black, ManaColour.Black, ManaColour.Black);

                        Abilities = new[] {
                            new Ability{
                                MoveToStackOnCast = true,
                                BreadText = "Destroy all creatures.",
                                IsCastable = context => InHandAndOwned(context) && ChannelsAreCastable(context) && ManaCostIsPayable(CastingCost),
                                MakeCastChoices = context => GenericPayManaChoice(CastingCost, context),
                                EnactCastChoices = GenericPayManaEnact,
                                Resolve = context => {
                                    var toKill = context.GameState.AllTokens.Where(token => token.IsValid && token.IsCreature).ToArray();
                                    foreach (var token in toKill)
                                    {
                                        context.GameState.MoveCard(token.TokenOf, token.TokenOf.Owner.Graveyard);
                                    }
                                }
                            }
                        };
                    } break;

                case CardTemplates.MistFiend:
                    {
                        Name = "Mist Fiend";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Spirit;
                        Colour = ManaColour.Blue;
                        Rarity = CardRarities.Rare;
                        CastingCost = new ManaSet(ManaColour.Blue, ManaColour.Blue, ManaColour.Colourless, ManaColour.Colourless, ManaColour.Colourless);

                        Attack = 4;
                        Health = 4;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility{
                                BreadText = $"When {Name} enters the battlefield; return all other creatures to their owners hand.",
                                IsTriggeredBy = BattlecryTrigger,
                                Resolve = context => {
                                    var toMove = context.GameState.AllTokens.Where(token => token.IsValid && token.IsCreature).ToArray();
                                    foreach (var token in toMove)
                                    {
                                        context.GameState.MoveCard(token.TokenOf, token.TokenOf.Owner.Hand);
                                    }
                                }
                            }
                        };

                    } break;

                case CardTemplates.HorsemanOfFamine:
                    {
                        Name = "Horseman of Famine";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Warrior;
                        Colour = ManaColour.Blue;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet(ManaColour.Blue, ManaColour.Blue, ManaColour.Blue, ManaColour.Colourless, ManaColour.Colourless);

                        Attack = 3;
                        Health = 6;

                        Abilities = new[] { 
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility{
                                BreadText = $"When {Name} enters the battlefield; stun all creatures your opponent controls.",
                                IsTriggeredBy = BattlecryTrigger,
                                Resolve = context => {
                                    var toNerf = context.GameState.AllTokens.Where(token => token.IsValid && token.IsCreature && token.Controller != context.CastingPlayer).ToArray();
                                    foreach (var token in toNerf)
                                    {
                                        context.GameState.StunToken(token);
                                    }
                                }
                            }
                        };


                    } break;

                case CardTemplates.HorsemanOfWar:
                    {
                        Name = "Horseman of War";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Warrior;
                        Colour = ManaColour.Purple;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet(ManaColour.Purple, ManaColour.Purple, ManaColour.Purple, ManaColour.Colourless, ManaColour.Colourless);

                        Attack = 3;
                        Health = 6;

                        KeywordAbilities[KeywordAbilityNames.Bloodlust] = true;
                        KeywordAbilities[KeywordAbilityNames.Vigilance] = true;

                        Abilities = new[] { GenericCreatureOrRelicCast() };
                    }
                    break;

                case CardTemplates.HorsemanOfPestilence:
                    {
                        Name = "Horseman of Pestilence";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Warrior;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet(ManaColour.Green, ManaColour.Green, ManaColour.Green, ManaColour.Colourless, ManaColour.Colourless);

                        Attack = 3;
                        Health = 6;

                        Abilities = new[] { 
                            GenericCreatureOrRelicCast(), 
                            new TriggeredAbility{
                                BreadText = $"When {Name} enters the battlefield; all creatures your opponent controls get -2/-2.",
                                IsTriggeredBy = BattlecryTrigger,
                                Resolve = context => {
                                    var toNerf = context.GameState.AllTokens.Where(token => token.IsValid && token.IsCreature && token.Controller != context.CastingPlayer).ToArray();
                                    foreach (var token in toNerf)
                                    {
                                        token.Auras.Add(new Aura{
                                            ApplyAura = (token, _) => {
                                                token.AuraModifiers.Attack += -2;
                                                token.AuraModifiers.Health += -2;
                                            }
                                        });
                                    }
                                }
                            }
                        };
                    } break;

                case CardTemplates.GiantSeagull:
                    {
                        Name = "Giant Seagull";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Bird;
                        Colour = ManaColour.Blue;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Blue, ManaColour.Blue);

                        Attack = 2;
                        Health = 3;

                        KeywordAbilities[KeywordAbilityNames.Flying] = true;

                        Abilities = new[] { GenericCreatureOrRelicCast() };
                    } break;

                case CardTemplates.ClericMilitia:
                    {
                        Name = "Cleric Militia";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Cleric;
                        Colour = ManaColour.White;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.White);

                        Attack = 1;
                        Health = 1;

                        KeywordAbilities[KeywordAbilityNames.Protected] = true;

                        Abilities = new[] { GenericCreatureOrRelicCast() };

                    } break;

                case CardTemplates.ClericSwordmaster:
                    {
                        Name = "Cleric Swordmaster";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Cleric;
                        Colour = ManaColour.White;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.White, ManaColour.White);

                        Attack = 2;
                        Health = 3;

                        KeywordAbilities[KeywordAbilityNames.Protected] = true;

                        Abilities = new[] { GenericCreatureOrRelicCast() };
                    } break;

                case CardTemplates.ClericChampion:
                    {
                        Name = "Cleric Champion";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Cleric;
                        Colour = ManaColour.White;
                        Rarity = CardRarities.Rare;
                        CastingCost = new ManaSet(ManaColour.White, ManaColour.White, ManaColour.White, ManaColour.White);

                        Attack = 4;
                        Health = 5;

                        KeywordAbilities[KeywordAbilityNames.Protected] = true;

                        Abilities = new[] { GenericCreatureOrRelicCast() };
                    } break;

                case CardTemplates.RisenAbomination:
                    {
                        Name = "Risen Abomination";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Abomination;
                        Colour = ManaColour.Black;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Black, ManaColour.Black);

                        Attack = 2;
                        Health = 3;

                        KeywordAbilities[KeywordAbilityNames.Terrify] = true;

                        Abilities = new[] { GenericCreatureOrRelicCast() };
                    } break;

                case CardTemplates.Parthiax:
                    {
                        Name = "Parthiax, Hand of Faith";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Cleric;
                        Colour = ManaColour.White;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet(ManaColour.White, ManaColour.White, ManaColour.White, ManaColour.White, ManaColour.White, ManaColour.White, ManaColour.White);

                        Attack = 7;
                        Health = 7;

                        KeywordAbilities[KeywordAbilityNames.Protected] = true;
                        KeywordAbilities[KeywordAbilityNames.Flying] = true;
                        KeywordAbilities[KeywordAbilityNames.Vigilance] = true;

                        Abilities = new[] { GenericCreatureOrRelicCast() };
                    } break;

                case CardTemplates.Tantrum:
                {
                    Name = "Tantrum";
                    Type = CardTypes.Scroll;
                    Colour = ManaColour.Red;
                    Rarity = CardRarities.Common;
                    CastingCost = new ManaSet(ManaColour.Red, ManaColour.Colourless);

                    Abilities = new[] {
                        new Ability {
                            MoveToStackOnCast = true,
                            BreadText = "Destroy target relic.",

                            IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost),
                            MakeCastChoices = context => {
                                var payment = MakePayManaChoice(CastingCost, context, context.CastingPlayer);
                                if (payment == null) { return false; }

                                context.ChoiceHelper.Text = $"Choose a target for {Name}.";
                                context.ChoiceHelper.ShowCancel = true;
                                var target = context.ChoiceHelper.ChooseToken(token => token.IsRelic);
                                if (target == null) { return false; }

                                context.SetManaSet("manacost", payment);
                                context.SetToken("!target", target);
                                return true;
                            },
                            EnactCastChoices = context => GenericPayManaEnact(context),
                            Resolve = context =>
                            {
                                var target = context.GetToken("!target");
                                if (!target.IsValid) { return; }

                                context.GameState.MoveCard(target.TokenOf, target.TokenOf.Owner.Graveyard);
                            }
                        }
                    };
                } break;

                case CardTemplates.FuriousRuby:
                    {
                        Name = "Furious Ruby";
                        Type = CardTypes.Relic;
                        Colour = ManaColour.Colourless;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Colourless);

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),

                            new Ability {
                                BreadText = "Exhaust, Pay 1 mana of any color: Gain 1 Red mana until the start of your next turn.",
                                IsCastable = context => Location == PileLocation.Battlefield && !Token.IsExhausted && context.CastingPlayer == Token.Controller,
                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(new ManaSet(ManaColour.Colourless), context);
                                },
                                EnactCastChoices = context => {
                                    GenericPayManaEnact(context);
                                    context.GameState.ExhaustToken(Token);
                                },
                                Resolve = context =>
                                {
                                    context.GameState.GainTemporaryMana(context.CastingPlayer, ManaColour.Red, 1);
                                }
                            },

                            new Ability {
                                BreadText = $"Sacrifice {Name}: Deal 1 damage to target creature.",
                                IsCastable = context => Location == PileLocation.Battlefield && context.CastingPlayer == Token.Controller,
                                MakeCastChoices = context => {
                                    Func<Token, bool> filter = token => token.IsValid && token.IsCreature;
                                    if (context.GameState.AllTokens.Where(filter).Count() == 0) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for Fury.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(filter);
                                    if (target == null) { return false; }

                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    context.GameState.MoveCard(this, Owner.Graveyard);
                                },
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }

                                    context.GameState.DealDamage(this, target, 1);
                                }
                            }
                        };
                    } break;

                case CardTemplates.MutatedLeech:
                    {
                        Name = "Mutated Leech";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Abomination;
                        Colour = ManaColour.Purple;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Purple, ManaColour.Purple);

                        Attack = 1;
                        Health = 2;

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                        };

                        Auras = new[] {
                            new Aura {
                                BreadText = $"{Name} gets +1/+1 for each card type in graveyards.",
                                ApplyAura = (token, gameState) => {
                                    var x = new bool[Enum.GetValues(typeof(CardTypes)).Length];
                                    foreach (var card in gameState.AllGraveyards)
                                    {
                                        x[(int)card.Type] = true;
                                    }
                                    foreach (var xx in x)
                                    {
                                        if (xx)
                                        {
                                            token.AuraModifiers.Attack += 1;
                                            token.AuraModifiers.Health += 1;
                                        }
                                    }
                                }
                            }
                        };

                    } break;

                case CardTemplates.DruidOfTalAal:
                    {
                        Name = "Druid of Tal'Aal";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Guardian;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Green, ManaColour.Green, ManaColour.Colourless);

                        Attack = 3;
                        Health = 3;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility {
                                BreadText = $"When {Name} enters the battlefield; restore 3 health to target creature or player.",
                                IsTriggeredBy = BattlecryTrigger,

                                MakeCastChoices = context => {
                                    context.ChoiceHelper.Text = "Choose a target for Heal.";
                                    var target = context.ChoiceHelper.ChooseToken(token => token.IsCreature || token.IsHero);
                                    if (target == null) { return false; }

                                    context.SetToken("!target", target);
                                    return true;
                                },
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }

                                    context.GameState.RestoreHealth(this, target, 3);
                                }
                            }
                        };

                    } break;

                case CardTemplates.VolatileAmethyst:
                    {
                        Name = "Volatile Amethyst";
                        Type = CardTypes.Relic;
                        Colour = ManaColour.Colourless;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Colourless);

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),

                            new Ability {
                                BreadText = "Exhaust, Pay 1 mana of any color: Gain 1 Purple mana until the start of your next turn.",
                                IsCastable = context => Location == PileLocation.Battlefield && !Token.IsExhausted && context.CastingPlayer == Token.Controller,
                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(new ManaSet(ManaColour.Colourless), context);
                                },
                                EnactCastChoices = context => {
                                    GenericPayManaEnact(context);
                                    context.GameState.ExhaustToken(Token);
                                },
                                Resolve = context =>
                                {
                                    context.GameState.GainTemporaryMana(context.CastingPlayer, ManaColour.Purple, 1);
                                }
                            },

                            new Ability {
                                BreadText = $"Sacrifice {Name}: Give target creature Rampage.",
                                IsCastable = context => Location == PileLocation.Battlefield && context.CastingPlayer == Token.Controller,
                                MakeCastChoices = context => {
                                    Func<Token, bool> filter = token => token.IsValid && token.IsCreature;
                                    if (context.GameState.AllTokens.Where(filter).Count() == 0) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for Volatility.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(filter);
                                    if (target == null) { return false; }

                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    context.GameState.MoveCard(this, Owner.Graveyard);
                                },
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }

                                    target.Auras.Add(new Aura{
                                        ApplyAura = (token, gameState) => {
                                            token.AuraModifiers.Keywords[KeywordAbilityNames.Rampage] = true;
                                        }
                                    });
                                }
                            }


                        };
                    } break;

                case CardTemplates.UnsettlingOnyx:
                    {
                        Name = "Unsettling Onyx";
                        Type = CardTypes.Relic;
                        Colour = ManaColour.Colourless;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Colourless);

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),

                            new Ability {
                                BreadText = "Exhaust, Pay 1 mana of any color: Gain 1 Black mana until the start of your next turn.",
                                IsCastable = context => Location == PileLocation.Battlefield && !Token.IsExhausted && context.CastingPlayer == Token.Controller,
                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(new ManaSet(ManaColour.Colourless), context);
                                },
                                EnactCastChoices = context => {
                                    GenericPayManaEnact(context);
                                    context.GameState.ExhaustToken(Token);
                                },
                                Resolve = context =>
                                {
                                    context.GameState.GainTemporaryMana(context.CastingPlayer, ManaColour.Black, 1);
                                }
                            },

                            new Ability {
                                BreadText = $"Sacrifice {Name}: Give target creature -1/-1.",
                                IsCastable = context => Location == PileLocation.Battlefield && context.CastingPlayer == Token.Controller,
                                MakeCastChoices = context => {
                                    Func<Token, bool> filter = token => token.IsValid && token.IsCreature;
                                    if (context.GameState.AllTokens.Where(filter).Count() == 0) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for Unsettle.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(filter);
                                    if (target == null) { return false; }

                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    context.GameState.MoveCard(this, Owner.Graveyard);
                                },
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }

                                    target.Auras.Add(new Aura{
                                        ApplyAura = (token, gameState) => {
                                            token.AuraModifiers.Attack += -1;
                                            token.AuraModifiers.Health += -1;
                                        }
                                    });
                                }
                            }
                        };
                    } break;

                case CardTemplates.RadiantDiamond:
                    {
                        Name = "Radiant Diamond";
                        Type = CardTypes.Relic;
                        Colour = ManaColour.Colourless;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Colourless);

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),

                            new Ability {
                                BreadText = "Exhaust, Pay 1 mana of any color: Gain 1 White mana until the start of your next turn.",
                                IsCastable = context => Location == PileLocation.Battlefield && !Token.IsExhausted && context.CastingPlayer == Token.Controller,
                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(new ManaSet(ManaColour.Colourless), context);
                                },
                                EnactCastChoices = context => {
                                    GenericPayManaEnact(context);
                                    context.GameState.ExhaustToken(Token);
                                },
                                Resolve = context =>
                                {
                                    context.GameState.GainTemporaryMana(context.CastingPlayer, ManaColour.White, 1);
                                }
                            },

                            new Ability {
                                BreadText = $"Sacrifice {Name}: Give target creature +1/+1.",
                                IsCastable = context => Location == PileLocation.Battlefield && context.CastingPlayer == Token.Controller,
                                MakeCastChoices = context => {
                                    Func<Token, bool> filter = token => token.IsValid && token.IsCreature;
                                    if (context.GameState.AllTokens.Where(filter).Count() == 0) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for Radiance.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(filter);
                                    if (target == null) { return false; }

                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    context.GameState.MoveCard(this, Owner.Graveyard);
                                },
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }

                                    target.Auras.Add(new Aura{
                                        ApplyAura = (token, gameState) => {
                                            token.AuraModifiers.Attack += 1;
                                            token.AuraModifiers.Health += 1;
                                        }
                                    });
                                }
                            }
                        };
                    } break;

                case CardTemplates.VibrantEmerald:
                    {
                        Name = "Vibrant Emerald";
                        Type = CardTypes.Relic;
                        Colour = ManaColour.Colourless;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Colourless);

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),

                            new Ability {
                                BreadText = "Exhaust, Pay 1 mana of any color: Gain 1 Green mana until the start of your next turn.",
                                IsCastable = context => Location == PileLocation.Battlefield && !Token.IsExhausted && context.CastingPlayer == Token.Controller,
                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(new ManaSet(ManaColour.Colourless), context);
                                },
                                EnactCastChoices = context => {
                                    GenericPayManaEnact(context);
                                    context.GameState.ExhaustToken(Token);
                                },
                                Resolve = context =>
                                {
                                    context.GameState.GainTemporaryMana(context.CastingPlayer, ManaColour.Green, 1);
                                }
                            },

                            new Ability {
                                BreadText = $"Sacrifice {Name}: Restore 1 health to target creature.",
                                IsCastable = context => Location == PileLocation.Battlefield && context.CastingPlayer == Token.Controller,
                                MakeCastChoices = context => {
                                    Func<Token, bool> filter = token => token.IsValid && token.IsCreature;
                                    if (context.GameState.AllTokens.Where(filter).Count() == 0) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for Vibrate.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(filter);
                                    if (target == null) { return false; }

                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    context.GameState.MoveCard(this, Owner.Graveyard);
                                },
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }

                                    context.GameState.RestoreHealth(this, target, 1);
                                }
                            }
                        };
                    } break;

                case CardTemplates.HarmoniousSapphire:
                    {
                        Name = "Harmonious Sapphire";
                        Type = CardTypes.Relic;
                        Colour = ManaColour.Colourless;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Colourless);

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),

                            new Ability {
                                BreadText = "Exhaust, Pay 1 mana of any color: Gain 1 Blue mana until the start of your next turn.",
                                IsCastable = context => Location == PileLocation.Battlefield && !Token.IsExhausted && context.CastingPlayer == Token.Controller,
                                MakeCastChoices = context => {
                                    return GenericPayManaChoice(new ManaSet(ManaColour.Colourless), context);
                                },
                                EnactCastChoices = context => {
                                    GenericPayManaEnact(context);
                                    context.GameState.ExhaustToken(Token);
                                },
                                Resolve = context =>
                                {
                                    context.GameState.GainTemporaryMana(context.CastingPlayer, ManaColour.Blue, 1);
                                }
                            },

                            new Ability {
                                BreadText = $"Sacrifice {Name}: Draw a card.",
                                IsCastable = context => Location == PileLocation.Battlefield && context.CastingPlayer == Token.Controller,
                                EnactCastChoices = context => {
                                    context.GameState.MoveCard(this, Owner.Graveyard);
                                },
                                Resolve = context =>
                                {
                                    context.GameState.DrawCards(Owner, 1);
                                }
                            }
                        };
                    } break;

                case CardTemplates.AmalgamatedSlime:
                    {
                        Name = "Amalgamated Slime";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Abomination;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Green, ManaColour.Colourless, ManaColour.Colourless);

                        Attack = 2;
                        Health = 3;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility {
                                BreadText = $"When {Name} dies; summon a Green 2/1 Slime token.",
                                IsTriggeredBy = DeathrattleTrigger,
                                Resolve = context => {
                                    context.GameState.SummonToken(CardTemplates.SlimeToken1, Owner);
                                }
                            }
                        };
                    } break;

                case CardTemplates.SlimeToken1:
                    {
                        Name = "Slime";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Abomination;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Token;
                        CastingCost = new ManaSet();

                        Attack = 2;
                        Health = 1;
                    } break;

                case CardTemplates.RobotAnt:
                    {
                        Name = "Robot Ant";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Ant;
                        Colour = ManaColour.Colourless;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet();

                        Attack = 1;
                        Health = 1;

                        Abilities = new Ability[] { GenericCreatureOrRelicCast() };
                    } break;

                case CardTemplates.ViciousTaskmaster:
                    {
                        Name = "Vicious Taskmaster";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Centaur;
                        Colour = ManaColour.Red;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Red);

                        Attack = 1;
                        Health = 1;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility{
                                BreadText = $"When {Name} enters the battlefield; give another creature +2 Attack until end of turn.",
                                IsTriggeredBy = BattlecryTrigger,
                                MakeCastChoices = context => {
                                    Func<Token, bool> filter = token => token.IsValid && token.IsCreature && token != Token;

                                    if (context.GameState.AllTokens.Where(filter).Count() == 0) { return false; }

                                    context.ChoiceHelper.Text = "Choose a target for Vicious Command.";
                                    var choice = context.ChoiceHelper.ChooseToken(filter);
                                    if (choice == null) { return false; } // Should never happen but who knows.

                                    context.SetToken("!target", choice);
                                    return true;
                                },
                                Resolve = context => {
                                    var token = context.GetToken("!target");
                                    if (!token.IsValid) { return; }

                                    token.Auras.Add(new Aura{
                                        ApplyAura = (token, gameState) => {
                                            token.AuraModifiers.Attack += 2;
                                        },
                                        IsCancelledBy = EndOfTurnCancel,
                                    });
                                }
                            }
                        };
                    } break;

                case CardTemplates.Mechamancer:
                    {
                        Name = "Mechamancer";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Artificer;
                        Rarity = CardRarities.Common;
                        Colour = ManaColour.Purple;
                        CastingCost = new ManaSet(ManaColour.Purple);

                        Attack = 1;
                        Health = 2;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                        };

                        Auras = new Aura[] {
                            new Aura {
                                BreadText = $"{Name} has +2 Attack while you control a Mechanical.",
                                ApplyAura = (token, gameState) => {
                                    var giveBuff =
                                        token.Controller.Battlefield
                                        .Where(card => card.CreatureType == CreatureTypes.Mechanical && card.Token != null && card.Token.IsValid)
                                        .Count() > 0;

                                    if (giveBuff)
                                    {
                                        token.AuraModifiers.Attack += 2;
                                    }
                                }
                            }
                        };
                    } break;

                case CardTemplates.ZapperVigilante:
                    {
                        Name = "Zapper Vigilante";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Artificer;
                        Rarity = CardRarities.Common;
                        Colour = ManaColour.Purple;
                        CastingCost = new ManaSet(ManaColour.Purple);

                        Attack = 1;
                        Health = 1;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility {
                                BreadText = $"When {Name} enters the battlefield; you may deal 1 damage to another creature.",
                                IsTriggeredBy = BattlecryTrigger,
                                MakeCastChoices = context => {
                                    Func<Token, bool> filter = token => token.IsValid && token.IsCreature && token.TokenOf != this;
                                    if (context.GameState.AllTokens.Where(filter).Count() == 0) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for Electrocute.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(filter);
                                    if (target == null) { return false; }

                                    context.SetToken("!target", target);
                                    return true;
                                },
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }

                                    context.GameState.DealDamage(this, target, 1);
                                }
                            }
                        };
                    } break;

                case CardTemplates.KeeperOfCuriosities:
                    {
                        Name = "Keeper of Curiosities";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Artificer;
                        Rarity = CardRarities.Uncommon;
                        Colour = ManaColour.Blue;
                        CastingCost = new ManaSet(ManaColour.Blue, ManaColour.Blue, ManaColour.Colourless, ManaColour.Colourless);

                        Attack = 3;
                        Health = 4;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility{
                                BreadText = "Whenever you cast a Scroll or Channel; draw a card.",
                                IsTriggeredBy = YouCastScrollOrSorceryTrigger,
                                Resolve = context => {
                                    context.GameState.DrawCards(Owner, 1);
                                }
                            }
                        };
                    } break;

                case CardTemplates.BeaconKeeper:
                    {
                        Name = "Beacon Keeper";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Artificer;
                        Rarity = CardRarities.Common;
                        Colour = ManaColour.Purple;
                        CastingCost = new ManaSet(ManaColour.Purple);

                        Attack = 0;
                        Health = 2;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility{
                                BreadText = $"Whenever you cast a Scroll or Channel; give {Name} +2 Attack until end of turn.",
                                IsTriggeredBy = YouCastScrollOrSorceryTrigger,
                                MakeCastChoices = context => {
                                    context.SetToken("t", Token);
                                    return true;
                                },
                                Resolve = context => {
                                    var token = context.GetToken("t");
                                    if (!token.IsValid) { return; }

                                    token.Auras.Add(new Aura{
                                        BreadText = "+2 Attack until end of turn.",
                                        IsCancelledBy = EndOfTurnCancel,
                                        ApplyAura = (token, gameState) => { token.AuraModifiers.Attack += 2; }
                                    });
                                }
                            }
                        };
                    } break;

                case CardTemplates.Knock:
                    {
                        Name = "Knock";
                        Type = CardTypes.Scroll;
                        Colour = ManaColour.Blue;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Blue);

                        Abilities = new[] {
                            new Ability {
                                MoveToStackOnCast = true,
                                BreadText = "Stun target creature.",

                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost),
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
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }
                                    context.GameState.StunToken(target);
                                }
                            }
                        };
                    } break;

                case CardTemplates.FrenziedVine:
                    {
                        Name = "Frenzied Vine";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Plant;
                        Rarity = CardRarities.Common;
                        Colour = ManaColour.Green;
                        CastingCost = new ManaSet(ManaColour.Green, ManaColour.Colourless);

                        Attack = 2;
                        Health = 1;

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility {
                                BreadText = $"When {Name} dies; draw a card.",
                                IsTriggeredBy = ThisDiesTrigger,
                                Resolve = context => {
                                    context.GameState.DrawCards(context.CastingPlayer, 1);
                                }
                            }
                        };
                    } break;

                case CardTemplates.WaterDown:
                    {
                        Name = "Water Down";
                        Type = CardTypes.Scroll;
                        Rarity = CardRarities.Common;
                        Colour = ManaColour.Blue;
                        CastingCost = new ManaSet(ManaColour.Blue);

                        Abilities = new[] {
                            new Ability{
                                MoveToStackOnCast = true,
                                BreadText = "Move the top 3 cards of target players deck into their graveyard.\nDraw a card.",
                                IsCastable = context => InHandAndOwned(context) && ManaCostIsPayable(CastingCost),
                                MakeCastChoices = context => {
                                    var payment = MakePayManaChoice(CastingCost, context, context.CastingPlayer);
                                    if (payment == null) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for {Name}.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChoosePlayer();
                                    if (target == null) { return false; }

                                    context.SetManaSet("manacost", payment);
                                    context.SetPlayer("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => GenericPayManaEnact(context),
                                Resolve = context =>
                                {
                                    var target = context.GetPlayer("!target");

                                    for (int i = 0; i < 3; i++)
                                    {
                                        var card = target.Deck.TopCard();
                                        if (card == null) { break; }
                                        context.GameState.MoveCard(card, card.Owner.Graveyard);
			                        }

                                    context.GameState.DrawCards(context.CastingPlayer, 1);
                                }
                            }
                        };
                    } break;

                case CardTemplates.TrollRider:
                    {
                        Name = "Troll Rider";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Troll;
                        Colour = ManaColour.Red;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Red, ManaColour.Colourless);

                        Attack = 3;
                        Health = 1;
                        KeywordAbilities[KeywordAbilityNames.Rampage] = true;

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                        };
                    } break;

                case CardTemplates.HunterOfTheNight:
                    {
                        Name = "Hunter of the Night";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Vampire;
                        Colour = ManaColour.Black;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Black, ManaColour.Black, ManaColour.Colourless);

                        Attack = 2;
                        Health = 3;
                        KeywordAbilities[KeywordAbilityNames.Lifesteal] = true;
                        KeywordAbilities[KeywordAbilityNames.Flying] = true;

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                        };
                    } break;

                case CardTemplates.JunkyardInnovator:
                    {
                        Name = "Junkyard Innovator";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Human;
                        Colour = ManaColour.Purple;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Purple, ManaColour.Purple, ManaColour.Colourless, ManaColour.Colourless);

                        Attack = 2;
                        Health = 4;

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                            new Ability{
                                BreadText = $"Discard a card: Give {Name} +3 attack and rampage until end of turn.",
                                IsCastable = context => Location == PileLocation.Battlefield && Controller.Hand.Count > 0,
                                MakeCastChoices = context => {
                                    context.ChoiceHelper.ShowCancel = true;
                                    context.ChoiceHelper.Text = "Choose a card to discard.";
                                    var options = context.CastingPlayer.Hand;
                                    var choice = context.ChoiceHelper.ChooseCardFromOptions(options, card => true);
                                    if (choice == null) { return false; }

                                    context.SetToken("token", Token);
                                    context.SetCard("discard", choice);
                                    return true;
                                },
                                EnactCastChoices = context => {
                                    var discard = context.GetCard("discard");
                                    context.GameState.MoveCard(discard, discard.Owner.Graveyard);
                                },
                                Resolve = context => {
                                    var token = context.GetToken("token");
                                    if (!token.IsValid) { return; }

                                    token.Auras.Add(new Aura{
                                        IsCancelledBy = EndOfTurnCancel,
                                        ApplyAura = (token, gameState) => {
                                            token.AuraModifiers.Attack += 3;
                                            token.AuraModifiers.Keywords[KeywordAbilityNames.Rampage] = true;
                                        }
                                    });
                                }
                            }
                        };
                    } break;

                case CardTemplates.Medusa:
                    {
                        Name = "Medusa";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Demon;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet(ManaColour.Green, ManaColour.Green, ManaColour.Green, ManaColour.Colourless);

                        Attack = 3;
                        Health = 4;

                        KeywordAbilities[KeywordAbilityNames.Stoning] = true;
                        KeywordAbilities[KeywordAbilityNames.Terrify] = true;

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                        };
                    } break;

                case CardTemplates.Cockatrice:
                    {
                        Name = "Cockatrice";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Beast;
                        Colour = ManaColour.Purple;
                        Rarity = CardRarities.Rare;
                        CastingCost = new ManaSet(ManaColour.Purple, ManaColour.Colourless);

                        Attack = 1;
                        Health = 3;

                        KeywordAbilities[KeywordAbilityNames.Stoning] = true;

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                        };
                    } break;

                case CardTemplates.GardenAnt:
                    {
                        Name = "Garden Ant";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Ant;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Green);

                        Attack = 1;
                        Health = 1;


                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                        };

                        Auras = new[] {
                            new Aura{
                                BreadText = $"Has +1/+1 as long as you control another Ant.",
                                ApplyAura = (token, gameState) => {
                                    foreach (var ally in token.Controller.Battlefield)
                                    {
                                        if (ally != token.TokenOf && ally.CreatureType == CreatureTypes.Ant)
                                        {
                                            token.AuraModifiers.Attack += 1;
                                            token.AuraModifiers.Health += 1;
                                            break;
                                        }
                                    }
                                }
                            }
                        };
                    } break;

                case CardTemplates.FireAnt:
                    {
                        Name = "Fire Ant";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Ant;
                        Colour = ManaColour.Mixed;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Green, ManaColour.Red);

                        Attack = 2;
                        Health = 2;

                        var firebreathcost = new ManaSet(ManaColour.Red);

                        Abilities = new[] {
                            GenericCreatureOrRelicCast(),
                            new Ability{
                                BreadText = $"Pay 1 Red Mana: Give {Name} +1 Attack until end of turn.",
                                IsCastable = context => Location == PileLocation.Battlefield && ManaCostIsPayable(firebreathcost),
                                MakeCastChoices = context => {
                                    if (Token == null) { return false; } // Paranoia.
                                    context.SetToken("t", Token);
                                    return GenericPayManaChoice(firebreathcost, context);
                                },
                                EnactCastChoices = context => GenericPayManaEnact(context),
                                Resolve = context => {
                                    var token = context.GetToken("t");
                                    if (!token.IsValid) { return; }

                                    token.Auras.Add(new Aura{
                                        IsCancelledBy = EndOfTurnCancel,
                                        ApplyAura = (token, gameState) => {
                                            token.AuraModifiers.Attack += 1;
                                        }
                                    });
                                }
                            }
                        };
                    } break;

                case CardTemplates.SoldierAnt:
                    {
                        Name = "Soldier Ant";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Ant;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Uncommon;
                        CastingCost = new ManaSet(ManaColour.Green, ManaColour.Colourless);

                        Attack = 3;
                        Health = 1;
                        KeywordAbilities[KeywordAbilityNames.Protected] = true;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                        };
                    } break;

                case CardTemplates.AntQueen:
                    {
                        Name = "Ant Queen";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Ant;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet(ManaColour.Green, ManaColour.Green, ManaColour.Green);

                        Attack = 1;
                        Health = 4;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                            new TriggeredAbility{
                                BreadText = $"When {Name} deals damage; give all other Ants you control +1/+1.",
                                IsTriggeredBy = ThisDealsDamageTrigger,
                                Resolve = context => {
                                    foreach (var allyCard in Controller.Battlefield)
                                    {
                                        if (allyCard.Token == null) { continue; } // Paranoia.
                                        var allyToken = allyCard.Token;
                                        if (allyCard != this && allyCard.CreatureType == CreatureTypes.Ant)
                                        {
                                            allyToken.Auras.Add(new Aura
                                            {
                                                ApplyAura = (token, gameState) =>{
                                                    token.AuraModifiers.Attack += 1;
                                                    token.AuraModifiers.Health += 1;
                                                }
                                            });
                                        }
                                    }
                                }
                            }
                        };
                    } break;

                case CardTemplates.FleshEatingAnt:
                    {
                        Name = "Flesh Eating Ant";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Ant;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Common;
                        CastingCost = new ManaSet(ManaColour.Green, ManaColour.Colourless, ManaColour.Colourless);

                        Attack = 4;
                        Health = 2;
                        KeywordAbilities[KeywordAbilityNames.Lifesteal] = true;

                        Abilities = new Ability[] {
                            GenericCreatureOrRelicCast(),
                        };
                    }
                    break;

                case CardTemplates.AntToken1:
                    {
                        Name = "Ant";
                        Type = CardTypes.Creature;
                        CreatureType = CreatureTypes.Ant;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Token;
                        CastingCost = new ManaSet();

                        Attack = 1;
                        Health = 1;
                    } break;

                case CardTemplates.Nalthax:
                    {
                        IsUncollectible = true;
                        Name = "Fleshrender Nalthax";
                        Type = CardTypes.Hero;
                        Colour = ManaColour.Red;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet();

                        Health = 20;

                        var heropowercost = new ManaSet(Colour, Colour);

                        Abilities = new Ability[] {
                            new Ability{
                                BreadText = "Exhaust, Pay 2 Red Mana: Deal 1 damage to target creature or player.",
                                IsCastable = context => !Token.IsExhausted && ManaCostIsPayable(heropowercost),
                                MakeCastChoices = context => {
                                    var payment = MakePayManaChoice(heropowercost, context, context.CastingPlayer);
                                    if (payment == null) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for Spit Fire.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(token => token.IsCreature || token.IsHero);
                                    if (target == null) { return false; }

                                    context.SetManaSet("manacost", payment);
                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => GenericPayManaEnact(context),
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }
                                    context.GameState.DealDamage(this, target, 1);
                                }
                            }
                        };
                    } break;

                case CardTemplates.Taldiel:
                    {
                        IsUncollectible = true;
                        Name = "Taldiel, Hand of Light";
                        Type = CardTypes.Hero;
                        Colour = ManaColour.White;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet();

                        Health = 20;

                        var heropowercost = new ManaSet(Colour, Colour);

                        Abilities = new Ability[] {
                            new Ability{
                                BreadText = "Exhaust, Pay 2 White Mana: Give target Creature +1/+1.",
                                IsCastable = context => !Token.IsExhausted && ManaCostIsPayable(heropowercost),
                                MakeCastChoices = context => {
                                    var payment = MakePayManaChoice(heropowercost, context, context.CastingPlayer);
                                    if (payment == null) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for Bolster.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(token => token.IsCreature);
                                    if (target == null) { return false; }

                                    context.SetManaSet("manacost", payment);
                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => GenericPayManaEnact(context),
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }

                                    target.Auras.Add(new Aura{
                                        ApplyAura = (token, gameState) => {
                                            token.AuraModifiers.Attack += 1;
                                            token.AuraModifiers.Health += 1;
                                        }
                                    });
                                }
                            }
                        };
                    }
                    break;

                case CardTemplates.Kelbans:
                    {
                        IsUncollectible = true;
                        Name = "Dr Kelbans";
                        Type = CardTypes.Hero;
                        Colour = ManaColour.Purple;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet();

                        Health = 20;

                        var heropowercost = new ManaSet(Colour, Colour);

                        Abilities = new Ability[] {
                            new Ability{
                                BreadText = "Exhaust, Pay 2 Purple Mana, Pay 2 Health: Draw a card.",
                                IsCastable = context => !Token.IsExhausted && ManaCostIsPayable(heropowercost) && Token.CurrentHealth > 2,
                                MakeCastChoices = context => GenericPayManaChoice(heropowercost, context),
                                EnactCastChoices = context => {
                                    context.GameState.DealDamage(this, Token, 2);
                                    GenericPayManaEnact(context);
                                },
                                Resolve = context =>
                                {
                                    context.GameState.DrawCards(context.CastingPlayer, 1);
                                }
                            }
                        };
                    }
                    break;

                case CardTemplates.Aelthys:
                    {
                        IsUncollectible = true;
                        Name = "Aelthys Tal'Aal";
                        Type = CardTypes.Hero;
                        Colour = ManaColour.Green;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet();

                        Health = 20;

                        var heropowercost = new ManaSet(Colour, Colour);

                        Abilities = new Ability[] {
                            new Ability{
                                BreadText = "Exhaust, Pay 2 Green Mana: Restore 2 health to target creature or player.",
                                IsCastable = context => !Token.IsExhausted && ManaCostIsPayable(heropowercost),
                                MakeCastChoices = context => {
                                    var payment = MakePayManaChoice(heropowercost, context, context.CastingPlayer);
                                    if (payment == null) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for Rejuvenate.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(token => token.IsCreature || token.IsHero);
                                    if (target == null) { return false; }

                                    context.SetManaSet("manacost", payment);
                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => GenericPayManaEnact(context),
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }
                                    context.GameState.RestoreHealth(this, target, 2);
                                }
                            }
                        };
                    }
                    break;

                case CardTemplates.Eris:
                    {
                        IsUncollectible = true;
                        Name = "Arch Lich Eris";
                        Type = CardTypes.Hero;
                        Colour = ManaColour.Black;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet();

                        Health = 20;

                        var heropowercost = new ManaSet(Colour, Colour);

                        Abilities = new Ability[] {
                            new Ability{
                                BreadText = "Exhaust, Pay 2 Black Mana: Give target Creature -2 Attack.",
                                IsCastable = context => !Token.IsExhausted && ManaCostIsPayable(heropowercost),
                                MakeCastChoices = context => {
                                    var payment = MakePayManaChoice(heropowercost, context, context.CastingPlayer);
                                    if (payment == null) { return false; }

                                    context.ChoiceHelper.Text = $"Choose a target for Weaken.";
                                    context.ChoiceHelper.ShowCancel = true;
                                    var target = context.ChoiceHelper.ChooseToken(token => token.IsCreature);
                                    if (target == null) { return false; }

                                    context.SetManaSet("manacost", payment);
                                    context.SetToken("!target", target);
                                    return true;
                                },
                                EnactCastChoices = context => GenericPayManaEnact(context),
                                Resolve = context =>
                                {
                                    var target = context.GetToken("!target");
                                    if (!target.IsValid) { return; }

                                    target.Auras.Add(new Aura{
                                        ApplyAura = (token, gameState) => {
                                            token.AuraModifiers.Attack += -2;
                                        }
                                    });
                                }
                            }
                        };
                    }
                    break;

                case CardTemplates.Haltram:
                    {
                        IsUncollectible = true;
                        Name = "Master Mage Haltram";
                        Type = CardTypes.Hero;
                        Colour = ManaColour.Blue;
                        Rarity = CardRarities.Legendary;
                        CastingCost = new ManaSet();

                        Health = 20;

                        var heropowercost = new ManaSet(Colour, Colour);

                        Abilities = new Ability[] {
                            new Ability{
                                BreadText = "Exhaust, Pay 2 Blue Mana: Look at the top card of your deck. You may put that card at the bottom of your deck.",
                                IsCastable = context => !Token.IsExhausted && ManaCostIsPayable(heropowercost),
                                MakeCastChoices = context => GenericPayManaChoice(heropowercost, context),
                                EnactCastChoices = context => GenericPayManaEnact(context),
                                MakeResolveChoicesCastingPlayer = context => {
                                    var topCard = context.CastingPlayer.Deck.TopCard();
                                    context.ChoiceHelper.ShowCards(new Card[] { topCard });

                                    context.ChoiceHelper.ShowYes = true;
                                    context.ChoiceHelper.ShowNo = true;
                                    context.ChoiceHelper.Text = "Move card to bottom?";
                                    var choice = context.ChoiceHelper.ChooseOption();
                                    if (choice == OptionChoice.Yes)
                                    {
                                        context.SetCard("move", topCard);
                                    }
                                },
                                Resolve = context =>
                                {
                                    var moveCard = context.GetCard("move");
                                    if (moveCard == null) { return; }
                                    context.CastingPlayer.Deck.MoveToBottom(moveCard);
                                }
                            }
                        };
                    } break;

                case CardTemplates.Overgrowth:
                    {
                        Name = "Overgrowth";
                        Type = CardTypes.Channel;
                        Rarity = CardRarities.Common;
                        Colour = ManaColour.Green;
                        CastingCost = new ManaSet(ManaColour.Green);

                        Abilities = new Ability[] {
                            new Ability {
                                MoveToStackOnCast = true,
                                BreadText = "Permanently gain 1 Green Mana",
                                IsCastable = context => InHandAndOwned(context) && ChannelsAreCastable(context) && ManaCostIsPayable(CastingCost),
                                MakeCastChoices = context => GenericPayManaChoice(CastingCost, context),
                                EnactCastChoices = GenericPayManaEnact,
                                Resolve = context => {
                                    context.GameState.GainPermanentMana(context.CastingPlayer, ManaColour.Green);
                                }
                            }
                        };
                    } break;

                case CardTemplates.None: { return; }

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

            var breadTextBuilderShort = new StringBuilder();
            var breadTextBuilderLong = new StringBuilder();


            foreach (var keywordAbility in KeywordAbilities.GetAbilities())
            {
                breadTextBuilderShort.AppendLine(keywordAbility.ToString());

                breadTextBuilderLong.Append(keywordAbility.ToString());
                breadTextBuilderLong.Append(" (");
                breadTextBuilderLong.Append(Constants.KeywordExplanation(keywordAbility));
                breadTextBuilderLong.AppendLine(")");
            }

            foreach (var ability in Abilities)
            {
                if (ability.BreadText != null) 
                {
                    breadTextBuilderShort.AppendLine(ability.BreadText);
                    breadTextBuilderLong.AppendLine(ability.BreadText);
                }
            }
            foreach (var aura in Auras)
            {
                if (aura.BreadText != null) 
                {
                    breadTextBuilderShort.Append(aura.BreadText);
                    breadTextBuilderLong.Append(aura.BreadText);
                }
            }

            if (flavourText != null)
            {
                breadTextBuilderLong.AppendLine();
                breadTextBuilderLong.AppendLine(flavourText);
            }

            BreadText = breadTextBuilderShort.ToString();
            BreadTextLong = breadTextBuilderLong.ToString();

            if (Template >= CardTemplates.AngryGoblin && Template <= CardTemplates.Overgrowth) { CardSet = CardSets.FirstEdition; }
            // else if (Template >= CardTemplates. && Template <= CardTemplates.) { CardSet = CardSets.; }
            else { throw new Exception(); }
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

        private bool ThisDiesTrigger(Trigger trigger)
        {
            if (trigger is MoveTrigger)
            {
                var moveTrigger = trigger as MoveTrigger;

                if (moveTrigger.Card == this && 
                    moveTrigger.From.Location == PileLocation.Battlefield && 
                    moveTrigger.To.Location == PileLocation.Graveyard)
                {
                    return true;
                }
            }
            return false;
        }

        private bool YouCastScrollOrSorceryTrigger(Trigger trigger)
        {
            // Only triggers from Battlefield.
            if (Location == PileLocation.Battlefield && trigger is AbilityCastTrigger)
            {
                var abilityCastTrigger = trigger as AbilityCastTrigger;
                var cardType = abilityCastTrigger.Context.Card.Type;
                if (abilityCastTrigger.Context.Ability.MoveToStackOnCast &&
                    abilityCastTrigger.Context.CastingPlayer == Owner &&
                    (cardType == CardTypes.Scroll || cardType == CardTypes.Channel))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ThisDealsDamageTrigger(Trigger trigger)
        {
            if (trigger is DamageDoneTrigger)
            {
                var damageDoneTrigger = trigger as DamageDoneTrigger;

                if (damageDoneTrigger.Source == this && damageDoneTrigger.Target.IsHero)
                {
                    return true;
                }
            }

            return false;
        }

        private bool EndOfTurnCancel(Trigger trigger, Token token, GameState gameState)
        {
            if (trigger is GameTimeTrigger)
            {
                var gameTimeTrigger = trigger as GameTimeTrigger;

                if (gameTimeTrigger.Time == GameTime.EndOfTurn) { return true; }
            }

            return false;
        }

        private bool BattlecryTrigger(Trigger trigger)
        {
            if (trigger is MoveTrigger)
            {
                var moveTrigger = trigger as MoveTrigger;
                if (moveTrigger.Card == this && moveTrigger.To.Location == PileLocation.Battlefield)
                {
                    return true;
                }
            }

            return false;
        }

        private bool DeathrattleTrigger(Trigger trigger)
        {
            if (trigger is MoveTrigger)
            {
                var moveTrigger = trigger as MoveTrigger;
                if (moveTrigger.Card == this && moveTrigger.To.Location == PileLocation.Graveyard && moveTrigger.From.Location == PileLocation.Battlefield)
                {
                    return true;
                }
            }

            return false;
        }

        private bool AnyCreatureDies(Trigger trigger)
        {
            if (trigger is MoveTrigger)
            {
                var moveTrigger = trigger as MoveTrigger;
                if (moveTrigger.To.Location == PileLocation.Graveyard && moveTrigger.From.Location == PileLocation.Battlefield)
                {
                    return true;
                }
            }

            return false;
        }

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
            if (KeywordAbilities[KeywordAbilityNames.Reinforcement]) { return true; }
            if (context.GameState.ActivePlayer != context.CastingPlayer) { return false; }
            if (context.GameState.CastingStack.Count > 0) { return false; }

            return true;
        }

        /// <summary>
        /// Checks if Controllers current mana covers the given cost.
        /// </summary>
        private bool ManaCostIsPayable(ManaSet cost)
        {
            return Controller.CurrentMana.Covers(cost);

        }

        private Ability GenericCreatureOrRelicCast()
        {
            return new Ability
            {
                MoveToStackOnCast = true,

                IsCastable = context =>
                {
                    return InHandAndOwned(context) && ManaCostIsPayable(CastingCost) && ChannelsAreCastable(context);
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
            var payment = MakePayManaChoice(cost, context, Controller);
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
        Angel,
        Mermaid,
        Fish,
        Mechanical,
        Spirit,
        Cleric,
        Abomination,
        Demon,
        Guardian,
        Ant,
        Centaur,
        Plant,
        Troll,
        Vampire,

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
        MechanicalToken1,
        SilvervenomSpider,
        Eliminate,
        Inspiration,
        CourtInformant,
        BattlehardenedMage,
        ArcticWatchman,
        HawkToken1,
        SavingGrace,
        DeepSeaMermaid,
        Conflagrate,
        Rapture,
        Overcharge,
        ControlBoar,
        Seblastian,
        HauntedChapel,
        GhostToken1,
        PortalJumper,
        PalaceGuard,
        Darkness,
        MistFiend,
        HorsemanOfFamine,
        HorsemanOfPestilence,
        HorsemanOfWar,
        GiantSeagull,
        ClericMilitia,
        ClericSwordmaster,
        ClericChampion,
        RisenAbomination,
        Parthiax,
        Tantrum,
        FuriousRuby,
        MutatedLeech,
        DruidOfTalAal,
        RadiantDiamond,
        UnsettlingOnyx, 
        VibrantEmerald,
        HarmoniousSapphire,
        VolatileAmethyst,
        AmalgamatedSlime,
        SlimeToken1,
        RobotAnt,
        ViciousTaskmaster,
        Mechamancer,
        ZapperVigilante,
        KeeperOfCuriosities,
        BeaconKeeper,
        Knock,
        FrenziedVine,
        WaterDown,
        TrollRider,
        HunterOfTheNight,
        JunkyardInnovator,
        Medusa,
        Cockatrice,
        GardenAnt,
        FleshEatingAnt,
        AntQueen,
        FireAnt,
        AntToken1,
        SoldierAnt,
        Taldiel,
        Nalthax,
        Haltram,
        Eris,
        Aelthys,
        Kelbans,
        Overgrowth,
    }

    public enum CardSets
    {
        None, 

        FirstEdition,
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
