using Telegram.Bot.Types;

namespace MGM.BotFlow.Steps
{
    public class PhotoStep : Step
    {
        protected override bool IsMatch(Update update)
        {
            if (update.Type != UpdateType.MessageUpdate) return false;//todo low : this is used in some places and can be moved to some base class
            if (update.Message.Type != MessageType.PhotoMessage) return false;
            return true;
        }

        protected override string GetParameter(Update update)
        {
            return update.Message.Photo[0].FileId;
        }

        protected internal override string GetId()
        {
            return "Photo";
        }
    }
}