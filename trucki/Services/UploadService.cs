using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using trucki.Interfaces.IServices;

public class UploadService: IUploadService
{
    private readonly Cloudinary _cloudinary;
    private readonly IConfiguration _configuration;
    public UploadService(IConfiguration configuration)
    {
        _configuration = configuration;
        var cloudName = _configuration.GetSection("Cloudinary").GetSection("cloudName").Value;
        var apiKey = _configuration.GetSection("Cloudinary").GetSection("apiKey").Value;
        var apiSecret = _configuration.GetSection("Cloudinary").GetSection("apiSecret").Value;
        Account account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }
    public Task<string> UploadFile(IFormFile file, string publicId)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            PublicId = publicId + file.FileName,
        };

        var uploadResult = _cloudinary.Upload(uploadParams);

        if (uploadResult.Error != null)
        {
            // Handle the error, you might want to throw an exception or log it.
            return null;
        }

        return Task.FromResult(uploadResult.SecureUrl.ToString());
    }
}