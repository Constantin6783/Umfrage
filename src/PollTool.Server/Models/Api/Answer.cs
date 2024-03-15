namespace PollTool.Server.Models.Api
{
    public class Answer
    {
        public string Text { get; set; } = null!;
        public bool Selected { get; set; }
        public int AnswerID { get; set; }
    }
}
