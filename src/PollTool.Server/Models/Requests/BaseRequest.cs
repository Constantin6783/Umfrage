using System.Diagnostics;

namespace PollTool.Server.Models.Requests
{
    public class BaseRequest
    {
        public string ApiKey { get; set; }

        public bool IsValid()
            => ApiKey == "ValidApiKey" || Debugger.IsAttached;
    }
}
