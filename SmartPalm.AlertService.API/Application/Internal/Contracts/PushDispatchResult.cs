namespace SmartPalm.AlertService.Application.Contracts;

public record PushDispatchResult(bool Sent, string? ProviderMessageId, string? Reason, bool Retryable = false);
