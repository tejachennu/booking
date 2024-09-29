using System;
using System.Collections.Generic;

namespace BusBooking.Server.Models;

public partial class Route
{
    public int RouteId { get; set; }

    public int? UserId { get; set; }

    public string? ArrivalCity { get; set; }

    public string? DepartureCity { get; set; }

    public decimal? Distance { get; set; }

    public string? Duration { get; set; }

    public string? Stops { get; set; }

    public string? DropStops { get; set; }

    public virtual ICollection<Journey> Journeys { get; set; } = new List<Journey>();

    public virtual TblUser? User { get; set; }
}
