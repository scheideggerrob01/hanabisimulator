using System;
using System.Collections.Generic;
using System.Text;

namespace HanabiSimulator.Shared
{
    public static partial class Strategies
    {
        public class SmartHanabiPlayer : HanabiPlayer
        {
            public SmartHanabiPlayer(int playerID) : base(playerID) 
            {
                
            }
            public override void Action(HanabiCard card, bool played, bool successful = true)
            {
                base.Action(card, played, successful);
            }

        }
        public static HanabiGame HintingStrategy(List<HanabiCard> deck = null)
        {
            int[,,] playerknowledge = new int[4, 4, 2];
            HanabiGame game = SetupGame(deck);
            while(!game.Ended)
            {

            }
            return game;
        }
    }
}
