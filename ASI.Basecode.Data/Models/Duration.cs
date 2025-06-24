using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class Duration
{
    public int Id { get; set; }

    public string Duration1 { get; set; }

    public virtual ICollection<Training> Training { get; set; } = new List<Training>();
}
