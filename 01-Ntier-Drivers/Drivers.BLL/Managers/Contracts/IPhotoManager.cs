using Drivers.DAL.EF.Entities;
using Microsoft.AspNetCore.Http;
namespace Drivers.BLL.Managers.Contracts
{
    public interface IPhotoManager
    {
       Task ImageSharp_CompressAndSaveImageAsync(IFormFile imageFile, string outputPath, int quality);
       Task MagickNet_CompressAndResizeImage(IFormFile imageFile, string outputImagePath, int quality, int width, int height);
       Task<EFPhoto> WriteToDbAsync(IFormFile image, CancellationToken cancellationToken);
    }
}
