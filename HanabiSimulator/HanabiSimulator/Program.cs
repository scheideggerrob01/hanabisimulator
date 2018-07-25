using System;
using HanabiSimulator.Shared;
namespace HanabiSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            HanabiGame game = Strategies.RandomStrategy();
            Console.WriteLine(game);
            Console.ReadLine();
        }
    }
}
