using System;
using System.Collections.Generic;

namespace BusBooking.Server.Models;

public partial class Ticket
{
    public int? BookId { get; set; }

    public int TicketId { get; set; }

    public string SeatNumber { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public DateTime? ReservedAt { get; set; }

    public string Status { get; set; } = null!;

    public string? RazorpayOrderId { get; set; }

    public virtual BookSeat? Book { get; set; }
}
