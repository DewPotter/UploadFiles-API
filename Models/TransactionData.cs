using CsvHelper.Configuration.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UploadFiles.Models
{
    [Table("TRANSACTION_DATA")]
    public class TransactionData
    {
        [Key]
        public long ID { get; set; }
        [Required]
        [MaxLength(50)]
        public string TRANSACTION_ID { get; set; } = string.Empty;
        [Required]
        [MaxLength(30)]
        public string ACCOUNT_NO { get; set; } = string.Empty;
        [Required]
        public decimal AMOUNT { get; set; }
        [Required]
        public string CURRENCY_CODE { get; set; } = string.Empty;
        [Required]
        public DateTime TRANSACTION_DATE { get; set; }
        [Required]
        public string STATUS { get; set; } = string.Empty;
        [Required]
        public string FILE_FORMATS { get; set; } = string.Empty;
        [Required]
        public string CREATED_BY { get; set; } = string.Empty;
        [Required]
        public DateTime CREATED_DATE { get; set; }
        public string? UPDATED_BY { get; set; }
        public DateTime? UPDATED_DATE { get; set; }
    }
}
