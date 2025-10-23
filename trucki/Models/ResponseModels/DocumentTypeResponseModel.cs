namespace trucki.Models.ResponseModels;

public class DocumentTypeResponseModel
{
    public string Id { get; set; }
    public string Country { get; set; }
    public string EntityType { get; set; }
    public string Name { get; set; }
    public bool IsRequired { get; set; }
    public string Description { get; set; }
    public bool HasTemplate { get; set; }
    public string TemplateUrl { get; set; }
}