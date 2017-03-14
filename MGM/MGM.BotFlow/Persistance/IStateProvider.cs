namespace MGM.BotFlow.Persistance
{
    public interface IStateProvider
    {
        IState GetStateForUserInChat(UserInChat userInChat);
    }
}