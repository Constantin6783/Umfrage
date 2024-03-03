using System;
using System.Collections.Generic;

namespace PollTool.Server.Models;

public partial class Answer
{
    public int AnswerId { get; set; }

    public string Text { get; set; } = null!;

    public int QuestionId { get; set; }
}
