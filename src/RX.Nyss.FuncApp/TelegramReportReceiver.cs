using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.FuncApp.Contracts;
using RX.Nyss.FuncApp.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RX.Nyss.FuncApp;

public class TelegramReportReceiver
{
    private readonly ILogger<TelegramReportReceiver> _logger;
    private readonly IConfig _config;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IReportPublisherService _reportPublisherService;
    private readonly INyssContext _nyssContext;

    public TelegramReportReceiver(ILogger<TelegramReportReceiver> logger, IConfig config, ITelegramBotClient telegramBotClient, IReportPublisherService reportPublisherService, INyssContext nyssContext)
    {
        _logger = logger;
        _config = config;
        _telegramBotClient = telegramBotClient;
        _reportPublisherService = reportPublisherService;
        _nyssContext = nyssContext;
    }

    private const string SetupFunctionName = "setup";
    private const string EnqueueTelegramReportFunctionName = "enqueueTelegramReport";

    [FunctionName(SetupFunctionName)]
    public async Task<IActionResult> Setup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "setup")] HttpRequestMessage httpRequest)
    {
        await _telegramBotClient.SetWebhookAsync("https://909f-77-88-106-66.ngrok.io/api/enqueueTelegramReport");
        return new NoContentResult();
    }

//    [FunctionName(SetupFunctionName)]
//    public async Task RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req)
//    {
//        await _telegramBotClient.SetWebhookAsync("http://localhost:7070/api/enqueueTelegramReport");
//    }


    [FunctionName(EnqueueTelegramReportFunctionName)]
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

        var model = JsonConvert.DeserializeObject<Update>(httpRequestContent);

        if (model.Message.Type == MessageType.Contact)
        {
            var phoneNumber = model.Message.Contact?.PhoneNumber;
            var dataCollector = await _nyssContext.DataCollectors.SingleOrDefaultAsync(dc => dc.PhoneNumber == phoneNumber ||
                (dc.AdditionalPhoneNumber != null && dc.AdditionalPhoneNumber == phoneNumber));
            if (dataCollector == null)
            {
                var message = await _telegramBotClient.SendTextMessageAsync(model.Message.Chat.Id, "User not found in the system. Please ask administrator for help");
                return new OkResult();
            }

            dataCollector.TelegramId = model.Message.From.Username;
            await _nyssContext.SaveChangesAsync();
            await _telegramBotClient.SendTextMessageAsync(model.Message.Chat.Id, "User updated");
            return new OkResult();
        }

        if (!_nyssContext.DataCollectors.Any(d => d.TelegramId == model.Message.From.Username))
        {
            var replyMockup = new ReplyKeyboardMarkup(new KeyboardButton("Share phone number") { RequestContact = true });
            replyMockup.OneTimeKeyboard = true;
            replyMockup.ResizeKeyboard = true;
            await _telegramBotClient.SendTextMessageAsync(model.Message.Chat.Id, "Please share your phone number", replyMarkup: replyMockup);
            return new OkResult();
        }
        
        var simpleModel = new SimpleModel($"{model.Message?.From?.Username}", model.Message?.Text);

        var report = new Report
        {
            Content = JsonConvert.SerializeObject(simpleModel),
            ReportSource = ReportSource.Telegram
        };

        await _reportPublisherService.AddReportToQueue(report);

        return new OkResult();
    }
}

record SimpleModel(string Sender, string Message);