﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardKartShared.GameState
{
    public class GameState
    {
        private int IDCounter = 1;
        private List<GameObject> GameObjects = new List<GameObject>();
        private List<Card> Cards = new List<Card>();

        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public Player ActivePlayer { get; private set; }
        public Player InactivePlayer { get; private set; }

        public GameState()
        {
            // An ID of 0 is invalid so just make lookups return null.
            GameObjects.Add(null);

            Player1 = new Player();
            AddGameObject(Player1);

            Player2 = new Player();
            AddGameObject(Player2);

            ActivePlayer = Player1;
            InactivePlayer = Player2;
        }

        public void LoadDecks(Deck deckPlayer1, Deck deckPlayer2)
        {
            var heroCard1 = CreateCard(CardTemplates.HeroTest);
            heroCard1.Owner = Player1;
            var heroToken1 = CreateToken(heroCard1);
            Player1.HeroCard = heroCard1;

            Player1.Deck.Add(
                deckPlayer1.CardTemplates.Select(template => CreateCard(template)).ToArray());
            foreach (var card in Player1.Deck)
            {
                card.Owner = Player1;
            }


            var heroCard2 = CreateCard(CardTemplates.HeroTest);
            heroCard2.Owner = Player2;
            var heroToken2 = CreateToken(heroCard2);
            Player2.HeroCard = heroCard2;

            Player2.Deck.Add(
                deckPlayer2.CardTemplates.Select(template => CreateCard(template)).ToArray());
            foreach (var card in Player2.Deck)
            {
                card.Owner = Player2;
            }
        }

        public GameObject GetByID(int gameID)
        {
            return GameObjects[gameID];
        }

        public void SwapActivePlayer()
        {
            var swap = ActivePlayer;
            ActivePlayer = InactivePlayer;
            InactivePlayer = swap;
        }

        public Player OtherPlayer(Player player)
        {
            if (player == Player1) { return Player2;}
            if (player == Player2) { return Player1;}
            throw new NotImplementedException();
        }

        #region Triggering 

        public void DrawCards(Player player, int cardCount)
        {
            for (int i = 0; i < cardCount; i++)
            {
                if (player.Deck.Count == 0) { return; }
                var drawnCard = player.Deck[player.Deck.Count - 1];
                player.Hand.Add(drawnCard);
            }
        }

        public void UseActiveAbility(AbilityCastingContext context)
        {
            throw new NotImplementedException();
        }

        public void ResetMana(Player player)
        {
            player.CurrentMana.CopyValues(player.MaxMana);
            player.NotifyOfChange();
        }

        public void GainMana(Player player, ManaColour colour)
        {
            player.MaxMana.IncrementColour(colour);
        }

        public void SpendMana(Player player, ManaSet spentMana)
        {
            player.CurrentMana.Subtract(spentMana);
            player.NotifyOfChange();
        }

        public void MoveCard(Card card, Pile pile)
        {
            card.Token = null;

            if (pile.Location == PileLocation.Battlefield)
            {
                CreateToken(card);
            }

            pile.Add(card);
        }

        public void DealDamage(Card source, Token target, int amount)
        {
            if (amount <= 0) { return; }

            target.Damage += amount;

            if (target.TokenOf.IsHero)
            {
                target.TokenOf.Owner.NotifyOfChange();
            }    
        }

        #endregion

        private Card CreateCard(CardTemplates template)
        {
            var card = new Card(template);
            AddGameObject(card);
            Cards.Add(card);

            return card;
        }

        private Token CreateToken(Card card)
        {
            var token = new Token();
            token.TokenOf = card;
            card.Token = token;
            AddGameObject(token);

            return token;
        }

        private void AddGameObject(GameObject newObject)
        {
            newObject.ID = IDCounter++;
            GameObjects.Add(newObject);
        }
    }
}
