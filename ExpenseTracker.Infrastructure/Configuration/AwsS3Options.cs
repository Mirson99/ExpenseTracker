namespace ExpenseTracker.Infrastructure.Configuration;

public class AwsS3Options
{
    public const string SectionName = "AwsS3Configuration";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string ServiceUrl { get; set; } = string.Empty;
    public string AuthenticationRegion { get; set; } = string.Empty;
}

