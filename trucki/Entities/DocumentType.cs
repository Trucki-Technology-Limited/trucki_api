namespace trucki.Entities
{
    public class DocumentType : BaseClass
    {
        // "US", "NG", etc.
        public string Country { get; set; }

        // "Driver", "User", "Transport", etc.
        public string EntityType { get; set; }

        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public string Description { get; set; }
        
        // New Fields:
        
        // If this document requires a user to download/fill a form
        public bool HasTemplate { get; set; }

        // The URL/path where the template form can be downloaded
        public string TemplateUrl { get; set; }
    }
}
