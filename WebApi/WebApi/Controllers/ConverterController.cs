using Converter.Api.Service.Extensions;
using Converter.Api.Service.Services.Interfaces;
using Converter.Api.Service.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("converter")]
    public class ConverterController(IFileService fileService, IOptions<ApplicationSettings> settings)
        : ControllerBase
    {
        private const long MaxFileSize = 100_000_000;
        private readonly ApplicationSettings _settings = settings.Value;

        public IFileService FileService { get; } = fileService;

        /// <summary>
        /// Get a list of files.
        /// </summary>
        /// <returns>A list of files.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = (await FileService.GetFilesAsync()).ToFileModels();
            return Ok(new { result });
        }

        /// <summary>
        /// Get details of a specific file by ID.
        /// </summary>
        /// <param name="id">The ID of the file.</param>
        /// <returns>Details of the file.</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Detail(Guid id)
        {
            var fileModel = await FileService.GetFileAsync(id);
            var stream = fileModel.Stream;

            return new FileStreamResult(stream, _settings.PdfContentType)
            {
                FileDownloadName = fileModel.Name,
                EnableRangeProcessing = true
            };
        }

        /// <summary>
        /// Convert HTML file to PDF.
        /// </summary>
        /// <param name="htmlFile">The HTML file to convert.</param>
        /// <returns>Result of the conversion operation.</returns>
        [HttpPost]
        [RequestSizeLimit(MaxFileSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
        public async Task<IActionResult> Create([FromForm] IFormFile htmlFile)
        {
            var resultModel = await FileService.ConvertAsync(htmlFile);

            return Ok(new { result = resultModel });
        }

        /// <summary>
        /// Delete a file by ID.
        /// </summary>
        /// <param name="id">The ID of the file to delete.</param>
        /// <returns>Status code indicating the success of the deletion operation.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await FileService.DeleteFileAsync(id);

            return StatusCode(201);
        }
    }
}
