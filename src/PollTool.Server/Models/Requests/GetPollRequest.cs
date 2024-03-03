namespace PollTool.Server.Models.Requests
{
    public class GetPollRequest : BaseRequest
    {
        public int PollID { get; set; }
    }
}
