namespace PollTool.Server.Models.Api
{
    public class UserQuestion
    {
        public string Question { get; set; } = string.Empty;
        public List<UserQuestionAnswer> Answers { get; set; } = new();
    }
}
