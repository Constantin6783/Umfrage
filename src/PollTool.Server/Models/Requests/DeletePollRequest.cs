namespace PollTool.Server.Models.Requests
{
    public class DeletePollRequest:BaseRequest
    {
        public int PollId { get; set; }
    }
}
