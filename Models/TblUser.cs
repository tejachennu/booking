using System;
using System.Collections.Generic;

namespace BusBooking.Server.Models;

public partial class TblUser
{
    public long? MobileNumber { get; set; }

    public string? Name { get; set; }

    public string Password { get; set; } = null!;

    public string? Email { get; set; }

    public string? Role { get; set; }

    public string? IsActive { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Bus> Buses { get; set; } = new List<Bus>();

    public virtual ICollection<Journey> Journeys { get; set; } = new List<Journey>();

    public virtual ICollection<Route> Routes { get; set; } = new List<Route>();
}
