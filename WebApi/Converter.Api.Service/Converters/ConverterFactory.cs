using Converter.Api.Service.Converters.Interfaces;
using Converter.Api.Service.Extensions;
using Converter.Api.Service.Repositories.Interfaces;
using Converter.Api.Service.Settings;
using Microsoft.Extensions.Options;

namespace Converter.Api.Service.Converters
{
    public class ConverterFactory(
        IFileRepository fileRepository,
        IOptions<ApplicationSettings> settings)
        : IConverterFactory
    {
        public IConverter CreateConverter(string fileExtension)
        {
            var fileType = fileExtension.ToConverterType();

            switch (fileType)
            {
                case ConverterType.Html:
                    return new HtmlToPdfConverter(fileRepository, settings);
                default:
                    throw new NotSupportedException("Invalid file. Please change type of file.");
            }
        }
    }
}
