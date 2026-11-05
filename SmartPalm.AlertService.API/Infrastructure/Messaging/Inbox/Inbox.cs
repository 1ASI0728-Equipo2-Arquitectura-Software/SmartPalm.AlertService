using Microsoft.EntityFrameworkCore;
using SmartPalm.AlertService.Application.Ports;
using SmartPalm.AlertService.Infrastructure.Persistence;

namespace SmartPalm.AlertService.Infrastructure.Messaging;

internal class Inbox(AlertDbContext context) : IInbox
{
    public Task<bool> HasProcessedAsync(Guid messageId, CancellationToken token) =>
        context.InboxMessages.AnyAsync(x => x.Id == messageId, token);
    public void MarkProcessed(Guid messageId, string eventType) => context.InboxMessages.Add(new InboxMessage
    { Id = messageId, EventType = eventType, ProcessedOnUtc = DateTime.UtcNow });
}
