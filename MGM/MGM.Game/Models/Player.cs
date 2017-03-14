using System.Runtime.Serialization;

namespace MGM.Game.Models
{
    [DataContract]
    public class Player
    {
        [DataMember]
        private readonly User _user;

        public Player(User user)
        {
            _user = user;
        }

        public string Username => _user.Username;
        public long Id => _user.Id;

        public User User => _user;

        public override bool Equals(object obj)
        {
            var player = obj as Player;
            if (player == null) return false;
            if (ReferenceEquals(this, obj)) return true;

            return Equals(User, player.User);
        }

        public override int GetHashCode()
        {
            return User.GetHashCode();
        }
    }
}