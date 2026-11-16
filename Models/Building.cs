using System.ComponentModel.DataAnnotations;

namespace CampusRooms.Models
{
    // One building on campus. One building has many rooms.
    public class Building
    {
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string Name { get; set; } = "";

        // Navigation property: the rooms in this building
        public List<Room> Rooms { get; set; } = new();
    }
}
