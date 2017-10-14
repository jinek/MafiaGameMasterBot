using System;
using System.Runtime.Serialization;
using MGM.BotFlow.Processing;
using MGM.Game.Models;
using MGM.Localization;

namespace MGM.Game.States
{
    [DataContract]
    public class EndState : GameState
    {
        public static TimeSpan EndStateMinMinutesTime = TimeSpan.FromSeconds(15)
            ; //todo: low this state is candidate to autostate

        [DataMember(IsRequired = false)]
        //old games don't have this field, there was no abortion, so they are not aborted for sure
        private readonly bool _abort;

        private readonly bool _bySystem;

        public EndState(GameState oldState, bool abort = false, bool bySystem = false) : base(GameStatus.Over, oldState)
        {
            if (bySystem && !abort)
                throw new ArgumentOutOfRangeException(nameof(bySystem),
                    $"{nameof(bySystem)} can be true only if {nameof(abort)} is true");
            _abort = abort;
            _bySystem = bySystem;
            Game.GameStateBannerProvider.CreateBanner(ToString() + @"
" + Game.GetPlayerStatuses(true));

            Game.Delayed(this, () =>
            {
                try
                {
                    Game.StatShower.ShowStat(Game); //todo: low this is not first time i wrote Game twice
                }
                catch (ApiChatBadRequestException)
                {
                }
            }, TimeSpan.FromSeconds(5));
        }

        public bool Abort => _abort;

        public sealed override string ToString()
        {
            return (Abort
                       ? (_bySystem
                           ? LocalizedStrings.EndState_GameAbortedBySystem
                           : LocalizedStrings.EndState_GameAborted)
                       : (Game.MafiaWins
                           ? LocalizedStrings.EndState_MafiaWin
                           : LocalizedStrings.EndState_CivilianWin)) + @"
" +
                   string.Format(LocalizedStrings.EndState_NextGameCanBePlayedAfter,
                       CreationTimeStamp + EndStateMinMinutesTime - DateTime.Now);
        }
    }
}