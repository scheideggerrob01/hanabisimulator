using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace HanabiSimulator.Shared
{
    public static partial class Strategies
    {
        public class OGMod8HanabiPlayer : HanabiPlayer
        {
            public OGMod8HanabiPlayer(int playerID) : base(playerID)
            {
                NextAction = new HanabiAction(HanabiActionType.Hint, null, -1);
                ShowPlays = false;
            }
            public bool ShowPlays { get; set; }
            public HanabiAction NextAction { get; set; }
            public void GiveClue(ref HanabiGame game)
            {
                List<HanabiAction> evaluations = new List<HanabiAction>();
                foreach (OGMod8HanabiPlayer player in game.Players.Where(p => p != this))
                {
                    evaluations.Add(player.EvaluateHand(game));
                }
                int cluevalue = PosMod(evaluations.Select(e => e.ToMod8Value()).Sum(), 8);
                for (int i = 0; i < game.PlayerCount; i++)
                {
                    var player = game.Players[i];
                    if (player != this)
                        (game.Players[i] as OGMod8HanabiPlayer).SetActionFromClue(this, ref game, cluevalue);
                }
                game.GiveHint();
                this.NextAction = new HanabiAction(HanabiActionType.Hint, null, -1);
            }
            public void SetActionFromClue(HanabiPlayer cluer, ref HanabiGame game, int clueValue)
            {
                int seenvalue = 0;
                foreach (OGMod8HanabiPlayer player in game.Players.Where(p => p != this && p != cluer))
                {
                    seenvalue += player.EvaluateHand(game).ToMod8Value();
                }
                int personalvalue = PosMod((clueValue - seenvalue), 8);
                this.NextAction = HanabiAction.FromMod8Value(this, personalvalue);
            }
            public HanabiAction EvaluateHand(HanabiGame game)
            {
                var preferredplay = Hand.Where(c => game.IsPlayable(c))
                    .OrderBy(c => c.Number == 5)
                    .ThenBy(c => c.Number)
                    .ThenBy(c => Hand.IndexOf(c)).ToArray();
                if(preferredplay.Length != 0)
                {
                    return new HanabiAction(HanabiActionType.Play, preferredplay[0], Hand.IndexOf(preferredplay[0]));
                }
                var preferreddiscard = Hand.OrderByDescending(c => game.PlayedCards.ContainsCard(c))
                    .ThenBy(c => Logic.IsDangerCard(game, c))
                    .ThenByDescending(c => c.Number)
                    .ThenBy(c=> Hand.IndexOf(c)).ToArray();
                return new HanabiAction(HanabiActionType.Discard,preferreddiscard[0],Hand.IndexOf(preferreddiscard[0]));
            }
            
            public int PlaysSeen(ref HanabiGame game)
            {
                int total = 0;
                foreach (OGMod8HanabiPlayer player in game.Players.Where(p => p != this))
                {
                    total += Logic.PreferredPlay(game, player) == null ? 0 : 1;
                }
                return total;
            }
            public void DoTurn(ref HanabiGame game)
            {
                if(NextAction.Type == HanabiActionType.Play)
                {
                    game.PlayCard(this, NextAction.Card);
                    NextAction.Type = HanabiActionType.Hint;
                    NextAction.Card = null;
                    NextAction.CardIndex = -1;
                    CardPlayed(ref game);
                }
                else if((NextAction.Type == HanabiActionType.Discard || NextAction.Type == HanabiActionType.Hint) && game.HintsRemaining > 0)
                {
                    GiveClue(ref game);
                }
                else if( NextAction.Type == HanabiActionType.Discard && game.HintsRemaining == 0)
                {
                    game.DiscardCard(this, NextAction.Card);
                    NextAction.Type = HanabiActionType.Hint;
                    NextAction.Card = null;
                    NextAction.CardIndex = -1;
                }
                else
                {
                    game.DiscardCard(this, Hand[0]);
                    NextAction.Type = HanabiActionType.Hint;
                    NextAction.Card = null;
                    NextAction.CardIndex = -1;
                }
            }
            public override void Action(HanabiGame game, HanabiCard card, bool played, bool successful = true)
            {
                if (ShowPlays)
                    Console.WriteLine($" Player {this.ID} {(played ? "Played" : "Discarded")} {card}. {((successful && played) ? "" : "Card was not playable.")}");
            }
            public void CardPlayed(ref HanabiGame game)
            {
                if(game.BombsUsed >= 2)
                {
                    for(int i = 0; i < game.PlayerCount;i++)
                    {
                        if((game.Players[i] as OGMod8HanabiPlayer).NextAction.Type == HanabiActionType.Play)
                        {
                            (game.Players[i] as OGMod8HanabiPlayer).NextAction.Type = HanabiActionType.Hint;
                            (game.Players[i] as OGMod8HanabiPlayer).NextAction.Card = null;
                            (game.Players[i] as OGMod8HanabiPlayer).NextAction.CardIndex = -1;
                        }
                    }
                }
            }
        }
        
        public static HanabiGame OGMod8Strategy(List<HanabiCard> deck = null, bool printMoves = false)
        {
            HanabiGame game = new HanabiGame();
            for (int i = 0; i < game.PlayerCount; i++)
            {
                game.Players[i] = new OGMod8HanabiPlayer(i) { ShowPlays = printMoves };
            }
            if (deck == null)
            {
                game.CreateDeck();
                game.ShuffleDeck();
            }
            else
                game.Deck = deck.ToList();
            game.DealCards();
            while (!game.Ended)
            {
                (game.CurrentPlayer as OGMod8HanabiPlayer).DoTurn(ref game);
                game.NextTurn();
                if (printMoves)
                    Console.WriteLine(game);
            }
            return game;
        }
    }
}