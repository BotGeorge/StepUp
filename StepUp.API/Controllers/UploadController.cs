using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace StepUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : BaseController
{
    private readonly IWebHostEnvironment _environment;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public UploadController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("image")]
    public async Task<ActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded." });
            }

            // Validate file size
            if (file.Length > MaxFileSize)
            {
                return BadRequest(new { success = false, message = "File size exceeds 5MB limit." });
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                return BadRequest(new { success = false, message = "Invalid file type. Allowed: jpg, jpeg, png, gif, webp" });
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "images");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Return URL (in production, this should be a public URL)
            // For now, we'll return a relative path that the mobile app can construct
            var imageUrl = $"/uploads/images/{fileName}";
            
            // In development, return full URL with localhost
            // In production, this should be your actual domain
            var baseUrl = Request.Scheme + "://" + Request.Host;
            var fullUrl = baseUrl + imageUrl;

            return Ok(new
            {
                success = true,
                message = "Image uploaded successfully.",
                url = fullUrl,
                relativeUrl = imageUrl
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while uploading the image.",
                error = ex.Message
            });
        }
    }
}
