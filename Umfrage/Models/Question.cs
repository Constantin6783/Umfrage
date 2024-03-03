using System;
using System.Collections.Generic;

namespace Umfrage.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public string Question1 { get; set; } = null!;

    public int UmfrageId { get; set; }

    public string QuestionType { get; set; } = null!;
}
