using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Transfer
    {
        [Key]
        public int Id { get; set; }
        public int FromEmployeeNumber { get; set; }
        public int? ToEmployeeNumber { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime TransferDate { get; set; }
        public string? ExternalBankIban { get; set; } // Dış transfer için
    }
}