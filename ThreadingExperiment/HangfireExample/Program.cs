using Common;
using Hangfire;
using HangfireExample;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    services.AddOptions<MailOptions>().Bind(hostContext.Configuration.GetSection("Mail"));
    
    services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseInMemoryStorage()); // to replace with sql

    services.AddHangfireServer(options =>
    {
        options.WorkerCount = 20; 
    });

    services.AddTransient<EmailScheduler>();
});

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    scope.ServiceProvider
        .GetRequiredService<IRecurringJobManager>()
        .AddOrUpdate<EmailScheduler>(
            nameof(EmailScheduler),
            job => job.DispatchScheduledEmailsAsync(),
            Cron.Daily(8)
        );
}

await host.RunAsync();