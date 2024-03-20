namespace PollTool.Server.Models.Requests
{
    public class GetPollRequest : BaseRequest
    {
        public int PollId { get; set; }
    }
}
