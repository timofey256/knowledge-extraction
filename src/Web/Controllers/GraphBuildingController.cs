namespace KnowledgeExtractionTool.Controllers;

using Microsoft.AspNetCore.Mvc;
using KnowledgeExtractionTool.Infra.Services.Interfaces;
using KnowledgeExtractionTool.Infra.Services;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly ITextProcessorService _textProcessorService;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, ITextProcessorService textProcessor)
    {
        _logger = logger;
        _textProcessorService = textProcessor;
    }

    [HttpGet(Name = "ProcessText")]
    public string Get(string text)
    {
        _logger.Log(LogLevel.Information, "Hit someEndpoint endpoint!");
        return _textProcessorService.Process(text);
    }
}