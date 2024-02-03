using Converter.Api.Service.Converters.Interfaces;
using Converter.Api.Service.Exceptions;
using Converter.Api.Service.Model;
using Converter.Api.Service.Repositories.Interfaces;
using Converter.Api.Service.Services.Interfaces;
using Converter.Api.Service.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using FileNotFoundException = Converter.Api.Service.Exceptions.FileNotFoundException;

namespace Converter.Api.Service.Services
{
    public class FileService : IFileService
    {
        private readonly IConverterFactory _converterFactory;
        private readonly IFileRepository _fileRepository;
        private readonly string _directoryPath;
        private readonly int _maxRetry;

        public FileService(
            IFileRepository fileRepository,
            IConverterFactory converterFactory,
            IOptions<ApplicationSettings> settings)
        {
            _fileRepository = fileRepository;
            _converterFactory = converterFactory;
            var settings1 = settings.Value;
            _maxRetry = settings1?.MaxRetries ?? 5;
            _directoryPath = settings1?.FileSettings.DirectoryPath;
        }

        public async Task<Dictionary<Guid, string>> GetFilesAsync()
        {
            try
            {
                return await RetryPolicyExecutor.ExecuteAsync(() => _fileRepository.GetAsync(), _maxRetry);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong.", ex);
            }
        }

        public async Task<DocumentModel> GetFileAsync(Guid id)
        {
            var files = await GetFilesAsync();

            if (files.TryGetValue(id, out var fileName) && File.Exists($"{_directoryPath}/{fileName}"))
            {
                var fileStream = new FileStream($"{_directoryPath}/{fileName}", FileMode.Open, FileAccess.Read);
                return new DocumentModel { Id = id, Name = fileName, Stream = fileStream };
            }

            throw new FileNotFoundException($"File '{id}' not found");
        }

        public async Task<FileModel> ConvertAsync(IFormFile htmlFile)
        {
            if (htmlFile == null || htmlFile.Length == 0)
            {
                throw new IncorrectInputFileException("Invalid file. Please select a valid file.");
            }

            var fileExtension = Path.GetExtension(htmlFile.FileName).ToLower();

            var factory = _converterFactory.CreateConverter(fileExtension);

            return await RetryPolicyExecutor.ExecuteAsync(async () => await factory.ConvertAsync(htmlFile), _maxRetry);
        }

        public async Task DeleteFileAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException($"Incorrect file name {id}");
            }
            try
            {
                await RetryPolicyExecutor.ExecuteAsync(() => _fileRepository.DeleteAsync(id), _maxRetry);
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong.", ex);
            }
        }
    }
}
