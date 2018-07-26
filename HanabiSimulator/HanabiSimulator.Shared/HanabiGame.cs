using System;
using System.Collections.Generic;
using System.Linq;

namespace HanabiSimulator.Shared
{
    public class HanabiGame
    {
        public HanabiGame(int players = 4)
        {
            Deck = new List<HanabiCard>();
            PlayedCards = new List<HanabiCard>();
            DiscardedCards = new List<HanabiCard>();
            Players = new List<HanabiPlayer>();
            PlayerTurn = 0;
            BombsUsed = 0;
            HintsRemaining = 8;
            TurnsCompleted = 0;
            TurnsRemaining = -1;
            DeckEmpty = false;
            Ended = false;
            KeepLog = false;
            CreatePlayers(players);
        }
        public List<HanabiPlayer> Players { get; set; }
        public HanabiPlayer CurrentPlayer { get { return Players[PlayerTurn]; } }
        public bool Ended { get; set; }
        public bool KeepLog { get; set; }
        public int PlayerTurn { get; set; }
        public int PlayerCount { get { return Players.Count; } }
        public int BombsUsed { get; set; }
        public int HintsRemaining { get; set; }
        public int TurnsCompleted { get; set; }
        public int TurnsRemaining { get; set; }
        public List<HanabiCard> Deck { get; set; }
        public List<HanabiCard> PlayedCards { get; set; }
        public int Score { get { return PlayedCards.Count; } }
        public bool DeckEmpty { get; set; }
        public List<HanabiCard> DiscardedCards { get; set; }
        private int highestCard = 0;
        /// <summary>
        /// Creates a normal deck with 5 colors and the standard 3-2-2-2-1 number distribution
        /// </summary>
        public void CreateDeck()
        {
            CreateSpecialDeck(new char[] { 'b', 'r', 'g', 'y', 'w' }, new int[]{3,2,2,2,1} );
        }
        public static List<HanabiCard> GenerateDeck(char[] colors, int[] numberDistribution)
        {
            var deck = new List<HanabiCard>();
            foreach (char c in colors)
            {
                for (int i = 0; i < numberDistribution.Length; i++)
                {
                    for (int x = 0; x < numberDistribution[i]; x++)
                    {
                        deck.Add(new HanabiCard(i + 1, c));
                    }
                }
            }
            return deck;
        }
        public void CreateSpecialDeck(char[] colors, int[] numberDistribution)
        {
            Deck = GenerateDeck(colors, numberDistribution);
            highestCard = numberDistribution.Length;
        }
        public void ShuffleDeck()
        {
            Deck.Shuffle();
        }
        public void CreatePlayers(int number)
        {
            for (int i = 0;i < number;i++)
            {
                Players.Add(new HanabiPlayer(i));
            }
        }
        public void DealCards(int number = 4)
        {
            for(int i = 0; i < number;i++)
            {
                for(int x = 0; x < PlayerCount;x++)
                {
                    Players[x].Hand.Add(Deck[0]);
                    Deck.RemoveAt(0);
                }
            }
        }
        public void NextTurn()
        {
            if (Deck.Count == 0 && TurnsRemaining == -1)
                TurnsRemaining = PlayerCount;
            else if (TurnsRemaining > -1)
                TurnsRemaining--;
            if (TurnsRemaining == 0)
                Ended = true;
            TurnsCompleted++;
            PlayerTurn = (PlayerTurn + 1) % PlayerCount;
        }
        public bool IsPlayable(HanabiCard card)
        {
            if (PlayedCards.Count(c => c.Color == card.Color && c.Number == card.Number) >= 1)
                return false;
            else if (PlayedCards.Count(c => c.Color == card.Color && c.Number == (card.Number - 1)) >= 1)
                return true;
            return card.Number == 1;
        }
        public int GetPlayerID(HanabiPlayer player)
        {
            return player.ID;
        }
        /// <summary>
        /// Plays a card from the given player's hand. 
        /// </summary>
        /// <param name="player">The player playing the card. Player's hand must con</param>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool PlayCard(HanabiPlayer player,HanabiCard card)
        {
            Players[GetPlayerID(player)].Hand.Remove(card);
            DrawCard(player);
            if (IsPlayable(card))
            {
                PlayedCards.Add(card);
                if (card.Number == highestCard && HintsRemaining < 8)
                    HintsRemaining++;
                if(Score == 25)
                {
                    Ended = true;
                }
                return true;
            }
            else
            {
                BombsUsed++;
                if(BombsUsed >= 4)
                {
                    Ended = true;
                }
                return false;
            }
        }
        public void DiscardCard(HanabiPlayer player, HanabiCard card)
        {
            Players[player.ID].Hand.Remove(card);
            DiscardedCards.Add(card);
            DrawCard(player);
            if (HintsRemaining < 8)
            {
                HintsRemaining++;
            }
        }
        public void DrawCard(HanabiPlayer p)
        {
            if (Deck.Count != 0)
            {
                Players[GetPlayerID(p)].Hand.Add(Deck[0]);
                Deck.RemoveAt(0);
            }
            if (!DeckEmpty && Deck.Count == 0)
            {
                DeckEmpty = true;
            }
        }
        public string ToPlayerHandString()
        {
            string s = "";
            for(int i = 0; i < PlayerCount;i++)
            {
                s += $"Player {i}: {Players[i].Hand.ToCleanString()}\n";
            }
            return s;
        }
        public string ToPlayedDiscardedString()
        {
            return $"Played({Score}): {PlayedCards.ToCleanString()} \nDiscarded({DiscardedCards.Count}): {DiscardedCards.ToCleanString()}";
        }
        public override string ToString()
        {
            string s = "";
            s += $"Deck({Deck.Count}): " + Deck.ToCleanString() + '\n';
            s += ToPlayedDiscardedString() + '\n';
            s += ToPlayerHandString() + '\n';
            return s;
        }
    }
    public class HanabiPlayer
    {
        public HanabiPlayer(int playerID)
        {
            Hand = new List<HanabiCard>();
            ID = playerID;
        }
        public List<HanabiCard> Hand { get; set; }
        public int ID { get; set; }
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
    public static class HanabiExtensions
    {
        public static string ToCleanString(this List<HanabiCard> cards)
        {
            string s = "";
            foreach(HanabiCard c in cards)
            {
                s += c.ToString();
            }
            return s;
        }
        public static bool ContainsCard(this List<HanabiCard> cards,HanabiCard card)
        {
            foreach(HanabiCard c in cards)
            {
                if (c.Color == card.Color && c.Number == card.Number)
                    return true;
            }
            return false;
        }
        // Shuffle taken from https://stackoverflow.com/questions/273313/randomize-a-listt
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
