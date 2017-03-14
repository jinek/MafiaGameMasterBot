using MGM.Game.Models;

namespace MGM.Game.DependencyInversion
{
    public interface IGameStateBannerProvider
    {
        IGameStateBanner CreateBanner(string message);
        void PrivateMessage(User user, string text, string[] replyOptions = null);
        void ShowPhoto(string photoId, string body);
    }
}