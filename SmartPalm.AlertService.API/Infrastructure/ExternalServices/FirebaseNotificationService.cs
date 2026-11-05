using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using SmartPalm.AlertService.Application.Contracts;
using SmartPalm.AlertService.Application.Ports;
using SmartPalm.AlertService.Infrastructure.Configuration;

namespace SmartPalm.AlertService.Infrastructure.ExternalServices;

internal class FirebaseNotificationService : IPushNotificationService, IDisposable
{
    private readonly FirebaseOptions _options;
    private readonly ILogger<FirebaseNotificationService> _logger;
    private readonly FirebaseApp? _app;
    private readonly FirebaseMessaging? _messaging;

    public FirebaseNotificationService(IOptions<FirebaseOptions> options, ILogger<FirebaseNotificationService> logger)
    {
        _options = options.Value; _logger = logger;
        if (!_options.Enabled) return;
        var credentials = _options.CredentialsJson;
        if (string.IsNullOrWhiteSpace(credentials)) credentials = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");
        if (string.IsNullOrWhiteSpace(credentials)) { _logger.LogWarning("Firebase is enabled but credentials are missing; push notifications are disabled."); return; }
        try
        {
            _app = FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromJson(credentials) }, $"smartpalm-alert-{Guid.NewGuid():N}");
            _messaging = FirebaseMessaging.GetMessaging(_app);
        }
        catch (Exception ex) { _logger.LogError(ex, "Firebase initialization failed; push notifications are disabled."); }
    }

    public async Task<PushDispatchResult> SendAsync(PushNotification notification, CancellationToken token)
    {
        if (_messaging is null) return new PushDispatchResult(false, null, "Firebase is disabled or not configured.");
        try
        {
            var id = await _messaging.SendAsync(new Message
            {
                Topic = string.IsNullOrWhiteSpace(notification.Topic) ? _options.Topic : notification.Topic,
                Notification = new Notification { Title = notification.Title, Body = notification.Body }
            }, token);
            return new PushDispatchResult(true, id, null);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Firebase dispatch failed."); return new PushDispatchResult(false, null, ex.Message, true); }
    }

    public void Dispose() => _app?.Delete();
}
