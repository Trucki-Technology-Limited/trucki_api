namespace trucki.Models.RequestModel;

public class UploadOrderManifestRequestModel
{
    public List<IFormFile> Documents { get; set; }
    public string orderId { get; set; }
}