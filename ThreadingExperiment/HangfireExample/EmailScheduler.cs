using Common;
using Hangfire;
using Microsoft.Extensions.Options;

namespace HangfireExample;

public class EmailScheduler(CensusLedgerManager ledgerManager, IOptions<MailOptions> options)
{
    [AutomaticRetry(Attempts = 0)]
    public async Task DispatchScheduledEmailsAsync()
    {
        var emailsToSend = await ledgerManager.DetermineWhoAndWhatToSendAsync("spring2025");

        var batchIndex = 0;
        var itemIndex = 0;
        foreach (var batch in emailsToSend.Chunk(options.Value.BatchAmount))
        {
            foreach (var email in batch)
            {
                var delay = (options.Value.BatchWaitAmountInSec * itemIndex) + (options.Value.ItemWaitAmountInSec * batchIndex);
                BackgroundJob.Schedule(() =>
                        SendEmailBackgroundAsync(email.LAEStab, email.Email, email.Status, CancellationToken.None),
                    TimeSpan.FromSeconds(delay));

                itemIndex++;
            }

            batchIndex++;
            itemIndex = 0;
        }
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task SendEmailBackgroundAsync(string laeStab, string toAddress, string status, CancellationToken cancellationToken)
    {
        var hash = $"{laeStab}:{toAddress}"; // to has or whatever
        await SendNotify(toAddress, cancellationToken);
        await UpdateStatus(hash, "Sent");
    }

    private Task SendNotify(string to, CancellationToken token)
    {
        // Mock
        return Task.Delay(200, token);
    }

    private Task UpdateStatus(string id, string status)
    {
        // Mock
        return Task.CompletedTask;
    }
}