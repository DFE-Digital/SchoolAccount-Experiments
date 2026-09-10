using Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    services.AddOptions<MailOptions>().Bind(hostContext.Configuration.GetSection("Mail"));
    services.AddTransient<CensusLedgerManager>();
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var ledgerManager = scope.ServiceProvider.GetRequiredService<CensusLedgerManager>();
var options = scope.ServiceProvider.GetRequiredService<IOptions<MailOptions>>().Value;

var emailsToSend = await ledgerManager.DetermineWhoAndWhatToSendAsync("spring2025");
var batches = emailsToSend.Chunk(options.BatchAmount);
var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = options.MaxDegreeOfParallelism };

foreach (var batch in batches)
{
    Console.WriteLine($"Starting a new batch of {batch.Length} emails...");

    await Parallel.ForEachAsync(batch, parallelOptions, async (email, token) =>
    {
        await SendEmailAsync(email.LAEStab, email.Email, email.Status, token);
    });

    // Throttle between chunks to protect your email server
    for (var i = 1; i <= options.BatchWaitAmountInSec; i++)
    {
        Console.WriteLine(
            $"Batch finished. Pausing for {i}/{options.BatchWaitAmountInSec} seconds to respect rate limits...");

        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}

async Task SendEmailAsync(string laeStab, string toAddress, string status, CancellationToken cancellationToken)
{
    var hash = $"{laeStab}:{toAddress}"; // to has or whatever
    await SendNotify(toAddress, cancellationToken);
    await UpdateStatus(hash, "Sent");
}

Task SendNotify(string to, CancellationToken token)
{
    // Mock
    Task.Delay(200, token);
    Console.WriteLine($"Sending Notification to {to}");
    return Task.CompletedTask;
}

Task UpdateStatus(string id, string status)
{
    // Mock
    Console.WriteLine($"Updating status for {id}: {status}");
    return Task.CompletedTask;
}