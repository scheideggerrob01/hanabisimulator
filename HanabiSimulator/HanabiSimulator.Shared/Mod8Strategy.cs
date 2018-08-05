using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace HanabiSimulator.Shared
{
    public static partial class Strategies
    {
        public class Mod8HanabiPlayer : HanabiPlayer
        {
            public Mod8HanabiPlayer(int playerID, Mod8Settings settings = null) : base(playerID)
            {
                NextAction = new HanabiAction(HanabiActionType.Hint, null, -1);
                ShowPlays = false;
                Settings = settings ?? new Mod8Settings() { };
            }
            public int[] hints = { 0, 0, 0, 0, 0, 0, 0, 0 };
            public Mod8Settings Settings { get; set; }
            public bool ShowPlays { get; set; }
            public HanabiAction NextAction { get; set; }
            public string ToActionString()
            {
                if(NextAction.Type == HanabiActionType.Play)
                {
                    return $"Play {NextAction.Card};";
                }
                else if(NextAction.Type == HanabiActionType.Discard)
                {
                    return $"Discard {NextAction.Card}";
                }
                else return "";
            }
            public IEnumerable<HanabiCard> GetPlayableCards(HanabiGame game)
            {
                return Hand.Where(c => game.IsPlayable(c));
            }
            public bool IsValidClue(ref HanabiGame game, int clue)
            {
                var nnextplayer = game.Players[(game.PlayerTurn + 2) % game.PlayerCount];
                if (clue % 2 == 1)
                {
                    return true;
                }
                if(clue >= 4)
                {
                    if ((clue % 4) - (clue % 2) == 0)
                    {
                        var num = game.NextPlayer.Hand[0].Number;
                        if (game.NextPlayer.Hand.Count(c => c.Number == num) == 4)
                        {
                            return false;
                        }
                        else return true;

                    }
                    else
                    {
                        var num = game.NextPlayer.Hand[0].Color;
                        if (game.NextPlayer.Hand.Count(c => c.Color == num) == 4)
                        {
                            return false;
                        }
                        else return true;
                    }
                }
                else
                {
                    if ((clue % 4) - (clue % 2) == 0)
                    {
                        var num = nnextplayer.Hand[0].Number;
                        if (nnextplayer.Hand.Count(c => c.Number == num) == 4)
                        {
                            return false;
                        }
                        else return true;

                    }
                    else
                    {
                        var num = nnextplayer.Hand[0].Color;
                        if (nnextplayer.Hand.Count(c => c.Color == num) == 4)
                        {
                            return false;
                        }
                        else return true;
                    }
                }

            }
            /// <summary>
            /// Called when someone wants to give a clue on their turn. 
            /// </summary>
            /// <param name="game"></param>
            public void GiveClue(ref HanabiGame game)
            {
                bool cluegiven = false;
                List<HanabiAction> evaluations = new List<HanabiAction>();
                foreach (Mod8HanabiPlayer player in game.Players.Where(p => p != this).OrderBy(p => p.ID))
                {
                    evaluations.Add(player.EvaluateHand(ref game));
                }
                
                int cluevalue = PosMod(evaluations.Select(e => e.ToMod8Value()).Sum(), 8);
                if (!IsValidClue(ref game, cluevalue))
                {
                    game.BadClues++;
                    cluegiven = true;
                }
                List<HanabiCard> playcards = new List<HanabiCard>();
                for (int i = 0; i < game.PlayerCount; i++)
                {
                    var player = game.Players[i];
                    if (player != this)
                    {
                        (game.Players[i] as Mod8HanabiPlayer).SetActionFromClue(this, ref game, cluevalue);
                    }
                }
                hints[cluevalue]++;
                var cluedplays = evaluations.Where(a => a.Type == HanabiActionType.Play).Select(a => a.Card).ToList();
                var nextplayer = game.NextPlayer as Mod8HanabiPlayer;
                if (nextplayer.NextAction.Type == HanabiActionType.Play)
                {
                    var cluedplay = nextplayer.NextAction.Card;
                    foreach (HanabiCard card in cluedplays)
                    {
                        if (cluedplays.Count(c => c.Color == card.Color && c.Number == card.Number) >= 2 && nextplayer.NextAction.Card.Color == card.Color && nextplayer.NextAction.Card.Number == card.Number)
                        {
                            HanabiAction[] bad = { nextplayer.NextAction };
                            LieClue(ref game, nextplayer, evaluations);
                            cluegiven = true;
                            break;
                        }
                    }
                }

                if(ShowPlays && !cluegiven)
                    Console.WriteLine($"Player {ID} gave a hint.");
                //Once the hint has been decided, make sure the game uses up a hint
                game.GiveHint();
                this.NextAction = new HanabiAction(HanabiActionType.Hint, null, -1);
            }
            public void SetActionFromClue(HanabiPlayer cluer, ref HanabiGame game, int clueValue)
            {
                int seenvalue = 0;
                foreach (Mod8HanabiPlayer player in game.Players.Where(p => p != this && p != cluer))
                {
                    seenvalue += player.EvaluateHand(ref game).ToMod8Value();
                }
                int personalvalue = PosMod((clueValue - seenvalue), 8);
                this.NextAction = HanabiAction.FromMod8Value(this, personalvalue);
            }
            public HanabiAction EvaluateHand(ref HanabiGame game)
            {
                var preferredplay = Logic.PreferredPlay(game, this);
                if (preferredplay != null)
                    return new HanabiAction(HanabiActionType.Play, preferredplay, this.PositionInHand(preferredplay));
                var discard = Logic.PreferredDiscard(game, this);
                return new HanabiAction(HanabiActionType.Discard, discard, this.PositionInHand(discard));
            }
            /// <summary>
            /// Looks at every player's hand in the game except the current player and checks whether a player has a card that is playable in the
            /// current game state, and adds 1 to the total if they do.
            /// </summary>
            /// <param name="game">The game being played</param>
            /// <returns>Returns the number of players who have playable card.</returns>
            public int PlaysSeen(ref HanabiGame game)
            {
                int total = 0;
                foreach (Mod8HanabiPlayer player in game.Players.Where(p => p != this))
                {
                    total += Logic.PreferredPlay(game, player) == null ? 0 : 1;
                }
                return total;
            }
            public bool SeesCollision(ref HanabiGame game,int forwardplayers = 2)
            {
                var nextplayer = game.NextPlayer as Mod8HanabiPlayer;
                var nextnextplayer = game.Players[(game.PlayerTurn + 2) % game.PlayerCount] as Mod8HanabiPlayer;
                if(nextplayer.NextAction.Type == HanabiActionType.Play)
                {
                    if(!game.IsPlayable(nextplayer.NextAction.Card))
                    {
                        return true;
                    }
                }
                if (nextplayer.NextAction.Type == HanabiActionType.Play && nextnextplayer.NextAction.Type == HanabiActionType.Play)
                {
                    if (nextplayer.NextAction.Card.Number == nextnextplayer.NextAction.Card.Number && nextplayer.NextAction.Card.Color == nextnextplayer.NextAction.Card.Color)
                    {
                        return true;
                    }
                }
                return false;
            }
            public void DangerCardDiscarded(ref HanabiGame game)
            {
                for(int i = 0; i < game.PlayerCount;i++)
                {
                    if((game.Players[i] as Mod8HanabiPlayer).NextAction.Type == HanabiActionType.Discard)
                    {
                        (game.Players[i] as Mod8HanabiPlayer).NextAction.Type = HanabiActionType.Hint;
                        (game.Players[i] as Mod8HanabiPlayer).NextAction.Card = null;
                        (game.Players[i] as Mod8HanabiPlayer).NextAction.CardIndex = -1;
                    }
                }
            }
            public void DoTurn(ref HanabiGame game)
            {
                if (this.NextAction.Type == HanabiActionType.Hint)
                {
                    if (game.HintsRemaining == 0)
                    {
                        var discard = Hand[0];
                        game.DiscardCard(this, discard);
                        if (game.IsDangerCard(discard))
                            DangerCardDiscarded(ref game);
                    }
                    else
                    {
                        GiveClue(ref game);
                    }
                }
                else if (NextAction.Type == HanabiActionType.Play)
                {
                    if (SeesCollision(ref game) && game.HintsRemaining > 0 && game.BombsUsed >= 2)
                    {
                        GiveClue(ref game);
                    }
                    else
                    {
                        game.PlayCard(this, NextAction.Card);
                        NextAction.Type = HanabiActionType.Hint;
                        NextAction.Card = null;
                        NextAction.CardIndex = -1;
                    }
                }
                else // Must be discard
                {
                    if(PlaysSeen(ref game) >= 2 && game.HintsRemaining > 0)
                    {
                        GiveClue(ref game);
                    }
                    else if (SeesCollision(ref game) && game.HintsRemaining > 0)
                    {
                        GiveClue(ref game);
                    }
                    else
                    {
                        game.DiscardCard(this, NextAction.Card);
                        if (game.IsDangerCard(NextAction.Card))
                            DangerCardDiscarded(ref game);
                        NextAction.Type = HanabiActionType.Hint;
                        NextAction.Card = null;
                        NextAction.CardIndex = -1;
                        
                    }
                }
            }
            public void LieClue(ref HanabiGame game, Mod8HanabiPlayer player, List<HanabiAction> badactions)
            {
                var badplays = badactions.Where(a => a.Type == HanabiActionType.Play).Select(a => a.Card).ToList();
                var plays = player.GetPlayableCards(game).Where(p => badplays.Count(c => c.Number == p.Number && c.Color == p.Color) == 0).ToList();
                if(plays.Count == 0)
                {
                    var desiredclue = HanabiAction.FromMod8Value(player, player.NextAction.ToMod8Value() + 4);
                    (game.Players[player.ID] as Mod8HanabiPlayer).NextAction = desiredclue; 
                    if (ShowPlays)
                        Console.WriteLine($"Player {ID} lied and gave a hint to Discard { (game.Players[player.ID] as Mod8HanabiPlayer).NextAction.Card}");
                    return;
                }
                //var bestplay = Logic.BestPlay(game, plays);
                else
                {
                    (game.Players[player.ID] as Mod8HanabiPlayer).NextAction = new HanabiAction(HanabiActionType.Play, plays[0], player.Hand.IndexOf(plays[0]));
                    if (ShowPlays)
                        Console.WriteLine($"Player {ID} lied and gave a hint to Play { (game.Players[player.ID] as Mod8HanabiPlayer).NextAction.Card}");
                    return;
                }

            }
            /// <summary>
            /// Triggers every time the the player plays/discards a card
            /// </summary>
            /// <param name="card">The card that the action was on.</param>
            /// <param name="played">Whether or no the card was played or discarded. True = Played, False = Discarded.</param>
            /// <param name="successful">Whether the card, if played, was played successfully or used a bomb.</param>
            public override void Action(HanabiGame game,HanabiCard card, bool played, bool successful = true)
            { 
                if (ShowPlays)
                    Console.WriteLine($" Player {this.ID} {(played ? "Played" : "Discarded")} {card}. {((successful && played) ? "" : "Card was not playable.")}");
            }
        }
        [Flags]
        public enum HanabiActionType { Play, Hint, Discard }
        /// <summary>
        /// A class to describe what a HanabiPlayer will do on their turn.
        /// </summary>
        public class HanabiAction
        {
            public HanabiAction(HanabiActionType t, HanabiCard card, int index)
            {
                Type = t;
                Card = card;
                CardIndex = index;
            }
            public HanabiActionType Type { get; set; }
            public HanabiCard Card { get; set; }
            public int CardIndex { get; set; }
            public int ToMod8Value()
            {
                return Type == HanabiActionType.Play ? CardIndex : 4 + CardIndex;
            }
            public static HanabiAction FromMod8Value(HanabiPlayer player, int value)
            {
                if (value >= 4)
                    return new HanabiAction(HanabiActionType.Discard, player.Hand[PosMod(value - 4, 4)], PosMod(value - 4, 4));
                else
                    return new HanabiAction(HanabiActionType.Play, player.Hand[PosMod(value, 4)], PosMod(value, 4));
            }
        }
        /// <summary>
        /// An implementation of mod, just to make sure that the result is always positive for indexing purposes (native "%" can return negative)
        /// </summary>
        /// <param name="i">Integer to be modded</param>
        /// <param name="m">Mod</param>
        /// <returns>i % m > 0</returns>
        public static int PosMod(int i, int m)
        {
            if (i % m >= 0)
                return i % m;
            else
                return i % m + m;
        }
        /// <summary>
        /// A wrapper to launch a version of Hanabi where all players are of type 'Mod8HanabiPlayer' and use advanced logic to make plays in the game.
        /// </summary>
        /// <param name="deck"></param>
        /// <param name="printMoves"></param>
        /// <param name="printGameStatus"></param>
        /// <param name="players"></param>
        /// <returns></returns>
        public static HanabiGame Mod8Strategy(List<HanabiCard> deck = null, bool printMoves = false,bool printGameStatus = false,int players = 4)
        {
            HanabiGame game = new HanabiGame();
            game.Players = new List<HanabiPlayer>();
            for (int i = 0; i < players; i++)
            {
                game.Players.Add(new Mod8HanabiPlayer(i) { ShowPlays = printMoves });
            }
            if (deck == null)
            {
                game.CreateDeck();
                game.ShuffleDeck();
            }
            else
                game.SetDeck(deck);
            game.DealCards();
            while (!game.Ended)
            {
                (game.CurrentPlayer as Mod8HanabiPlayer).DoTurn(ref game);
                game.NextTurn();
                if (printGameStatus)
                    Console.WriteLine(game);
            }
            return game;
        }
        public class Mod8Settings
        {
            public bool PrintMoves { get; set; }
            public bool CollisionLying { get; set; }
            public bool EnsureProperClues { get; set; }
            public bool Finesse { get; set; }
            

        }
    }
}