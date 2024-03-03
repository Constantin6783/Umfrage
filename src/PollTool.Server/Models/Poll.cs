using System;
using System.Collections.Generic;

namespace PollTool.Server.Models;

public partial class Poll
{
    public int PollId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;
}
