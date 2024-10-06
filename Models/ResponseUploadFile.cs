namespace UploadFiles.Models
{
    public class ResponseUploadFile
    {
        public int Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public List<TransactionData> TransactionDataList { get; set; } = new List<TransactionData>();
        public List<ResponseInvalidCSVFile> ResponseInvalidCSVFileList { get; set; } = new List<ResponseInvalidCSVFile>();
    }
}
