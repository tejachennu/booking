using System;
using System.Collections.Generic;

namespace BusBooking.Server.Models;

public partial class Journey
{
    public int? BusId { get; set; }

    public int? RouteId { get; set; }

    public int JourneyId { get; set; }

    public int? UserId { get; set; }

    public string? DepartureDate { get; set; }

    public string? DepartureTime { get; set; }

    public string? ArrivalDate { get; set; }

    public string? ArrivalTime { get; set; }

    public string? Duration { get; set; }

    public decimal? Rating { get; set; }

    public string? Reviews { get; set; }

    public decimal? Price { get; set; }

    public string? BusNumber { get; set; }

    public string? DriverName { get; set; }

    public string? DriverPhone { get; set; }

    public virtual Bus? Bus { get; set; }

    public virtual Route? Route { get; set; }

    public virtual TblUser? User { get; set; }
}
