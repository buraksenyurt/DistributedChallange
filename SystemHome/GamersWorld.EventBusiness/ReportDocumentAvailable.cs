﻿using System.Net.Http.Json;
using GamersWorld.Domain.Enums;
using GamersWorld.Domain.Responses;
using Microsoft.Extensions.Logging;
using GamersWorld.Domain.Requests;
using GamersWorld.Domain.Constants;
using GamersWorld.Application.Contracts.Events;
using GamersWorld.Application.Contracts.Document;

namespace GamersWorld.EventBusiness;

public class ReportDocumentAvailable(
    ILogger<ReportDocumentAvailable> logger
    , IHttpClientFactory httpClientFactory
    , IDocumentWriter documentSaver)
    : IEventDriver<ReportReadyEvent>
{
    private readonly ILogger<ReportDocumentAvailable> _logger = logger;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IDocumentWriter _documentSaver = documentSaver;

    public async Task Execute(ReportReadyEvent appEvent)
    {
        var client = _httpClientFactory.CreateClient(Names.KahinGateway);
        _logger.LogInformation("{TraceId}, Ref Doc: {CreatedReportId}", appEvent.TraceId, appEvent.CreatedReportId);

        var payload = new
        {
            DocumentId = appEvent.CreatedReportId
        };
        var response = await client.PostAsJsonAsync("/getReport", payload);
        _logger.LogInformation("GetReport call status code is {StatusCode}", response.StatusCode);
        // QUESTION: Doküman çekilen servise ulaşılamadı. Akış nasıl devam etmeli?
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Document fetching error");
            return;
        }

        var getReportResponse = await response.Content.ReadFromJsonAsync<GetReportResponse>();

        if (getReportResponse != null && getReportResponse.StatusCode == StatusCode.ReportReady)
        {
            _logger.LogWarning("{DocumentId} is ready and fetching...", getReportResponse.DocumentId);
            var content = getReportResponse.Document;
            // Başka bir Business enstrüman kullanılarak yazma işlemi gerçekleştirilir
            var docContent = new DocumentSaveRequest
            {
                TraceId = appEvent.TraceId,
                EmployeeId = appEvent.EmployeeId,
                DocumentId = getReportResponse.DocumentId,
                Content = content,
            };
            var saveResponse = await _documentSaver.SaveAsync(docContent);
            _logger.LogInformation("Save response is {StatusCode} and message is {Message}"
            , saveResponse.StatusCode, saveResponse.Message);
        }
    }
}
