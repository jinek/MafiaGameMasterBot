using System;
using System.Runtime.Serialization;
using MGM.Game.Models;
using MGM.Localization;

namespace MGM.Game.States
{
    [DataContract]
    public class EndState : GameState
    {
        [DataMember(IsRequired = false)]
        //old games don't have this field, there was no abortion, so they are not aborted for sure
        private readonly bool _abort;

        public static TimeSpan EndStateMinMinutesTime = TimeSpan.FromSeconds(15);//todo: low this state is candidate to autostate
        public EndState(GameState oldState,bool abort=false) : base(GameStatus.Over, oldState)
        {
            _abort = abort;
            Game.GameStateBannerProvider.CreateBanner(ToString()+@"
"+Game.GetPlayerStatuses(true));

            Game.Delayed(this, () =>
            {
                Game.StatShower.ShowStat(Game);//todo: low this is not first time i wrote Game twice
            },TimeSpan.FromSeconds(5));
        }

        public sealed override string ToString()
        {
            return (Abort?LocalizedStrings.EndState_GameAborted:
                (Game.MafiaWins ? LocalizedStrings.EndState_MafiaWin : LocalizedStrings.EndState_CivilianWin))+@"
" +
                string.Format(LocalizedStrings.EndState_NextGameCanBePlayedAfter,
                    CreationTimeStamp+EndStateMinMinutesTime - DateTime.Now);
        }
        
        public bool Abort => _abort;
    }
}