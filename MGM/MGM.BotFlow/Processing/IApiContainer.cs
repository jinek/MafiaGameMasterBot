using Telegram.Bot;

namespace MGM.BotFlow.Processing
{
    public interface IApiContainer
    {
        Api Api { get; }
    }
}