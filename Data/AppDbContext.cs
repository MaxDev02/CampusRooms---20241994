using CampusRooms.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusRooms.Data
{
    // The app's door to the database. One DbSet per table, plus the Identity user tables.
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Building> Buildings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One building has many rooms; each room is in one building
            modelBuilder.Entity<Room>()
                .HasOne(room => room.Building)
                .WithMany(building => building.Rooms)
                .HasForeignKey(room => room.BuildingId);

            modelBuilder.Entity<Building>().HasData(
                new Building { Id = 1, Name = "Main Block" },
                new Building { Id = 2, Name = "Science Block" },
                new Building { Id = 3, Name = "Library" },
                new Building { Id = 4, Name = "Sports Complex" }
            );

            // Five rooms already in the database when it is made
            modelBuilder.Entity<Room>().HasData(
                new Room { Id = 1, Code = "MB-101", Name = "Lecture Theatre A", Seats = 120, HasProjector = true, BuildingId = 1 },
                new Room { Id = 2, Code = "MB-204", Name = "Seminar Room", Seats = 30, HasProjector = false, BuildingId = 1 },
                new Room { Id = 3, Code = "SB-110", Name = "Chemistry Lab", Seats = 24, HasProjector = true, BuildingId = 2 },
                new Room { Id = 4, Code = "SB-215", Name = "Computer Lab 2", Seats = 35, HasProjector = true, BuildingId = 2 },
                new Room { Id = 5, Code = "LIB-01", Name = "Study Room 1", Seats = 8, HasProjector = false, BuildingId = 3 }
            );

            // No two rooms with the same code. SQL Server refuses an INSERT or UPDATE that breaks it.
            modelBuilder.Entity<Room>()
                .HasIndex(room => room.Code)
                .IsUnique();
        }
    }
}
