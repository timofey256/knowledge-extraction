namespace KnowledgeExtractionTool.Controllers;

using Microsoft.AspNetCore.Mvc;
using KnowledgeExtractionTool.Infra.Services.Interfaces;
using KnowledgeExtractionTool.Infra.Services;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("[controller]")]
public class KnowledgeExtractionController : ControllerBase
{
    private readonly ILogger<KnowledgeExtractionController> _logger;
    private readonly ITextProcessorService _textProcessorService;

    public KnowledgeExtractionController(ILogger<KnowledgeExtractionController> logger, ITextProcessorService textProcessor)
    {
        _logger = logger;
        _textProcessorService = textProcessor;
    }

    [Authorize]
    [HttpGet("process-text", Name = "ProcessText")]
    public IActionResult GetProcessedText(string text)
    {
        _logger.Log(LogLevel.Information, "Hit someEndpoint endpoint!");
        return Ok(_textProcessorService.Process(text));
    }

    [Authorize]
    [HttpGet("dummy-endpoint", Name = "Dummy")]
    public IActionResult DummyGet(string text)
    {
        return Ok("Autorized");
    }
}