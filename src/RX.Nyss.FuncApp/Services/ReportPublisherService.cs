using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RX.Nyss.FuncApp.Clients;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services;

public interface IReportPublisherService
{
    Task AddReportToQueue(Report report);
}

public class ReportPublisherService : IReportPublisherService
{
    private readonly ServiceBusSender _sender;

    public ReportPublisherService(IConfiguration configuration, ServiceBusClient serviceBusClient)
    {
        _sender = serviceBusClient.CreateSender(configuration["SERVICEBUS_REPORTQUEUE"]);
    }

    public async Task AddReportToQueue(Report report)
    {
        var message = new ServiceBusMessage(JsonConvert.SerializeObject(report))
        {
            ContentType = "application/json"
        };
        await _sender.SendMessageAsync(message);
    }
}


public class DirectReportPublisherService : IReportPublisherService
{
    private readonly ILogger<DirectReportPublisherService> _logger;
    private readonly ReportApiClient _reportApiClient;

    public DirectReportPublisherService(ILogger<DirectReportPublisherService> logger, ReportApiClient reportApiClient)
    {
        _logger = logger;
        _reportApiClient = reportApiClient;
    }
    public async Task AddReportToQueue(Report report)
    {
        await _reportApiClient.PostReport(report);
        _logger.LogInformation("Report source: {Source}, message: {Content}", report.ReportSource, report.Content);
    }
}