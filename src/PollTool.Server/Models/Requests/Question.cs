namespace PollTool.Server.Models.Requests
{
    public class Question
    {
        public int QuestionId { get; set; }

        public string Title { get; set; } = null!;
        public List<Answer> Answers { get; set; }
    }
}
