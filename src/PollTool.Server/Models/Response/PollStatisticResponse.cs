using PollTool.Server.Models.Api;

namespace PollTool.Server.Models.Response
{
    public class PollStatisticResponse : BaseResponse
    { 
        public string PollTitle { get; set; } = string.Empty;
        public string PollDescription { get; set; } = string.Empty;
        public List<UserQuestion> Questions { get; set; } = new();
    }
}
