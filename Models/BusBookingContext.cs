using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Server.Models;

public partial class BusBookingContext : DbContext
{
    public BusBookingContext()
    {
    }

    public BusBookingContext(DbContextOptions<BusBookingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BookSeat> BookSeats { get; set; }

    public virtual DbSet<Bus> Buses { get; set; }

    public virtual DbSet<BusImage> BusImages { get; set; }

    public virtual DbSet<Deck> Decks { get; set; }

    public virtual DbSet<Journey> Journeys { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<TblRefreshtoken> TblRefreshtokens { get; set; }

    public virtual DbSet<TblRegistration> TblRegistrations { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=TEJA\\SQLEXPRESS;Database=busBooking;Integrated Security=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookSeat>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__BookSeat__490D1AE16BE20243");

            entity.HasIndex(e => e.Pnr, "UQ__BookSeat__DD37C14D58813932").IsUnique();

            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.DropPoint)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("drop_point");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.JourneyId).HasColumnName("journeyId");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.PickupPoint)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("pickup_point");
            entity.Property(e => e.Pnr)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValueSql("(right(CONVERT([varchar](10),abs(checksum(newid()))),(6)))")
                .HasColumnName("pnr");
            entity.Property(e => e.RazorpayOrderId)
                .HasMaxLength(220)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(220)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.TotalCost)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("totalCost");
        });

        modelBuilder.Entity<Bus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Buses__3214EC07DE3A45D6");

            entity.Property(e => e.BusCompany).HasMaxLength(220);
            entity.Property(e => e.BusName).HasMaxLength(100);
            entity.Property(e => e.BusNumber).HasMaxLength(50);
            entity.Property(e => e.BusType).HasMaxLength(220);
            entity.Property(e => e.OwnerName).HasMaxLength(220);

            entity.HasOne(d => d.User).WithMany(p => p.Buses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Buses__UserId__5CD6CB2B");
        });

        modelBuilder.Entity<BusImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BusImage__3214EC07FAD5E6D6");

            entity.Property(e => e.ImageUrl).HasMaxLength(500);

            entity.HasOne(d => d.Bus).WithMany(p => p.BusImages)
                .HasForeignKey(d => d.BusId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__BusImages__BusId__5FB337D6");
        });

        modelBuilder.Entity<Deck>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Decks__3214EC07B29478D9");

            entity.Property(e => e.DeckType).HasMaxLength(50);

            entity.HasOne(d => d.Bus).WithMany(p => p.Decks)
                .HasForeignKey(d => d.BusId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Decks__BusId__628FA481");
        });

        modelBuilder.Entity<Journey>(entity =>
        {
            entity.HasKey(e => e.JourneyId).HasName("PK__Journey__BBECC39FAF3C498D");

            entity.ToTable("Journey");

            entity.Property(e => e.JourneyId).HasColumnName("journeyId");
            entity.Property(e => e.ArrivalDate)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("arrival_date");
            entity.Property(e => e.ArrivalTime)
                .HasMaxLength(18)
                .IsUnicode(false)
                .HasColumnName("arrival_time");
            entity.Property(e => e.BusId).HasColumnName("busId");
            entity.Property(e => e.BusNumber)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("busNumber");
            entity.Property(e => e.DepartureDate)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("departure_date");
            entity.Property(e => e.DepartureTime)
                .HasMaxLength(18)
                .IsUnicode(false)
                .HasColumnName("departure_time");
            entity.Property(e => e.DriverName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("driver_name");
            entity.Property(e => e.DriverPhone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("driver_phone");
            entity.Property(e => e.Duration)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("duration");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Rating)
                .HasColumnType("decimal(2, 1)")
                .HasColumnName("rating");
            entity.Property(e => e.Reviews)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("reviews");
            entity.Property(e => e.RouteId).HasColumnName("route_id");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Bus).WithMany(p => p.Journeys)
                .HasForeignKey(d => d.BusId)
                .HasConstraintName("FK__Journey__busId__2B0A656D");

            entity.HasOne(d => d.Route).WithMany(p => p.Journeys)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("FK__Journey__route_i__2A164134");

            entity.HasOne(d => d.User).WithMany(p => p.Journeys)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Journey__userId__2BFE89A6");
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.RouteId).HasName("PK__Route__BAC024C7672EC159");

            entity.ToTable("Route");

            entity.Property(e => e.RouteId).HasColumnName("routeId");
            entity.Property(e => e.ArrivalCity)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("arrival_city");
            entity.Property(e => e.DepartureCity)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("departure_city");
            entity.Property(e => e.Distance)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("distance");
            entity.Property(e => e.DropStops)
                .HasMaxLength(220)
                .IsUnicode(false)
                .HasColumnName("drop_stops");
            entity.Property(e => e.Duration)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("duration");
            entity.Property(e => e.Stops)
                .IsUnicode(false)
                .HasColumnName("stops");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.User).WithMany(p => p.Routes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Route__userId__06CD04F7");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Seats__3214EC0750621392");

            entity.Property(e => e.Berth).HasMaxLength(20);
            entity.Property(e => e.Col)
                .HasMaxLength(220)
                .IsUnicode(false)
                .HasColumnName("col");
            entity.Property(e => e.Row)
                .HasMaxLength(220)
                .IsUnicode(false)
                .HasColumnName("row");
            entity.Property(e => e.SeatNumber).HasMaxLength(20);
            entity.Property(e => e.SeatSelection)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("seatSelection");
            entity.Property(e => e.SeatType).HasMaxLength(20);

            entity.HasOne(d => d.Deck).WithMany(p => p.Seats)
                .HasForeignKey(d => d.DeckId)
                .HasConstraintName("FK__Seats__DeckId__656C112C");
        });

        modelBuilder.Entity<TblRefreshtoken>(entity =>
        {
            entity.HasKey(e => e.Email);

            entity.ToTable("tbl_refreshtoken");

            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TokenId)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblRegistration>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__tbl_regi__1788CCACA3739898");

            entity.ToTable("tbl_registration");

            entity.HasIndex(e => e.Email, "UQ__tbl_regi__A9D1053420679C57").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MobileNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password).HasMaxLength(512);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("User");
            entity.Property(e => e.VerificationCode)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__tbl_user__1788CCAC433140A4");

            entity.ToTable("tbl_user");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsActive)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(512)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__tickets__3333C6101045F005");

            entity.ToTable("tickets");

            entity.Property(e => e.TicketId).HasColumnName("ticketId");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("gender");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.RazorpayOrderId)
                .HasMaxLength(220)
                .IsUnicode(false);
            entity.Property(e => e.ReservedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("reservedAt");
            entity.Property(e => e.SeatNumber)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("seatNumber");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("pending")
                .HasColumnName("status");

            entity.HasOne(d => d.Book).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK__tickets__book_id__6AEFE058");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
