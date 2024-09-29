using System;
using System.Collections.Generic;

namespace BusBooking.Server.Models;

public partial class BookSeat
{
    public int BookId { get; set; }

    public string? Pnr { get; set; }

    public int JourneyId { get; set; }

    public decimal TotalCost { get; set; }

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? PickupPoint { get; set; }

    public string? DropPoint { get; set; }

    public string? RazorpayOrderId { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
