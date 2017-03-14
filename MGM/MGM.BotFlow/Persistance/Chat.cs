using System.Runtime.Serialization;

namespace MGM.BotFlow.Persistance
{
    [DataContract]
    public struct Chat
    {
        [DataMember]
        private readonly string _title;
        [DataMember]
        private readonly long _id;

        public Chat(long id, string title)
        {
            _id = id;
            _title = title;
        }

        public string Title => _title;

        public long Id => _id;
    }
}