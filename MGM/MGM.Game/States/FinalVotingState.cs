using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using MGM.Game.Helpers;
using MGM.Game.Models;
using MGM.Localization;

namespace MGM.Game.States
{
    [DataContract]
    public class FinalVotingState : VotingStateBase
    {
        private const string KillVoteKey = "Kill";
        private const string LiveVoteKey = "Live";
        private const string SkipVoteKey = "Skip";

        [DataMember]
        private readonly Dictionary<Player, bool> _votes = new Dictionary<Player, bool>();
        [DataMember]
        private readonly Player _voteWinner;
        [DataMember]
        private readonly bool _secondVoting;

        public FinalVotingState(Player voteWinner, GameState oldState, bool secondVoting) : base(GameStatus.Day, oldState,
            GetMessage(voteWinner), TimeSpan.FromSeconds(30))
        {
            _voteWinner = voteWinner;
            _secondVoting = secondVoting;

            Update();
        }

        private static string GetMessage(Player voteWinner)
        {
            return string.Format(LocalizedStrings.FinalVotingState_VotingAgainst, voteWinner.Username);
        }

        protected override GameState GetNewState()
        {
            Update(true);// may be it can be transfered to votingState (base class)

            if (_votes.Count(vote => vote.Value) > _votes.Count(vote => !vote.Value))
            {
                Game.Dead.Add(_voteWinner);
                Game.GameStateBannerProvider.CreateBanner(string.Format(LocalizedStrings.FinalVotingState_Kiiled, Game.GetNameAndRole(_voteWinner)));
            }
            else
            {
                Game.GameStateBannerProvider.CreateBanner(
                    LocalizedStrings.FinalVotingState_CiviliansDecidedNotToKill);
                if (!_secondVoting) return new PreVotingState(null,this, true);
            }

            return CheckAndGetFinal() ?? new NightState(this);
        }

        protected override void Vote(Player player, string voteKey)
        {
            if(Equals(player, _voteWinner))throw new GameCommandException(LocalizedStrings.FinalVotingState_YouCanNotVote);

            var vote = voteKey == SkipVoteKey ? (bool?) null : voteKey != LiveVoteKey;

            if (vote == null)
            {
                if (!_votes.ContainsKey(player)) return;
                _votes.Remove(player);
            }
            else
            {
                if (_votes.ContainsKey(player) && _votes[player] == vote) return;
                _votes[player] = (bool) vote;
            }

            Update();

            Game.Delayed(this,CheckCanEnd);
        }

        private void CheckCanEnd()
        {
            if (_votes.Count == Game.GetAlivePlayers().Length-1) Game.Timer(true);
        }

        private void Update(bool showResults=false)
        {
            var alivePlayers = Game.GetAlivePlayers();
            var newLine = Environment.NewLine;
            var text = GetMessage(_voteWinner) + newLine;

            foreach (var player in alivePlayers)
            {
                text += $"{player.Username}";
                if(showResults) text += $" - {GetVote(player)}";
                text += newLine;
            }

            if (showResults)
                text += LocalizedStrings.GameStateBanner_VoteFinished;

            Banner.Update(text);
            Thread.Sleep(1000);
            if(!showResults)
            Banner.Update(new Dictionary<string, string>
            {
                {KillVoteKey, string.Format(LocalizedStrings.FinalVotingState_Yes, _votes.Count(v => v.Value))},
                {LiveVoteKey, string.Format(LocalizedStrings.FinalVotingState_Against, _votes.Count(v => !v.Value))},
                {SkipVoteKey, string.Format(LocalizedStrings.FinalVotingState_Skip, Game.GetAlivePlayers().Length - _votes.Count)}
            });
            else Banner.Terminate();
        }

        private string GetVote(Player player)
        {
            if (!_votes.ContainsKey(player)) return LocalizedStrings.FinalVotingState_SkipFact;
            return _votes[player] ? LocalizedStrings.FinalVotingState_YesFact : LocalizedStrings.VinalVotingState_NoFact;
        }
    }
}