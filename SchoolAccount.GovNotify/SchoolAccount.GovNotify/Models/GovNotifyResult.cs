using Notify.Exceptions;

namespace SchoolAccount.GovNotify.Models;

public class GovNotifyResult
{
    public bool IsSuccessful => string.IsNullOrEmpty(ErrorMessage);
    public string? ErrorMessage { get; set; }
    
    public GovNotifyResult()
    {}

    public GovNotifyResult(NotifyClientException exception)
    {
        ErrorMessage = exception.Message;
    }
}