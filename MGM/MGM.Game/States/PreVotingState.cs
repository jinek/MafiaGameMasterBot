using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using MGM.BotFlow.Processing;
using MGM.Game.Helpers;
using MGM.Game.Models;
using MGM.Localization;

namespace MGM.Game.States
{
    [DataContract]
    public class PreVotingState : VotingStateBase
    {
        [DataMember]
        private readonly bool _secondVoting;
        [DataMember]
        private readonly Dictionary<Player, Player> _votes = new Dictionary<Player, Player>();
        [DataMember]
        private Player[] _alivePlayers;
        [DataMember]
        private string _initialBannerText;

        public PreVotingState(TimeSpan? additionalTime, GameState oldState, bool secondVoting = false)
            : base(
                GameStatus.Day, oldState,
                additionalTime != null
                    ? LocalizedStrings.PrevotingState_EarlyVotingStarted
                    : LocalizedStrings.PrevotingState_FirstVotingStarted,
                TimeSpan.FromMinutes(1) + additionalTime != null ? (TimeSpan) additionalTime : TimeSpan.Zero)
        {
            _alivePlayers = Game.GetAlivePlayers();
            _secondVoting = secondVoting;
            _initialBannerText = Banner.Text;
            Update();
        }

        public IReadOnlyDictionary<Player, Player> Votes => new ReadOnlyDictionary<Player, Player>(_votes);

        protected override void Vote(Player player, string voteKey)
        {
            Player againstPlayer;
            if (voteKey == SkipKey)
            {
                againstPlayer = null;
            }
            else
            {
                int against;
                try
                {
                    against = int.Parse(voteKey);
                }
                catch (FormatException formatException)
                {
                    throw new FormatException($"Строка {voteKey}",formatException);
                }
                againstPlayer = Game.CheckUserIsPlayer(against, LocalizedStrings.PrevotingState_UserDoesNotPlay); //Game.Players.ById(against);
                Game.CheckPlayerIsAlive(againstPlayer);
                if (Equals(player, againstPlayer))
                    throw new GameCommandException(LocalizedStrings.NightState_YouCanNotChooselYourself);
            }
            VoteInternal(player, againstPlayer);
        }

        public void VoteInternal(Player player, Player againstPlayer)
        {
            if (againstPlayer == null)
            {
                _votes.Remove(player);
            }
            else
            {
                if (_votes.ContainsKey(player))
                {
                    if (_votes[player].Id == againstPlayer.Id) return; //update does not have to be called

                    _votes[player] = againstPlayer;
                }
                else
                {
                    _votes.Add(player, againstPlayer);
                }
            }
            //Update(); не обновляем так как хитятся лимиты на вызов
            Game.Delayed(this,CheckAllVoted);
        }

        private void CheckAllVoted()
        {
            if (Game.GetAlivePlayers().Length == _votes.Count)//todo: low also we can check if someone is absolute winner even if he hasn't voted
            {
                Game.Timer(true);
            }
        }

        protected override GameState GetNewState()
        {
            var groups = _votes.GroupBy(v => v.Value).ToArray();
            var killWinner = groups.OrderByDescending(gr => gr.Count()).FirstOrDefault()?.Key;
            if (killWinner != null)
            {
                if (
                    groups.Any(
                        group =>
                            !Equals(@group.Key, killWinner) &&
                            group.Count() == groups.Single(gr => Equals(gr.Key, killWinner)).Count()))//todo:low must be simplified or at least speeded up
                    killWinner = null;//case there are more than one winner
            }

            Update(true);

            if (killWinner == null)
            {
                Game.GameStateBannerProvider.CreateBanner(LocalizedStrings.PrevotingState_PlayerWasNotSelected);
                return new NightState(this);
            }

            return new FinalVotingState(killWinner, this, _secondVoting);
        }

        public void Update(bool showResults=false)
        {
            var pars = new Dictionary<string, string>();
            string text = _initialBannerText+@"
";

            for (int i = 0; i < _alivePlayers.Length; i++)
            {
                var alivePlayer = _alivePlayers[i];
                int voteAgainstCount = 0;
                var alivePlayerId = alivePlayer.Id;
                if(showResults)
                voteAgainstCount = Votes.Count(pair => pair.Value.Id == alivePlayerId);

                text += $@"{i}. {alivePlayer.Username}";

                if(showResults) text += $@" - {voteAgainstCount}";
                text += @"
";

                try
                {
                    pars.Add(alivePlayerId.ToString(), i.ToString());
                }
                catch (ArgumentException exception)
                {
                    throw new InvalidOperationException($"Id was {alivePlayerId}",exception);
                }
            }

            pars.Add(SkipKey,LocalizedStrings.PrevotingState_ToSkip);

            if (showResults)
            {
                text += LocalizedStrings.GameStateBanner_VoteFinished;
            }

            Banner.Update(text);
            Thread.Sleep(1000);

            if (showResults)
                Banner.Terminate();
            else
                Banner.Update(pars);

            Thread.Sleep(500);

        }

        const string SkipKey="SKIP";
    }
}