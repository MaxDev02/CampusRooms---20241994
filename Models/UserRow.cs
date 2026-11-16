namespace CampusRooms.Models
{
    // One line of the Admin's Users page
    public class UserRow
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsStaff { get; set; }
    }
}
