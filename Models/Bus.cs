using System;
using System.Collections.Generic;

namespace BusBooking.Server.Models;

public partial class Bus
{
    public int Id { get; set; }

    public string BusName { get; set; } = null!;

    public string BusNumber { get; set; } = null!;

    public int? UserId { get; set; }

    public string? OwnerName { get; set; }

    public string? BusType { get; set; }

    public string? BusCompany { get; set; }

    public virtual ICollection<BusImage> BusImages { get; set; } = new List<BusImage>();

    public virtual ICollection<Deck> Decks { get; set; } = new List<Deck>();

    public virtual ICollection<Journey> Journeys { get; set; } = new List<Journey>();

    public virtual TblUser? User { get; set; }
}
