using System;
using System.Collections.Generic;
using System.Text;

namespace HanabiSimulator.Shared
{
    public static partial class Strategies
    {
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

    }
}
