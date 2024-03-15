using System;
using System.Collections.Generic;

namespace PollTool.Server.Models.Database;

public partial class Answer
{
    public int AnswerId { get; set; } = 0;

    public string Text { get; set; } = null!;

    public int QuestionId { get; set; } = 0;
}
