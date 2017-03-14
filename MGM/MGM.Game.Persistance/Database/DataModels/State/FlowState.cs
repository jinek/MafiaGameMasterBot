namespace MGM.Game.Persistance.Database.DataModels.State
{
    public class FlowState
    {
        public int Id { get; set; }
        public int UserInChatId { get; set; }
        public UserInChat UserInChat { get; set; }

        public string FlowStateId { get; set; }
        public string Value { get; set; }

    }
}