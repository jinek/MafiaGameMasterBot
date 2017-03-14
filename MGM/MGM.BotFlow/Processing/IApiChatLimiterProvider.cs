namespace MGM.BotFlow.Processing
{
    public interface IApiChatLimiterProvider//todo: low  There are to much containers and providers, they should be one generic class with one "provide" method of type T
    {
        ApiChatLimiter GetLimiter();
    }
}