using Common;
using System.Threading.RateLimiting;
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

var rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
{
    TokenLimit = options.BatchAmount,               // Maximum burst capacity
    QueueLimit = 0,                                 // Don't queue extra tasks, fail or block immediately
    TokensPerPeriod = options.BatchWaitAmountInSec, // Add 50 new permits every second
    AutoReplenishment = true,
    ReplenishmentPeriod = TimeSpan.FromSeconds(1)
});

var parallelOptions = new ParallelOptions
{
    MaxDegreeOfParallelism = options.MaxDegreeOfParallelism,   // Max 10 threads running at the exact same millisecond
    CancellationToken = CancellationToken.None
};

Console.WriteLine($"Sending {emailsToSend.Count} messages");

await Parallel.ForEachAsync(emailsToSend, parallelOptions, async (email, token) =>
{
    // Threads will naturally block here and wait if they exceed 50 requests/sec using a RateLimitLease
    using var lease = await rateLimiter.AcquireAsync(permitCount: 1, token);
    
    if (lease.IsAcquired)
    {
        try
        {
            Console.WriteLine($"Starting {email.Email} {token.ToString()}");
            await SendEmailAsync(email.LAEStab, email.Email, email.Status, token);
            Console.WriteLine($"Complete {email.Email} {token.ToString()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed: {ex.Message}");
        }
    }
    
    Console.WriteLine($"Complete for {email.Email} {token.ToString()}");
});

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