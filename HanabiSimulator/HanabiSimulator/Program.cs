using System;
using System.Collections.Generic;
using System.Linq;
using HanabiSimulator.Shared;
namespace HanabiSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            HanabiGame game = new HanabiGame();
            game.CreateDeck();
            game.ShuffleDeck();
            game.DealCards();
            var c = Strategies.Logic.PreferredDiscard(game, game.CurrentPlayer);
            //RandomStrategyTest(10000);
            BasicCheatingStrategy(10000);
            Console.ReadLine();
        }
        static void RandomStrategyTest(int trials)
        {
            float[] scores = new float[trials];
            for(int i = 0; i < trials; i++)
            {
                scores[i] = Strategies.RandomStrategy().Score;
            }
            Console.WriteLine(scores.Average());
        }
        static void BasicCheatingStrategy(int trials)
        {
            float[] scores = new float[trials];
            List<HanabiGame> games = new List<HanabiGame>();
            for (int i = 0; i < trials; i++)
            {
                games.Add(Strategies.BasicCheatingStrategy());
                scores[i] = games.Last().Score;
            }
            Console.Write(games[10]);
            Console.WriteLine(scores.Average());
            Console.WriteLine(games.OrderBy(g => g.Score).First());
        }
    }
}
