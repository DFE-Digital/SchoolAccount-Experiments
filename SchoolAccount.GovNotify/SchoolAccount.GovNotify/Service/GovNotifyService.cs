using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Exceptions;
using Notify.Models.Responses;
using SchoolAccount.GovNotify.Models;

namespace SchoolAccount.GovNotify.Service;

public class GovNotifyService(
    IOptions<GovNotifySettings> settings
)
{
    private readonly NotificationClient _client = new(settings.Value.ApiKey);

    public async Task<GovNotifyResult> SendMessage(
        string templateId, 
        string recipient, 
        Dictionary<string, dynamic>? properties = null, 
        GovNotifyMailOptions? options = null)
    {
        try
        {
            await _client.SendEmailAsync(
                recipient, 
                templateId, 
                properties,
                clientReference: options?.Reference,
                emailReplyToId: options?.ReplyTo ?? settings.Value.FromAddress);
            
            return new GovNotifyResult();
        }
        catch (NotifyClientException ex)
        {
            return new GovNotifyResult(ex);
        }
    }
}