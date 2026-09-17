using AutoMapper;
using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.Managers.Contracts;
using Drivers.DAL.ADO.UOW.Contracts;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.UOW.Contracts;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Drivers.BLL.Managers;

/// <summary>
/// Менеджер обробки та завантаження фотографій водіїв
/// </summary>
public class PhotoManager : IPhotoManager
{
    private readonly ILogger<PhotoManager> _logger;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _adoUow;
    private readonly IEFUnitOfWork _efUow;

    public PhotoManager(
        ILogger<PhotoManager> logger,
        IMapper mapper,
        IUnitOfWork adoUow,
        IEFUnitOfWork efUow)
    {
        _logger = logger;
        _mapper = mapper;
        _adoUow = adoUow;
        _efUow = efUow;
    }

    /// <summary>
    /// Стиснення та збереження зображення через бібліотеку ImageSharp
    /// </summary>
    public async Task ImageSharp_CompressAndSaveImageAsync(
        IFormFile imageFile,
        string outputPath,
        int quality = 50)
    {
        using var imageStream = imageFile.OpenReadStream();
        using var image = await Image.LoadAsync(imageStream);

        image.Mutate(x => x
            .Resize(image.Width / 2, image.Height / 2)
            .Grayscale());

        var encoder = new JpegEncoder
        {
            Quality = quality,
        };

        await image.SaveAsync(outputPath, encoder);
    }

    /// <summary>
    /// Стиснення та зміна розміру зображення через Magick.NET
    /// </summary>
    public async Task MagickNet_CompressAndResizeImage(
        IFormFile imageFile,
        string outputImagePath,
        int quality = 50,
        int width = 100,
        int height = 100)
    {
        using var imageStream = imageFile.OpenReadStream();
        using var image = new MagickImage(imageStream);

        image.Resize(width, height);
        image.Quality = quality;

        using var outputStream = new FileStream(outputImagePath, FileMode.Create);
        await image.WriteAsync(outputStream);
    }

    /// <summary>
    /// Збереження завантаженого зображення у файлову систему
    /// </summary>
    public async Task UploadImage(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            throw new ArgumentException("Файл зображення порожній або відсутній.", nameof(imageFile));
        }

        var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Photos");
        if (!Directory.Exists(uploadDirectory))
        {
            Directory.CreateDirectory(uploadDirectory);
        }

        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await imageFile.CopyToAsync(stream);

        _logger.LogInformation("Зображення успішно збережено: {FilePath}", filePath);
    }

    /// <summary>
    /// Збереження обробленого мініатюрного зображення безпосередньо в базу даних через EF Core
    /// </summary>
    public async Task<EFPhoto> WriteToDbAsync(IFormFile image, CancellationToken cancellationToken)
    {
        try
        {
            await _efUow.BeginTransactionAsync(cancellationToken);

            var photoReqDTO = new PhotoReqDTO();

            using (var imageStream = image.OpenReadStream())
            {
                using var originalImage = await Image.LoadAsync(imageStream, cancellationToken);

                originalImage.Mutate(x => x
                    .Resize(new ResizeOptions
                    {
                        Size = new Size(100, 100),
                        Mode = ResizeMode.Max
                    }));

                using var resizedImageStream = new MemoryStream();
                await originalImage.SaveAsync(resizedImageStream, new JpegEncoder(), cancellationToken);
                photoReqDTO.PhotoData = resizedImageStream.ToArray();

                photoReqDTO.ContentType = image.ContentType;
                photoReqDTO.FileName = image.FileName;
                photoReqDTO.FileSize = photoReqDTO.PhotoData.Length;
            }

            var result = await _efUow.EFPhotoRepository.AddAsync(_mapper.Map<EFPhoto>(photoReqDTO));
            await _efUow.CompleteAsync(cancellationToken);
            await _efUow.CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await _efUow.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Помилка транзакції при додаванні фото водія");
            throw new ApplicationException("Error in transaction while adding information about photo", ex);
        }
    }
}
