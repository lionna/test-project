using System.Text;
using Converter.Api.Service.Converters;
using Converter.Api.Service.Exceptions;
using Converter.Api.Service.Repositories.Interfaces;
using Converter.Api.Service.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Converter.Api.Service.Tests.Converters
{
    public class HtmlToPdfConverterTests
    {
        private readonly IFileRepository _fileRepository;
        private readonly IOptions<ApplicationSettings> _settings;

        public HtmlToPdfConverterTests()
        {
            _fileRepository = new Mock<IFileRepository>().Object;
            _settings = Options.Create(new ApplicationSettings
            {
                FileSettings = new FileSettings
                {
                    BufferSize = 8192,
                    DirectoryPath = "files",
                    DateFormat = "yyyy-MM-dd-HH-mm-ss"
                }
            });
        }

        [Fact]
        public async Task ConvertAsync_ValidHtmlFile_Success()
        {
            // Arrange
            var converter = new HtmlToPdfConverter(_fileRepository, _settings);
            var htmlContent = "<html><body><h1>Hello World!</h1></body></html>";
            var htmlFile = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes(htmlContent)), 0, htmlContent.Length, "htmlFile", "file.html");

            // Act
            var result = await converter.ConvertAsync(htmlFile);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.EndsWith(".pdf", result.Name, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ConvertAsync_InvalidHtmlFile_ThrowsException()
        {
            // Arrange
            var converter = new HtmlToPdfConverter(_fileRepository, _settings);
            var invalidFile = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("Invalid content")), 0, 10, "invalidFile", "invalid.txt");

            // Act & Assert
            await Assert.ThrowsAsync<IncorrectInputFileException>(() => converter.ConvertAsync(invalidFile));
        }
    }
}
