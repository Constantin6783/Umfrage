using System;
using System.Collections.Generic;

namespace Umfrage.Models;

public partial class UserAnswer
{
    public long UserAnswerId { get; set; }

    public int QuestionId { get; set; }

    public int AnswerId { get; set; }
}
