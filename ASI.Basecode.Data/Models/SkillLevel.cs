using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class SkillLevel
{
    public int Id { get; set; }

    public string SkillLevel1 { get; set; }

    public virtual ICollection<Training> Training { get; set; } = new List<Training>();
}
