using System;
using System.Collections.Generic;

namespace Umfrage.Models;

public partial class Answer
{
    public int AnswerId { get; set; }

    public string Answer1 { get; set; } = null!;

    public int QuestionId { get; set; }
}
