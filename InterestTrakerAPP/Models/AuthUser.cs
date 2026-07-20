using SQLite;

namespace InterestTrakerAPP.Models
{
    public class AuthUser
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string Username { get; set; }

        // Note: For a production app, this should be a cryptographic hash, 
        // but for your presentation demo, plain text is perfectly fine.
        public string Password { get; set; }
    }
}