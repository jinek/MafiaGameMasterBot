using Telegram.Bot.Types;

namespace MGM.BotFlow.Steps
{
    public abstract class TextInputStep : Step
    {
        protected override string GetParameter(Update update)
        {
            return update.Message.Text;
        }
    }
}