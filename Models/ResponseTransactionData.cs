namespace UploadFiles.Models
{
    public class ResponseTransactionData
    {
        public int Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public List<TransactionDisplay> TransactionDisplays { get; set; } = new List<TransactionDisplay>();
    }

    public class TransactionDisplay
    {
        public string Id { get; set; } = string.Empty;
        public string Payment { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
