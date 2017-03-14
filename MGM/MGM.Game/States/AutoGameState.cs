using System;
using System.Runtime.Serialization;
using System.Threading;
using MGM.Game.DependencyInversion;
using MGM.Game.Models;
using MGM.Localization;

namespace MGM.Game.States
{
    [DataContract]
    public abstract class AutoGameState : GameState
    {
        [DataMember]
        private readonly IGameStateBanner _banner;

        [DataMember]
        private readonly TimeSpan _stateLong;

        [DataMember]
        public const int WarnBeforeEndInSecond = 20;

        public IGameStateBanner Banner => _banner;

        public TimeSpan StateLong => _stateLong;

        public TimeSpan TimeLeft => CreationTimeStamp + StateLong- DateTime.Now;

        protected internal GameState Timer(bool force)
        {
            if (force || TimeLeft <= TimeSpan.Zero)
            {
                var gameState = GetNewState();
                if (gameState == null)
                    throw new InvalidOperationException(
                        "Implementer must return new game state. This was finished and can not be continued");
                return gameState;
            }

            if (TimeLeft <= TimeSpan.FromSeconds(WarnBeforeEndInSecond))
            {
                Game.GameStateBannerProvider.CreateBanner(GetTimeLeftText(TimeLeft));
            }

            return null;
        }

        protected virtual string GetTimeLeftText(TimeSpan timeLeft)
        {
            return string.Format(LocalizedStrings.AutoGameState_XSecondsLeft, timeLeft);
        }

        protected abstract GameState GetNewState();

        private static readonly TimeSpan MaxStateLong = TimeSpan.FromMinutes(7);
        protected AutoGameState(GameStatus status, GameState oldState,string message,TimeSpan stateLong) : base(status, oldState)
        {
            if(stateLong>MaxStateLong)throw new ArgumentOutOfRangeException(nameof(stateLong),$"Long can not be greater than {MaxStateLong}");
            _stateLong = stateLong;
            _banner = Game.GameStateBannerProvider.CreateBanner(message);
            Thread.Sleep(1000);//todo: i think it's telegram issue - clients are not being updated in case update was sent right after post
        }

        protected virtual GameState CheckAndGetFinal()
        {
            if (Game.HasWinner)
            {
                return new EndState(this);
            }
            return null;
        }
    }
}