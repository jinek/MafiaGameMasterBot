using System.Runtime.Serialization;

namespace MGM.Game.Models
{
    [DataContract]
    public class User
    {
        [DataMember]
        private readonly string _username;
        [DataMember]
        private readonly long _id;

        public User(long id, string username)
        {
            _id = id;
            _username = "@"+username;
        }

        public string Username => _username;

        public long Id => _id;

        public override bool Equals(object obj)
        {
            var user = obj as User;
            if (user == null) return false;

            return user.Id == Id && user.Username == Username;
        }

        public override int GetHashCode()
        {
            return (int)Id^Username.GetHashCode();
        }
    }
}