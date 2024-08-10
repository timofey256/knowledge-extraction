namespace KnowledgeExtractionTool.Controllers;

using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KnowledgeExtractionTool.Infra.Services.Interfaces;
using KnowledgeExtractionTool.Infra.Services;

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

    [Authorize]
    [HttpPost("file-upload", Name = "FileUpload")]
    public async Task<ActionResult> UploadFile(IFormFile file)
    {
        var filePath = Path.GetTempFileName();

        if (file.Length > 0)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        return Ok(filePath);
    }
}