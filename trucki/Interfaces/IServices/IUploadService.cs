namespace trucki.Interfaces.IServices;

public interface IUploadService
{
    Task<string> UploadFile(IFormFile file, string publicId);
}