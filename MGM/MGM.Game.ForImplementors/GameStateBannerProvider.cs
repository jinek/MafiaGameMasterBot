using System.Runtime.Serialization;
using MGM.BotFlow.Processing;
using MGM.Game.DependencyInversion;
using MGM.Game.Models;

namespace MGM.Game.ForImplementors
{
    [DataContract]
    public class GameStateBannerProvider : IGameStateBannerProvider
    {
        [DataMember]
        private readonly ApiChat _apiChat;

        public GameStateBannerProvider(ApiChat apiChat)
        {
            _apiChat = apiChat;
        }

        public IGameStateBanner CreateBanner(string message)
        {
            var echo = _apiChat.Echo(message, 0, null);
            return new GameStateBanner(echo);
        }

        public void PrivateMessage(User user, string text, string[] replyOptions = null)
        {
            _apiChat.PrivateEcho(user.Id, text, replyOptions==null?EchoOptions.SimpleEcho() : EchoOptions.EchoReplyButtons(replyOptions));
        }

        public void ShowPhoto(string photoId, string body)
        {
            _apiChat.PhotoEcho(body,photoId);
        }
    }
}