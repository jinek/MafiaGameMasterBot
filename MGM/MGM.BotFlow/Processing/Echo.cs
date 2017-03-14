using System.Runtime.Serialization;

namespace MGM.BotFlow.Processing
{
    [DataContract]
    public class Echo
    {
        [DataMember]
        private readonly ApiChat _apiChat;
        [DataMember]
        private EchoOptions _echoOptions;
        [DataMember]
        private readonly int _id;

        public Echo(int id, string text, ApiChat apiChat, EchoOptions echoOptions)
        {
            _apiChat = apiChat;
            _echoOptions = echoOptions;
            _id = id;
            Text = text;
        }

        public int Id => _id;

        [DataMember]
        public string Text { get; set; }

        public Echo Update(EchoOptions echoInlineButtons)
        {
            _echoOptions = echoInlineButtons;
            return _apiChat.UpdateEchoInlineButtons(Id, echoInlineButtons);
        }

        public Echo Update(string newText)
        {
            return _apiChat.UpdateEchoText(Id, newText,_echoOptions);
        }
    }
}