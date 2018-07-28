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
            public Mod8HanabiPlayer(int playerID) : base(playerID)
            {
                NextAction = new HanabiAction(HanabiActionType.Hint, null, -1);
                ShowPlays = false;
            }
            public bool ShowPlays { get; set; }
            public HanabiAction NextAction { get; set; }
            public void GiveClue(ref HanabiGame game)
            {
                List<HanabiAction> evaluations = new List<HanabiAction>();
                foreach (Mod8HanabiPlayer player in game.Players.Where(p => p != this))
                {
                    evaluations.Add(player.EvaluateHand(ref game));
                }
                int cluevalue = PosMod(evaluations.Select(e => e.ToMod8Value()).Sum(), 8);
                for (int i = 0; i < game.PlayerCount; i++)
                {
                    var player = game.Players[i];
                    if (player != this)
                        (game.Players[i] as Mod8HanabiPlayer).SetActionFromClue(this, ref game, cluevalue);
                }
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
            public int PlaysSeen(ref HanabiGame game)
            {
                int total = 0;
                foreach (Mod8HanabiPlayer player in game.Players.Where(p => p != this))
                {
                    total += Logic.PreferredPlay(game, player) == null ? 0 : 1;
                }
                return total;
            }
            public bool SeesCollision(ref HanabiGame game)
            {
                var nextplayer = game.NextPlayer as Mod8HanabiPlayer;
                if(nextplayer.NextAction.Type == HanabiActionType.Play)
                {
                    if(!game.IsPlayable(nextplayer.NextAction.Card))
                    {
                        return true;
                    }
                }
                return false;
            }
            public void DoTurn(ref HanabiGame game)
            {
                if (this.NextAction.Type == HanabiActionType.Hint)
                {
                    if (game.HintsRemaining == 0)
                    {
                        var discard = Hand[0];
                        game.DiscardCard(this, discard);
                    }
                    else
                    {
                        GiveClue(ref game);
                    }
                }
                else if (NextAction.Type == HanabiActionType.Play)
                {
                    if (SeesCollision(ref game) && game.HintsRemaining > 0)
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
                    if ((PlaysSeen(ref game) >= 2 || SeesCollision(ref game)) && game.HintsRemaining > 0)
                    {
                        GiveClue(ref game);
                    }
                    else
                    {
                        game.DiscardCard(this, NextAction.Card);
                        NextAction.Type = HanabiActionType.Hint;
                        NextAction.Card = null;
                        NextAction.CardIndex = -1;
                    }
                }
            }
            public override void Action(HanabiCard card, bool played, bool successful = true)
            {
                if (ShowPlays)
                    Console.WriteLine($" Player {this.ID} {(played ? "Played" : "Discarded")} {card}. {((successful && played) ? "" : "Card was not playable.")}");
            }
        }
        [Flags]
        public enum HanabiActionType { Play, Hint, Discard }
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
        public static int PosMod(int i, int m)
        {
            if (i % m >= 0)
                return i % m;
            else
                return i % m + m;
        }
        public static HanabiGame BasicMod8Strategy(List<HanabiCard> deck = null, bool printMoves = false)
        {
            HanabiGame game = new HanabiGame();
            for (int i = 0; i < game.PlayerCount; i++)
            {
                game.Players[i] = new Mod8HanabiPlayer(i) { ShowPlays = printMoves };
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
                if (printMoves)
                    Console.WriteLine(game);
            }
            return game;
        }
    }
}