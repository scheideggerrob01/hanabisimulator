using System;
using System.Collections.Generic;
using System.Linq;
using HanabiSimulator.Shared;
using static HanabiSimulator.Shared.Strategies;

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
            Mod8Test(1000);
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
            int[] hints = { 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < trials; i++)
            {
                List<HanabiCard> deck = HanabiGame.GenerateDeck(new char[] { 'b', 'r', 'g', 'y', 'w' }, new int[] { 3, 2, 2, 2, 1 });
                deck.Shuffle();
                decks.Add(deck.ToList());
                games.Add(Strategies.Mod8Strategy(deck,false,false,4));
                foreach (Mod8HanabiPlayer p in games[i].Players)
                {
                    for (int o = 0; o < 8; o++)
                    {
                        hints[o] += p.hints[o];
                    }
                }
                scores[i] = games.Last().Score;
            }
            Console.WriteLine(scores.Average());
            Console.WriteLine((float)games.Count(g => g.Score == 25) / (float)trials);
            Console.WriteLine(games.Select(g => g.BombsUsed).Sum());
            Console.WriteLine(games.Count(g => g.BombsUsed == 4));
            Console.WriteLine(games.Select(g => g.BadClues).Sum());
            for (int i = (int)scores.Min(); i <= scores.Max(); i++)
            {
                Console.WriteLine($"{i} {scores.Count(s => s == i)}");
            }
            for (int o = 0; o < 8; o++)
            {
                Console.WriteLine($"{o}: {hints[o]}");
            }
            var q = games.IndexOf(games.OrderBy(g => g.BombsUsed).Where(g=> g.BombsUsed > 0).Last());
            Strategies.Mod8Strategy(decks[q], true);
        }
        
        static void OGMod8Test(int trials = 100)
        {
            float[] scores = new float[trials];
            List<HanabiGame> games = new List<HanabiGame>();
            List<List<HanabiCard>> decks = new List<List<HanabiCard>>();
            int[] hints = { 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < trials; i++)
            {
                List<HanabiCard> deck = HanabiGame.GenerateDeck(new char[] { 'b', 'r', 'g', 'y', 'w' }, new int[] { 3, 2, 2, 2, 1 });
                deck.Shuffle();
                decks.Add(deck.ToList());
                games.Add(Strategies.OGMod8Strategy(deck));
                foreach(Mod8HanabiPlayer p in games[i].Players)
                {
                    for(int o = 0; o < 8; o++)
                    {
                        hints[o] += p.hints[o];
                    }
                }
                scores[i] = games.Last().Score;
            }
            Console.WriteLine(scores.Average());
            Console.WriteLine(games.OrderBy(g => g.Score).First());
            Console.WriteLine((float)games.Count(g => g.Score == 25) / (float)trials);
            Console.WriteLine(games.Select(g => g.BombsUsed).Sum());
            Console.WriteLine(games.Count(g => g.BombsUsed == 4));
            Console.WriteLine(games.Select(g => g.BadClues).Sum());
            for (int i = (int)scores.Min(); i <= scores.Max(); i++)
            {
                Console.WriteLine($"{i} {scores.Count(s => s == i)}");
            }
            for(int o = 0; o < 8;o++)
            {
                Console.WriteLine($"{o}: {hints[o]}");
            }
            var q = games.IndexOf(games.OrderBy(g => g.Score).First());
            Strategies.OGMod8Strategy(decks[q], true);
        }
    }
}