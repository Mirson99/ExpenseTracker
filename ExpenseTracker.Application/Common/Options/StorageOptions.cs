namespace ExpenseTracker.Application.Common.Options;

public class StorageOptions
{
    public const string SectionName = "Storage";
    public string BucketName { get; set; } = string.Empty;
}

