using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Transfer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("FromEmployee")]
        public int FromEmployeeNumber { get; set; } // Gönderen (FK)

        [ForeignKey("ToEmployee")]
        public int? ToEmployeeNumber { get; set; } // Alıcı (FK, null ise dış banka transferi)

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // Transfer tutarı

        [StringLength(255)]
        public string? Description { get; set; } // Açıklama (opsiyonel)

        [Required]
        public DateTime TransferDate { get; set; } // Tarih

        [StringLength(34)]
        public string? ExternalBankIban { get; set; } // Dış banka IBAN (opsiyonel, dış transfer için)

        // Navigation properties (opsiyonel, çalışan detayları için)
        public Employee? FromEmployee { get; set; }
        public Employee? ToEmployee { get; set; }
    }
}