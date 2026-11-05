namespace SmartPalm.AlertService.Domain.Services;

public class DuplicateAlertPolicy
{
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(30);
    public DateTime WindowStart(DateTime nowUtc) => nowUtc.ToUniversalTime().Subtract(Window);
}
