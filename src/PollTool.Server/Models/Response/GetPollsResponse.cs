using ApiPoll = PollTool.Server.Models.Api.Poll;

namespace PollTool.Server.Models.Response
{
    public class GetPollsResponse: BaseResponse
    {
        public List<ApiPoll> Polls { get; set; }= new List<ApiPoll>();

    }
}
