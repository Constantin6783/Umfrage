using PollTool.Server.Models.Api;

namespace PollTool.Server.Models.Requests
{
    public class ProcessPollRequest:BaseRequest
    {
        public int PollId { get; set; }
        public List<AnsweredQuestion> Answereds { get; set; } = new List<AnsweredQuestion>();
    }
}
