using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace HanabiSimulator.Shared
{
    public static partial class Strategies
    {
        private static HanabiGame SetupGame(List<HanabiCard> deck)
        {
            HanabiGame game = new HanabiGame() { Deck = deck };
            if (deck == null)
            {
                game.CreateDeck();
                game.ShuffleDeck();
            }
            game.DealCards();
            return game;
        }
        public static class Logic
        {
            //Discard priorities: 
            public static IOrderedEnumerable<HanabiCard> OrderedDiscards(HanabiGame game, HanabiPlayer player)
            {
                return player.Hand.OrderByDescending(card => game.PlayedCards.ContainsCard(card))
                    .ThenBy(card => IsDangerCard(game, card))
                    .ThenBy(card => player.Hand.Count(p => p.Color == card.Color && p.Number == card.Number) == 2)
                    .ThenByDescending(card => card.Number)
                    .ThenBy(card => player.Hand.IndexOf(card));
            }
            public static HanabiCard PreferredDiscard(HanabiGame game, HanabiPlayer player)
            {
                var discards = OrderedDiscards(game,player).ToList();
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
            public static List<HanabiCard> OrderedPlays(HanabiGame game, HanabiPlayer player)
            {
                return player.Hand.Where(card => game.IsPlayable(card))
                    .OrderBy(card => card.Number)
                    .ThenBy(card => IsDangerCard(game, card))
                    .ThenByDescending(card => player.Hand.IndexOf(card)).ToList();
            }
            public static HanabiCard PreferredPlay(HanabiGame game, HanabiPlayer player)
            {
                var playableCards = OrderedPlays(game, player);
                if (playableCards.Count == 0)
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
            while (!game.Ended)
            {
                var player = game.CurrentPlayer;
                var playcard = player.Hand[rng.Next() % player.Hand.Count];
                game.PlayCard(player, player.Hand.LastIndexOf(playcard));
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
            while (!game.Ended)
            {
                var play = Logic.PreferredPlay(game, game.CurrentPlayer);
                if (play != null)
                    game.PlayCard(game.CurrentPlayer, game.CurrentPlayer.Hand.IndexOf(play));
                else
                {
                    var discard = Logic.PreferredDiscard(game, game.CurrentPlayer);
                    (HanabiCard c1, HanabiCard c2) = (new HanabiCard(discard.Number - 1, discard.Color), new HanabiCard(discard.Number - 2, discard.Color));
                    if (game.Players.Select(p => p.Hand).Count(h => h.ContainsCard(c1) || h.ContainsCard(c2)) >= 1 && game.HintsRemaining > 0) //If the cards necessary to play this one are in people's hands already.
                    {
                        game.HintsRemaining--;
                    }
                    else
                    {
                        game.DiscardCard(game.CurrentPlayer, game.CurrentPlayer.Hand.IndexOf(discard));
                    }
                }
                game.NextTurn();
            }
            return game;
        }
    }
}