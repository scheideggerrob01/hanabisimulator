using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace HanabiSimulator.Shared
{
    public static partial class Strategies
    {
        public static class Logic
        {
            //Discard priorities: 
            public static HanabiCard PreferredDiscard(HanabiGame game, HanabiPlayer player)
            {
                var discards = player.Hand.OrderByDescending(card => game.PlayedCards.ContainsCard(card))
                    .ThenBy(card => IsDangerCard(game, card))
                    .ThenByDescending(card => card.Number)
                    .ThenBy(card => player.Hand.IndexOf(card))
                    .ToArray();
                return discards[0];
            }
            public static bool IsDangerCard(HanabiGame g, HanabiCard c)
            {
                if (g.PlayedCards.ContainsCard(c))
                    return false;
                else if (c.Number == 5)
                    return true;
                else if (g.DiscardedCards.ContainsCard(c))
                {
                    if (c.Number == 1)
                    {
                        return g.DiscardedCards.Count(card => card.Color == c.Color && card.Color == c.Color) == 2;
                    }
                    else
                        return true;
                }
                else return false;

            }
            public static HanabiCard PreferredPlay(HanabiGame game, HanabiPlayer player)
            {
                var playableCards = player.Hand.Where(card => game.IsPlayable(card))
                    .OrderBy(card => card.Number)
                    //.ThenBy(card => IsDangerCard(game, card))
                    .ThenByDescending(card => player.Hand.IndexOf(card)).ToArray();
                if (playableCards.Length == 0)
                    return null;
                else return playableCards[0];
            }
        }
        public static HanabiGame RandomStrategy(List<HanabiCard> deck = null)
        {
            HanabiGame game = new HanabiGame() { Deck = deck };
            if (deck == null)
            {
                game.CreateDeck();
                game.ShuffleDeck();
            }
            game.DealCards();
            Random rng = new Random();
            while(!game.Ended)
            {
                var player = game.CurrentPlayer;
                var playcard = player.Hand[rng.Next() % player.Hand.Count];
                game.PlayCard(player, playcard);
            }
            return game;
        }
        public static HanabiGame BasicCheatingStrategy(List<HanabiCard> deck = null)
        {
            HanabiGame game = new HanabiGame() { Deck = deck };
            if (deck == null)
            {
                game.CreateDeck();
                game.ShuffleDeck();
            }
            game.DealCards();
            while(!game.Ended)
            {
                var play = Logic.PreferredPlay(game, game.CurrentPlayer);
                if (play != null)
                    game.PlayCard(game.CurrentPlayer, play);
                else
                {
                    game.DiscardCard(game.CurrentPlayer, Logic.PreferredDiscard(game, game.CurrentPlayer));
                }
                game.NextTurn();
            }
            return game;
        }
        
    }
}
