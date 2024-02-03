using Converter.Api.Service.Model;
using Microsoft.AspNetCore.Http;

namespace Converter.Api.Service.Services.Interfaces
{
    public interface IFileService
    {
        Task<Dictionary<Guid, string>> GetFilesAsync();
        Task<DocumentModel> GetFileAsync(Guid id);
        Task<FileModel> ConvertAsync(IFormFile htmlFile);
        Task DeleteFileAsync(Guid id);
    }
}
