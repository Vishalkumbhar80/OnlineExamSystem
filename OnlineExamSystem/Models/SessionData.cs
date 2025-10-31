namespace OnlineExamSystem.Models
{
    public class SessionData
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;    // short name or username
        public string FullName { get; set; } = string.Empty;    // display name
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Guest";             // Admin, User, Guest
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        // Optional extras
        public string ProfilePicUrl { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public List<RoleModel> UserRoles { get; set; }
    }
}
