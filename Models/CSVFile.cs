using CsvHelper.Configuration.Attributes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace UploadFiles.Models
{
    public class CSVFile
    {
        [Index(0)]
        public string TransactionIdentificator { get; set; } = string.Empty;
        [Index(1)]
        public string AccountNumber { get; set; } = string.Empty;
        [Index(2)]
        public string Amount { get; set; } = string.Empty;
        [Index(3)]
        public string CurrencyCode { get; set; } = string.Empty;
        [Index(4)]
        public string TransactionDate { get; set; } = string.Empty;
        [Index(5)]
        public string Status { get; set; } = string.Empty;
    }

    public class ResponseInvalidCSVFile
    {
        public string TransactionIdentificator { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string TransactionDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;
    }
}
