using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UploadFiles.Models;
using UploadFiles.Interfaces;
using UploadFiles.Data;

namespace UploadFiles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadFilesController : ControllerBase
    {
        private readonly ILogger<UploadFilesController> _logger;
        private readonly ITransactionDataService _transactionDataService;

        public UploadFilesController(ILogger<UploadFilesController> logger, ITransactionDataService transactionDataService)
        {
            _logger = logger;
            _transactionDataService = transactionDataService;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseUploadFile>> UploadFile(IFormFile file)
        {
            ResponseUploadFile response = new ResponseUploadFile();
            response = await _transactionDataService.UploadFile(file);

            return StatusCode(response.Status, response);
        }
    }
}
