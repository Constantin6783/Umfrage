using System;
using System.Collections.Generic;

namespace PollTool.Server.Models.Database;

public partial class UserAnswer
{
    public long UserAnswerId { get; set; }

    public int AnswerId { get; set; }

    public int QuestionId { get; set; }

    public string AnsweredBy { get; set; } = null!;
}
