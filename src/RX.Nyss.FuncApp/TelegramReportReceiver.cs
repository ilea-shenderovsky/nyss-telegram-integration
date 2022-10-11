using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.FuncApp.Contracts;
using RX.Nyss.FuncApp.Services;

namespace RX.Nyss.FuncApp;

public class TelegramReportReceiver
{
    private readonly ILogger<TelegramReportReceiver> _logger;
    private readonly IConfig _config;
    private readonly IReportPublisherService _reportPublisherService;

    public TelegramReportReceiver(ILogger<TelegramReportReceiver> logger, IConfig config, IReportPublisherService reportPublisherService)
    {
    }

    [FunctionName("EnqueueTelegramReport")]
    public async Task<IActionResult> EnqueueTelegramReport(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "enqueueTelegramReport")] HttpRequestMessage httpRequest,
        [Blob("%AuthorizedApiKeysBlobPath%", FileAccess.Read)] string authorizedApiKeys)
    {
        var maxContentLength = _config.MaxContentLength;
        var contentLength = httpRequest.Content.Headers.ContentLength;
        if (contentLength == null || contentLength > maxContentLength)
        {
            _logger.Log(LogLevel.Warning, $"Received a telegram request with length more than {maxContentLength} bytes. (length: {contentLength.ToString() ?? "N/A"})");
            return new BadRequestResult();
        }

        var httpRequestContent = await httpRequest.Content.ReadAsStringAsync();
        _logger.Log(LogLevel.Debug, $"Received telegram report: {httpRequestContent}.{Environment.NewLine}HTTP request: {httpRequest}");

        if (string.IsNullOrWhiteSpace(httpRequestContent))
        {
            _logger.Log(LogLevel.Warning, "Received an empty telegram report.");
            return new BadRequestResult();
        }

        var decodedHttpRequestContent = HttpUtility.UrlDecode(httpRequestContent);

        var report = new Report
        {
            Content = httpRequestContent,
            ReportSource = ReportSource.Telegram
        };

        await _reportPublisherService.AddReportToQueue(report);

        return new OkResult();
    }
}