namespace KnowledgeExtractionTool.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KnowledgeExtractionTool.Infra.Services.Interfaces;
using KnowledgeExtractionTool.Infra.Services;
using KnowledgeExtractionTool.Core.Domain;

[ApiController]
[Route("[controller]")]
public class KnowledgeExtractionController : ControllerBase
{
    private readonly ILogger<KnowledgeExtractionController> _logger;
    private readonly KnowledgeExtractorService _knowledgeExtractorService;

    public KnowledgeExtractionController(ILogger<KnowledgeExtractionController> logger, KnowledgeExtractorService knowledgeExtractorService)
    {
        _logger = logger;
        _knowledgeExtractorService = knowledgeExtractorService;
    }

    [Authorize]
    [HttpGet("build-graph", Name = "BuildKnowledgeGraph")]
    public ActionResult<KnowledgeGraph> BuildKnowledgeGraph(string text)
    {
        _logger.Log(LogLevel.Information, $"Got an BuildKnowledgeGraph request! Provided context length: {text.Length} characters");
        return Ok(_knowledgeExtractorService.ExtractKnowledgeGraph(text));
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