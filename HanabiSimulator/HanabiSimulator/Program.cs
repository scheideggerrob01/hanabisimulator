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
            //var c = Strategies.Logic.PreferredDiscard(game, game.CurrentPlayer);
            //RandomStrategyTest(10000);
            //BasicCheatingStrategy(1000);
            Mod8Test(10000);
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
            Console.WriteLine(scores.Average());
            Console.WriteLine(games.OrderBy(g => g.Score).First());
            Console.WriteLine((float)games.Count(g => g.Score == 25) / (float)trials);
            for(int i = (int)scores.Min();i <= scores.Max();i++)
            {
                Console.WriteLine($"{i} {scores.Count(s => s == i)}");
            }
        }
        static void Mod8Test(int trials = 100)
        {
            float[] scores = new float[trials];
            List<HanabiGame> games = new List<HanabiGame>();
            List<List<HanabiCard>> decks = new List<List<HanabiCard>>();
            for (int i = 0; i < trials; i++)
            {
                List<HanabiCard> deck = HanabiGame.GenerateDeck(new char[] { 'b', 'r', 'g', 'y', 'w' }, new int[] { 3, 2, 2, 2, 1 });
                deck.Shuffle();
                decks.Add(deck.ToList());
                games.Add(Strategies.BasicMod8Strategy(deck));
                scores[i] = games.Last().Score;
            }
            Console.WriteLine(scores.Average());
            Console.WriteLine(games.OrderBy(g => g.Score).First());
            Console.WriteLine((float)games.Count(g => g.Score == 25) / (float)trials);
            for (int i = (int)scores.Min(); i <= scores.Max(); i++)
            {
                Console.WriteLine($"{i} {scores.Count(s => s == i)}");
            }
            var q = games.IndexOf(games.OrderBy(g => g.Score).First());
            //Strategies.BasicMod8Strategy(decks[q], true);
        }
    }
}