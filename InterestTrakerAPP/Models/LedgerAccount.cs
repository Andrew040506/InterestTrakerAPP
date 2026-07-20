using SQLite;

namespace InterestTrakerAPP.Models
{
    public class LedgerAccount
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; } = true;
    }
}