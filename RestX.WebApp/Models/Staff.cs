using System;
using System.Collections.Generic;

namespace RestX.WebApp.Models;

public partial class Staff : Entity<Guid>
{
    public Guid? OwnerId { get; set; }

    public Guid FileId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual File File { get; set; } = null!;

    public virtual Owner? Owner { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}