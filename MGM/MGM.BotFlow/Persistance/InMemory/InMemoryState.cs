namespace MGM.BotFlow.Persistance.InMemory
{
    internal class InMemoryState : IState
    {
        public InMemoryState(string id, string value)
        {
            Id = id;
            Value = value;
        }

        public string Id { get; }
        public IState NextState { get; private set; }
        public string Value { get; }

        public IState AddStepState(string stepId, string parameter)
        {
            return NextState = new InMemoryState(stepId, parameter);
        }

        public void ClearForward()
        {
            NextState = null;
        }
    }
}