using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolAccount.GovNotify;
using SchoolAccount.GovNotify.Models;
using SchoolAccount.GovNotify.Service;

// Initialise 

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddOptions<GovNotifySettings>()
    .Bind(builder.Configuration.GetSection(GovNotifySettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddTransient<GovNotifyService>();

// Run

using var app = builder.Build();

var service = app.Services.GetRequiredService<GovNotifyService>();
var result = await service.SendMessage(
    GovNotifyTemplates.CensusStatusChange,
    "chris.kelly@education.gov.uk",
    new Dictionary<string, dynamic> { { "status", "In Progress" } }
);

if (result.IsSuccessful)
{
    Console.WriteLine("Message sent successfully");
}
else
{
    Console.WriteLine("Message failed to send: {0}", result.ErrorMessage);   
}
