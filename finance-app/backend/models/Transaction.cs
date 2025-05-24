using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        public int EmployeeNumber { get; set; } // FK
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}