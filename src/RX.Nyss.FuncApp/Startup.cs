using System;
using System.IO;
using System.Reflection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RX.Nyss.Data;
using RX.Nyss.FuncApp;
using RX.Nyss.FuncApp.Clients;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Services;
using Telegram.Bot;

[assembly: FunctionsStartup(typeof(Startup))]

namespace RX.Nyss.FuncApp;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder) => builder.AddConfiguration();
}

public static class FunctionHostBuilderExtensions
{
    private const string LocalSettingsJsonFileName = "local.settings.json";

    public static void AddConfiguration(this IFunctionsHostBuilder builder)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var localSettingsFile = Path.Combine(currentDirectory, LocalSettingsJsonFileName);

        var provider = builder.Services.BuildServiceProvider();
        var configuration = provider.GetService<IConfiguration>();

        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile(localSettingsFile, true, true)
            .AddEnvironmentVariables()
            .AddConfiguration(configuration)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true);

        var newConfiguration = configurationBuilder.Build();
        var nyssFuncAppConfig = newConfiguration.Get<NyssFuncAppConfig>();
        builder.Services.AddSingleton<IConfiguration>(newConfiguration);
        builder.Services.AddSingleton<IConfig>(nyssFuncAppConfig);
        builder.Services.AddAzureClients(clientFactoryBuilder =>
        {
            clientFactoryBuilder.AddServiceBusClient(newConfiguration["SERVICEBUS_CONNECTIONSTRING"]);
        });
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient<ReportApiClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5002");
        });
        builder.Services.AddSingleton<IHttpPostClient, HttpPostClient>();
        builder.Services.AddLogging();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IWhitelistValidator, WhitelistValidator>();
        builder.Services.AddScoped<ISmsService, SmsService>();
        builder.Services.AddScoped<IEmailAttachmentService, EmailAttachmentService>();
        builder.Services.AddScoped<IDeadLetterSmsService, DeadLetterSmsService>();

//        builder.Services.AddScoped<IReportPublisherService, ReportPublisherService>();
        builder.Services.AddScoped(typeof(IReportPublisherService),
            newConfiguration["AZURE_FUNCTIONS_ENVIRONMENT"] == "Development"
                ? typeof(DirectReportPublisherService)
                : typeof(ReportPublisherService));

        builder.Services.AddScoped<ITelegramBotClient>(_ => new TelegramBotClient("5707250499:AAF1S4T2buX3qEEJDDTzh7Ti8BAuG0ry3bk"));
        builder.Services.AddDbContext<NyssContext>(options => options.UseSqlServer(nyssFuncAppConfig.ConnectionStrings.NyssDatabase, x => x.UseNetTopologySuite()));

        //https://docs.microsoft.com/en-us/azure/azure-functions/functions-app-settings#azure_functions_environment
        builder.Services.AddScoped(typeof(IEmailClient), newConfiguration["AZURE_FUNCTIONS_ENVIRONMENT"] == "Development"
            ? typeof(DummyConsoleEmailClient)
            : typeof(SendGridEmailClient));
    }
}