
namespace PollTool.Server.Models.Api
{
    public class Poll
    {
        public int PollId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public bool DoneByUser { get; internal set; }
        public bool OwnedByUser { get; internal set; }
        public bool HasAnswers { get; internal set; }
    }
}
