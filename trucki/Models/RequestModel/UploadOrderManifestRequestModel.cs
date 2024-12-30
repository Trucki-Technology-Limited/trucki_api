namespace trucki.Models.RequestModel;

public class UploadOrderManifestRequestModel
{
    public List<string> Documents { get; set; }
    public string orderId { get; set; }
    public string? type { get; set; }
}