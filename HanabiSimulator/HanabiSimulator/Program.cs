using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            Mod8Settings settings = new Mod8Settings()
            {
                CollisionLying = true,
                PrintMoves = false,
                EnsureProperClues = true,
                PrintInfoWithMoves = false
            };
            float[] scores = new float[trials];
            List<HanabiGame> games = new List<HanabiGame>();
            List<List<HanabiCard>> decks = new List<List<HanabiCard>>();
            int[] hints = { 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < trials; i++)
            {
                List<HanabiCard> deck = HanabiGame.GenerateDeck(new char[] { 'b', 'r', 'g', 'y', 'w' }, new int[] { 3, 2, 2, 2, 1 });
                deck.Shuffle();
                decks.Add(deck.ToList());
                games.Add(Strategies.Mod8Strategy(settings,deck,4));
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
            settings.PrintMoves = true;
            settings.PrintInfoWithMoves = true;
            var q = games.IndexOf(games.OrderBy(g => g.Score).First());
            Console.WriteLine(decks[q].ToCleanString());
            Strategies.Mod8Strategy(settings,decks[q]);
        }
        static void ParallelMod8Test(int trials = 10000)
        {
            Mod8Settings settings = new Mod8Settings()
            {
                CollisionLying = false,
                PrintMoves = false,
                EnsureProperClues = true
            };

            HanabiGame[] games = new HanabiGame[trials];
            List<HanabiCard>[] decks = new List<HanabiCard>[trials];
            int[] hints = { 0, 0, 0, 0, 0, 0, 0, 0 };
            for(int i = 0;i < trials;i++)
            {
                decks[i] = HanabiGame.GenerateDeck(HanabiGame.StdColors, HanabiGame.StdNumbers).Shuffled();
            }
            Parallel.For(0, trials, i =>
             {
                 games[i] = (Strategies.Mod8Strategy(settings, decks[i], 4));
             });
            int[] scores = games.Select(g => g.Score).ToArray();
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
            settings.PrintMoves = true;
            var q = Array.IndexOf(games,games.OrderBy(g => g.Score).First());
            Console.WriteLine(decks[q].ToCleanString());
            Strategies.Mod8Strategy(settings, decks[q]);
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