using System;
using System.Collections.Generic;
using System.Linq;
namespace HanabiSimulator.Shared
{
    public class HanabiGame
    {
        public HanabiGame()
        {
            Deck = new List<HanabiCard>();
            PlayedCards = new List<HanabiCard>();
            DiscardedCards = new List<HanabiCard>();
        }
        public List<HanabiPlayer> Players { get; set; }
        public int PlayerCount { get { return Players.Count; } }
        public int BombsUsed { get; set; }
        public int HintsRemaining { get; set; }
        public int TurnsCompleted { get; set; }
        public int TurnsRemaining { get; set; }
        public List<HanabiCard> Deck { get; set; }
        public List<HanabiCard> PlayedCards { get; set; }
        public int Score { get { return PlayedCards.Count; } }
        public List<HanabiCard> DiscardedCards { get; set; }
        private int highestCard = 0;
        /// <summary>
        /// Creates a normal deck with 5 colors and the standard 3-2-2-2-1 number distribution
        /// </summary>
        public void CreateDeck()
        {
            CreateSpecialDeck(new char[] { 'b', 'r', 'g', 'y', 'w' }, new int[]{3,2,2,1} );
        }
        public void CreateSpecialDeck(char[] colors, int[] numberDistribution)
        {
            Deck = new List<HanabiCard>();
            foreach(char c in colors)
            {
                for(int i = 0; i < numberDistribution.Length; i++)
                {
                    for(int x = 0; x < numberDistribution[i];x++)
                    {
                        Deck.Add(new HanabiCard(i + 1, c));
                    }
                }
            }
            highestCard = numberDistribution.Length;
        }
        public void NextTurn()
        {
            if (Deck.Count == 0 && TurnsRemaining > -1)
                TurnsRemaining = PlayerCount;
            else if (TurnsRemaining > -1)
                TurnsRemaining--;
            TurnsCompleted++;
        }
        public bool IsPlayable(HanabiCard card)
        {
            if (PlayedCards.Count(c => c.Color == card.Color && c.Number == c.Number) >= 1)
                return false;
            else if (PlayedCards.Count(c => c.Color == card.Color && c.Number == (c.Number - 1)) >= 1)
                return true;
            return card.Number == 1;
        }
        //Static objects for ease of calling externally
    }
    public class HanabiPlayer
    {
        public HanabiPlayer()
        {

        }
        public List<HanabiCard> Hand { get; set; }

    }
    public class HanabiCard
    {
        public HanabiCard(int num, char color)
        {
            Number = num;
            Color = color;
        }
        public int Number { get; set; }
        public char Color { get; set; }
        public override string ToString()
        {
            return Number.ToString() + Color;
        }
    }
}
