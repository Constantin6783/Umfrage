namespace PollTool.Server.Models.Requests
{
    public class CreatePollRequest:BaseRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Question> Questions { get; set; }
    }
}
