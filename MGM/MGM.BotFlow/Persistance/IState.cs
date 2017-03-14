namespace MGM.BotFlow.Persistance
{
    public interface IState
    {
        string Id { get; }
        IState NextState { get; }
        string Value { get; }
        IState AddStepState(string stepId, string updateValue);
        void ClearForward();
    }
}