using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public int EmployeeNumber { get; set; } // İlgili çalışan (FK)

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // Harcama/Masraf tutarı

        [Required]
        [StringLength(50)]
        public string Category { get; set; } // Harcama kategorisi (örn: Gıda, Ulaşım...)

        [StringLength(255)]
        public string? Description { get; set; } // Açıklama (opsiyonel)

        [Required]
        public DateTime TransactionDate { get; set; } // Tarih

        // Navigation property (opsiyonel, çalışan detayı için)
        public Employee? Employee { get; set; }
    }
}