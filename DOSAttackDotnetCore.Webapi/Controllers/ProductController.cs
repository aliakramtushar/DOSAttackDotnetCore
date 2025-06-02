using DOSAttackDotnetCore.Webapi.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DOSAttackDotnetCore.Webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProductController> _logger;
        private readonly string[] allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        private readonly string[] allowedMimeTypes = new[] { "image/jpeg", "image/png" };

        public ProductController(IWebHostEnvironment webHostEnvironment, ILogger<ProductController> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [HttpPost("createLimitFileSize")]
        public async Task<IActionResult> createLimitFileSize([FromForm] ProductDto product)
        {
            try
            {
                if (product.ImageFile != null)
                {
                    if (product.ImageFile.Length == 0)
                    {
                        _logger.LogWarning("❗ File missing in request.");
                        return BadRequest("File is empty.");
                    }

                    if (product.ImageFile.Length > 5 * 1024 * 1024) // 5MB
                    {
                        // this is for specific, for all api we added code in program.cs
                        _logger.LogWarning("❗ File size exceeded: {Size} bytes", product.ImageFile.Length);
                        return BadRequest("❌ File too large.");
                    }

                    var extension = Path.GetExtension(product.ImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        _logger.LogError("❌ Invalid extension: {Ext}", extension);
                        return BadRequest("❌ Invalid file extension.");
                    }

                    if (!allowedMimeTypes.Contains(product.ImageFile.ContentType))
                    {
                        _logger.LogWarning("❌ Invalid MIME type: {Mime}", product.ImageFile.ContentType);
                        return BadRequest("❌ Invalid MIME type.");
                    }

                    var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsPath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await product.ImageFile.CopyToAsync(stream);

                    _logger.LogInformation("✅ File uploaded: {FileName}", filePath);
                    return Ok(new { message = "Product created with image.", fileName });
                }

                return BadRequest("ImageFile is required.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Unexpected error during file upload.");
                return StatusCode(500, "❌ Server error occurred.");
            }
        }

        [HttpPost("createRateLimiting")]
        public IActionResult createRateLimiting([FromForm] ProductDto product)
        {
            _logger.LogInformation("📦 Product created: {Name}, Price: {Price}", product.Name, product.Price);
            return Ok("✅ Product created successfully.");
        }
    }

}
