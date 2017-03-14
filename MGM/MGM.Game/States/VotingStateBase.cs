using System;
using System.Runtime.Serialization;
using MGM.Game.Helpers;
using MGM.Game.Models;

namespace MGM.Game.States
{
    [DataContract]
    public abstract class VotingStateBase : AutoGameState
    {
        protected VotingStateBase(GameStatus status, GameState oldState, string message, TimeSpan stateLong)
            : base(status, oldState, message, stateLong)
        {
        }

        public void Vote(User user, string voteKey)
        {
            var player = Game.CheckUserIsPlayerAndAlive(user);
            Vote(player, voteKey);
        }

        protected abstract void Vote(Player player, string voteKey);
    }
}
