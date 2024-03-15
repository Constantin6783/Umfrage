using System;
using System.Collections.Generic;

namespace PollTool.Server.Models.Database;

public partial class Question
{
    public int QuestionId { get; set; }

    public string Title { get; set; } = null!;

    public int PollId { get; set; }
}
