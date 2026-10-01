using Drivers.BLL.Managers.Contracts;
using Drivers.DAL.EF.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Drivers.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<PhotoController> _logger;
        private IPhotoManager _photoManager;

        public PhotoController(IWebHostEnvironment webHostEnvironment,
            ILogger<PhotoController> logger,
            IPhotoManager photoManager)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _photoManager = photoManager;
        }


        [HttpPost("ImageSharp_Upload")]
        public async Task<ActionResult> ImageSharp_UploadImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

                string datestring = DateTime.Now.ToString("yyyyMMddHHmmss");
                string fileExtension = Path.GetExtension(imageFile.FileName);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imageFile.FileName);
                var transfromedFilePath = Path.Combine("Uploads", fileNameWithoutExtension + "_" + datestring + fileExtension);
                await _photoManager.ImageSharp_CompressAndSaveImageAsync(imageFile, transfromedFilePath, 30);
            var originalFilePath = Path.Combine("Uploads", fileNameWithoutExtension + "_" + "original" + "_" + datestring + fileExtension);
            using (var fileStream = new FileStream(originalFilePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return Ok("Image uploaded and compressed.");
        }

        [HttpPost("MagicNet_Upload")]
        public async Task<ActionResult> MagicNet_UploadImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No file uploaded");
            }
                string datestring = DateTime.Now.ToString("yyyyMMddHHmmss");
                string fileExtension = Path.GetExtension(imageFile.FileName);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imageFile.FileName);
                var transfromedFilePath = Path.Combine("Uploads", fileNameWithoutExtension + "_" + datestring + fileExtension);
                await _photoManager.MagickNet_CompressAndResizeImage(imageFile, transfromedFilePath, 50, 200, 200);
            var originalFilePath = Path.Combine("Uploads", fileNameWithoutExtension + "_" + "original" + "_" + datestring + fileExtension);
            using (var fileStream = new FileStream(originalFilePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            return Ok("Image uploaded and compressed.");
        }

        [HttpPost("Image_SimpleUpload")]
        public async Task<ActionResult> Post(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("Please upload any Image");
            }

            string fileName = image.FileName;
            string extension = Path.GetExtension(fileName);

            string[] allow = { ".jpg", ".png" };

            if (!allow.Contains(extension.ToLower()))
            {
                return BadRequest("Invalid Image, Try Another");
            }

            string fileNameNew = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads", fileNameNew);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await image.CopyToAsync(fileStream);
            }

            return Ok($"Uploads/{fileNameNew}");
        }

        [HttpPost("Image_DatabaseUpload")]
        public async Task<ActionResult<EFPhoto>> UploadPhoto(IFormFile image,CancellationToken cancellationToken)
        {
            string fileName = image.FileName;
            string extension = Path.GetExtension(fileName);

            string[] allow = { ".jpg", ".png" };

            if (!allow.Contains(extension.ToLower())) return BadRequest("Invalid Image, Try Another");

            string fileNameNew = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads", fileNameNew);

            var result = await _photoManager.WriteToDbAsync(image, cancellationToken);

            return Ok(result);
        }
    }
}

