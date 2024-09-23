using System;
using System.Collections.Generic;

namespace BusBooking.Server.Models;

public partial class BusImage
{
    public int Id { get; set; }

    public int? BusId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public virtual Bus? Bus { get; set; }
}
