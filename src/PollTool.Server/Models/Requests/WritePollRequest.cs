using PollTool.Server.Models.Api;

namespace PollTool.Server.Models.Requests
{
    public class WritePollRequest:BaseRequest
    {
        public int PollId { get; set; } = 0;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Question> Questions{ get; set; } = new();
    }
}
