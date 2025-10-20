﻿using System;
using System.Collections.Generic;

namespace RestX.WebApp.Models;

public partial class OrderStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
