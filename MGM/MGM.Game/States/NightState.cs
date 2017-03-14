using System;
using System.Linq;
using System.Runtime.Serialization;
using MGM.Game.Helpers;
using MGM.Game.Models;
using MGM.Localization;

namespace MGM.Game.States
{
    [DataContract]
    public class NightState : AutoGameState
    {
        [DataMember]
        private Player _killedPlayer;
        [DataMember]
        private bool _policeVoted;
        [DataMember]
        private Player _repairedPlayer;
        [DataMember]
        private Player _thisNightMafia;

        public NightState(GameState oldState)
            : base(GameStatus.Night, oldState, LocalizedStrings.NightState_CityGoesSleep, TimeSpan.FromMinutes(2))
        {
            SendInviteToPolice();
            SendInviteToMafia();
            SendInviteToDoctor();

            Game.Delayed(this,SendRandomPicture, TimeSpan.FromSeconds(10));

        }

        private void SendRandomPicture()
        {
            Game.StoryTeller.ShowRandomStory(Game);
        }

        public PlayerRole? Vote(User user, string against)
        {
            var role = VoteInternal(user, against);
            Game.Delayed(this,CheckCanFin);
            return role;
        }

        private static bool PlayerIsNotNullAndAlive(Game game, Player player)
        {
            return player!= null && game.IsAlive(player);
        }

        private void CheckCanFin()
        {
            bool allVoted = true;
            if (PlayerIsNotNullAndAlive(Game,Game.Doctor))
            {
                if (_repairedPlayer == null) allVoted = false;
            }

            if (PlayerIsNotNullAndAlive(Game, Game.Police))
            {
                if (!_policeVoted) allVoted = false;
            }

            if (_killedPlayer == null) allVoted = false;

            if(allVoted)Game.Timer(true);
        }

        private PlayerRole? VoteInternal(User user, string against)
        {
            var player = Game.CheckUserIsPlayerAndAlive(user);


            Player againstPlayer;
            try
            {
                againstPlayer = Game.Players.SingleOrDefault(
                    ap => string.Equals(ap.Username, against, StringComparison.CurrentCultureIgnoreCase));
            }
            catch (InvalidOperationException exception)
            {
                throw new InvalidOperationException($"{exception.Message}: {against}",exception);
            }
            if (againstPlayer == null) throw new GameCommandException(LocalizedStrings.NightState_PlayerNotFound);
            Game.CheckPlayerIsAlive(againstPlayer);

            if (Equals(player, Game.Doctor))
            {
                DoctorRepairs(againstPlayer);
                return null;
            }

            if(!Equals(player,Game.Police) && !Game.Mafias.Contains(player))throw new GameCommandException(LocalizedStrings.NightState_CiviliansSleep);

            if (Equals(player, againstPlayer)) throw new GameCommandException(LocalizedStrings.NightState_YouCanNotChooselYourself);

            if (Equals(player, Game.Police))
            {
                return PoliceWorks(againstPlayer);
            }
            if (Equals(_thisNightMafia, player))
            {
                MafiaKills(againstPlayer);
                return null;
            }
            if (Game.Mafias.Contains(player)) throw new GameCommandException(LocalizedStrings.NightState_AnotherMafiaVoting);


            throw new NotSupportedException();
        }

        private void DoctorRepairs(Player againstPlayer)
        {
            _repairedPlayer = againstPlayer;
        }

        private void MafiaKills(Player againstPlayer)
        {
            if (Game.Mafias.Contains(againstPlayer)) throw new GameCommandException(LocalizedStrings.NightState_CanNotKillMafia);
            _killedPlayer = againstPlayer;
        }

        private PlayerRole? PoliceWorks(Player againstPlayer)
        {
            if (_policeVoted) throw new GameCommandException(LocalizedStrings.NightState_OnlyOnePlayerCanBeChecked);
            _policeVoted = true;
            if (Equals(againstPlayer, Game.Doctor)) return PlayerRole.Doctor;
            if (Game.Mafias.Contains(againstPlayer)) return PlayerRole.Mafia;
            return PlayerRole.Citizen;
        }

        private void SendInviteToDoctor()
        {
            if (Game.Doctor != null)
            {
                var player = Game.Doctor;
                if (Game.IsAlive(player))
                {
                    var options =
                        Game.GetAlivePlayers().Select(pl => pl.Username).ToArray();
                    Game.GameStateBannerProvider.PrivateMessage(player.User,
                        LocalizedStrings.NightState_YouCanHeelOnePlayer, options);
                }
            }
        }

        private void SendInviteToMafia()
        {
            var availalbe = Game.Mafias.Where(Game.IsAlive).ToArray();
            
            _thisNightMafia = availalbe[Game.DayCount % availalbe.Length];//todo: low One by one if all alive, if players die - almost random

            if (_thisNightMafia == null) throw new InvalidOperationException("Check should be happened before this");

            var options =
                Game.GetAlivePlayers()
                    .Where(player => !Game.Mafias.Contains(player))
                    .Select(pl => pl.Username)
                    .ToArray();
            Game.GameStateBannerProvider.PrivateMessage(_thisNightMafia.User, LocalizedStrings.NightState_ChooseToKill, options);
        }

        private void SendInviteToPolice()
        {
            if (Game.Police != null)
            {
                var player = Game.Police;
                if (Game.IsAlive(player))
                {
                    var options =
                        Game.GetAlivePlayers().Where(pl => !Equals(pl, player)).Select(pl => pl.Username).ToArray();
                    Game.GameStateBannerProvider.PrivateMessage(player.User, LocalizedStrings.NightState_ChooseToCheck, options);
                }
            }
        }

        protected override GameState GetNewState()
        {
            if (_killedPlayer == null)
            {//if mafia has not killed anyone - kills himself
                _killedPlayer = _thisNightMafia;
            }

            if (Equals(_killedPlayer, _repairedPlayer))
            {
                _killedPlayer = null;
            }
            if (_killedPlayer != null)
            {
                Game.Dead.Add(_killedPlayer);
            }

            return CheckAndGetFinal() ?? new DayState(this,_killedPlayer);
        }
    }
}