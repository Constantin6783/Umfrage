namespace PollTool.Server.Models.Requests
{
    public class DeletePollRequest:BaseRequest
    {
        public int PollID { get; set; }
    }
}
