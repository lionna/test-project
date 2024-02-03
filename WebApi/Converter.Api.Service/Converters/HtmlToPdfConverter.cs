using Converter.Api.Service.Converters.Interfaces;
using Converter.Api.Service.Exceptions;
using Converter.Api.Service.Model;
using Converter.Api.Service.Repositories.Interfaces;
using Converter.Api.Service.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using System.Text;

namespace Converter.Api.Service.Converters
{
    public class HtmlToPdfConverter : IConverter
    {
        private static int _bufferSize;
        private static string _directoryPath;
        private static string _dateFormat;

        private readonly IFileRepository _fileRepository;

        public HtmlToPdfConverter(
            IFileRepository fileRepository,
            IOptions<ApplicationSettings> settings)
        {
            _fileRepository = fileRepository;
            var settings1 = settings.Value;
            _bufferSize = settings1?.FileSettings?.BufferSize ?? 8192;
            _directoryPath = settings1?.FileSettings?.DirectoryPath ?? "files";
            _dateFormat = settings1?.FileSettings?.DateFormat ?? "yyyy-MM-dd-HH-mm-ss";
        }

        private static string PdfFileName(string fileName)
        {
            var date = DateTime.Now.ToString(_dateFormat);
            var originalFileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            return $"{originalFileNameWithoutExtension}_{date}.pdf";
        }

        public async Task<FileModel> ConvertAsync(IFormFile htmlFile)
        {
            if (htmlFile == null || htmlFile.Length == 0 || !Path.GetExtension(htmlFile.FileName).Equals(".html", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new IncorrectInputFileException("Invalid file. Please select a valid HTML file.");
            }

            var id = Guid.NewGuid();
            var pdfFileName = PdfFileName(htmlFile.FileName);

            try
            {

                using (var htmlStream = new StreamReader(htmlFile.OpenReadStream()))
                {
                    var pdfFilePath = Path.Combine(_directoryPath, pdfFileName);
                    Directory.CreateDirectory(_directoryPath);

                    using (var pdfFileStream = new FileStream(pdfFilePath, FileMode.Create))
                    {
                        try
                        {
                            await ConvertToPdfAsync(htmlStream, pdfFileStream);
                        }
                    }

                    await _fileRepository.AddAsync(id, pdfFileName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing file.", ex);
            }

            return new FileModel { Id = id, Name = pdfFileName };
        }

        private static async Task ConvertToPdfAsync(StreamReader htmlStream, Stream pdfStream)
        {
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                //ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"
            };

            using (var browser = await Puppeteer.LaunchAsync(launchOptions))
            {
                using (var page = await browser.NewPageAsync())
                {
                    var buffer = new char[_bufferSize];
                    int bytesRead;

                    var content = new StringBuilder();

                    while ((bytesRead = await htmlStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        content.Append(buffer, 0, bytesRead);
                    }

                    await page.SetContentAsync(content.ToString());

                    var tempPdfPath = Path.GetTempFileName();

                    try
                    {
                        await page.PdfAsync(tempPdfPath, new PdfOptions { Format = PuppeteerSharp.Media.PaperFormat.Letter, PageRanges = "" });

                        using (var tempPdfStream = new FileStream(tempPdfPath, FileMode.Open))
                        {
                            await tempPdfStream.CopyToAsync(pdfStream);
                        }
                    }
                    finally
                    {
                        File.Delete(tempPdfPath);
                    }
                }
            }
        }
    }
}
