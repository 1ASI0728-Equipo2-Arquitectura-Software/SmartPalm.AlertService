namespace SmartPalm.AlertService.Infrastructure.Configuration;

public class FirebaseOptions
{
    public const string SectionName = "Firebase";
    public bool Enabled { get; init; }
    public string? CredentialsJson { get; init; }
    public string Topic { get; init; } = "alerts";
}
