using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class PasswordResetToken
{
    public int TokenId { get; set; }

    public int AccountId { get; set; }

    public string Token { get; set; }

    public DateTime ExpirationTime { get; set; }

    public bool? IsUsed { get; set; }

    public DateTime? CreatedTime { get; set; }

    public virtual Account Account { get; set; }
}
