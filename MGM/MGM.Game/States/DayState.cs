using System;
using System.Runtime.Serialization;
using MGM.Game.Helpers;
using MGM.Game.Models;
using MGM.Localization;

namespace MGM.Game.States
{
    [DataContract]
    public class DayState : AutoGameState
    {
        public DayState(GameState oldState, Player dead = null) : base(GameStatus.Day, oldState,
            string.Format(LocalizedStrings.DayState_DayCaption, oldState.Game.DayCount+1) + @"
" +
            (dead == null
                ? LocalizedStrings.DayState_EverybodyWakeUp
                : string.Format(LocalizedStrings.DayState_EverybodyWakeUpBut, oldState.Game.GetNameAndRole(dead)))
            , TimeSpan.FromMinutes(5))
        {
            Game.DayCount++;
        }

        public void ForceVoting()
        {
            Game.Timer(true);
        }

        private PreVotingState StartVoting()
        {
            return new PreVotingState(TimeLeft>TimeSpan.Zero?TimeLeft:(TimeSpan?) null,this);//If day has time left - add it to voting
        }

        protected override GameState GetNewState()
        {
            return StartVoting();
        }
    }
}