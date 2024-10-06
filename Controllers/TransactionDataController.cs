using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UploadFiles.Interfaces;
using UploadFiles.Models;

namespace UploadFiles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionDataController : ControllerBase
    {
        private readonly ILogger<TransactionDataController> _logger;
        private readonly ITransactionDataService _transactionDataService;

        public TransactionDataController(ILogger<TransactionDataController> logger, ITransactionDataService transactionDataService)
        {
            _logger = logger;
            _transactionDataService = transactionDataService;
        }

        // GET: api/TransactionData
        [HttpGet]
        public async Task<ActionResult<ResponseTransactionData>> GetAllTransactionData()
        {
            ResponseTransactionData response = new ResponseTransactionData();
            response = await _transactionDataService.GetAllTransactionData();

            return StatusCode(response.Status, response);
        }

        [HttpGet("Currency")]
        public async Task<ActionResult<ResponseTransactionData>> GetTransactionDataByCurrency(string currencyCode)
        {
            ResponseTransactionData response = new ResponseTransactionData();
            response = await _transactionDataService.GetTransactionDataByCurrency(currencyCode);

            return StatusCode(response.Status, response);
        }

        [HttpGet("Status")]
        public async Task<ActionResult<ResponseTransactionData>> GetTransactionDataByStatus(string status)
        {
            ResponseTransactionData response = new ResponseTransactionData();
            response = await _transactionDataService.GetTransactionDataByStatus(status);

            return StatusCode(response.Status, response);
        }
    }
}
