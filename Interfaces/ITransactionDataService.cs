using Microsoft.AspNetCore.Mvc;
using UploadFiles.Models;

namespace UploadFiles.Interfaces
{
    public interface ITransactionDataService
    {
        public Task<ResponseUploadFile> UploadFile(IFormFile file);
        public Task<ResponseTransactionData> GetAllTransactionData();
        public Task<ResponseTransactionData> GetTransactionDataByCurrency(string currencyCode);
        public Task<ResponseTransactionData> GetTransactionDataByStatus(string status);
    }
}
