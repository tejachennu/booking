using System;
using System.Collections.Generic;

namespace BusBooking.Server.Models;

public partial class Deck
{
    public int Id { get; set; }

    public string DeckType { get; set; } = null!;

    public int? BusId { get; set; }

    public virtual Bus? Bus { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
