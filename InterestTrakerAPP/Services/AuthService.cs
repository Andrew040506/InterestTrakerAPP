using SQLite;
using System.IO;
using InterestTrakerAPP.Models;

namespace InterestTrakerAPP.Services
{
    public class AuthService
    {
        private SQLiteConnection _authDb;

        public AuthService()
        {
            // This is the master ledger for users
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "Auth.db");
            _authDb = new SQLiteConnection(dbPath);
            _authDb.CreateTable<AuthUser>();
        }

        public bool Register(string username, string password)
        {
            // Check if username is taken
            var existingUser = _authDb.Table<AuthUser>()
                                      .FirstOrDefault(u => u.Username.ToLower() == username.ToLower());

            if (existingUser != null)
                return false;

            _authDb.Insert(new AuthUser { Username = username, Password = password });
            return true;
        }

        public bool Login(string username, string password)
        {
            var user = _authDb.Table<AuthUser>()
                              .FirstOrDefault(u => u.Username.ToLower() == username.ToLower() && u.Password == password);

            return user != null;
        }
    }
}