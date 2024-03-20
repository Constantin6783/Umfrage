using PollTool.Server.Models.Api;

namespace PollTool.Server.Models.Response
{
    public class GetPollResponse:BaseResponse
    {
        public Poll Poll { get; set; } = new();
        public List<Question> Questions { get; set; } = new();
    }
}
