using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusBooking.Server.Models;

public partial class City
{
    [Key]
    public int Id { get; set; } 

    public string? City1 { get; set; }
}
