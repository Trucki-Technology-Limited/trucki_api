namespace trucki.Entities;

public class TermsAndConditions : BaseClass
{
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool IsCurrentVersion { get; set; }
    public string DocumentType { get; set; }
}