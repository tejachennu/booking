using System;
using System.Collections.Generic;

namespace BusBooking.Server.Models;

public partial class Seat
{
    public int Id { get; set; }

    public string SeatNumber { get; set; } = null!;

    public string SeatType { get; set; } = null!;

    public string Berth { get; set; } = null!;

    public int? DeckId { get; set; }

    public string? Col { get; set; }

    public string? Row { get; set; }

    public string? SeatSelection { get; set; }

    public virtual Deck? Deck { get; set; }
}
