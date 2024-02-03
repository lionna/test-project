using Converter.Api.Service.Model;
using Microsoft.AspNetCore.Http;

namespace Converter.Api.Service.Converters.Interfaces
{
   public interface IConverter
    {
        Task<FileModel> ConvertAsync(IFormFile htmlFile);
    }
}
