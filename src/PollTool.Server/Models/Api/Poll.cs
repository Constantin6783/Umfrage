namespace PollTool.Server.Models.Api
{
    public class Poll
    {
        public int PollId { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;
        public bool DoneByUser { get; set; }
    }
}
