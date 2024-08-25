namespace KnowledgeExtractionTool.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KnowledgeExtractionTool.Infra.Services;
using KnowledgeExtractionTool.Core.Domain;
using System.Text;
using Microsoft.Extensions.Options;
using KnowledgeExtractionTool.Data;
using KnowledgeExtractionTool.Data.Types;

[ApiController]
[Route("[controller]")]
public class KnowledgeExtractionController : ControllerBase
{
    private readonly ILogger<KnowledgeExtractionController> _logger;
    private readonly KnowledgeExtractorService _knowledgeExtractorService;
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly UserService _userService;
    private readonly GraphRepository _graphRepository;

    public KnowledgeExtractionController(ILogger<KnowledgeExtractionController> logger, 
                                         KnowledgeExtractorService knowledgeExtractorService,
                                         UserService userService,
                                         GraphRepository graphRepository,
                                         IOptions<JwtOptions> jwtOptions)
    {
        _logger = logger;
        _knowledgeExtractorService = knowledgeExtractorService;
        _jwtOptions = jwtOptions;
        _userService = userService;
        _graphRepository = graphRepository;
    }

    /// <summary>
    /// Builds a knowledge graph from the provided text context and stores it.
    /// </summary>
    /// <param name="text">The context text from which to extract the knowledge graph.</param>
    /// <returns>A KnowledgeGraph object representing the extracted knowledge.</returns>
    [Authorize]
    [HttpGet("build-graph", Name = "BuildKnowledgeGraph")]
    public async Task<ActionResult<KnowledgeGraph>> BuildKnowledgeGraph(string text) {
        _logger.Log(LogLevel.Information, $"Got an BuildKnowledgeGraph request! Provided context length is {text.Length} characters long.");

        var IdResult = GetUserIdFromRequest();
        if (!IdResult.IsSuccess && IdResult.Value is null) {
            BadRequest($"Error occured: {IdResult.ErrorMessage}");
        }
        string? id = IdResult.Value;
        
        _logger.Log(LogLevel.Information, $"Successfuly got UserId from JWT Token: {id}.");

        var graph = _knowledgeExtractorService.ExtractKnowledgeGraph(id, text);
        StorageResult result = await _graphRepository.TryInsert(graph);
        
        LogGraphStorageResult(result);
        return Ok(graph);
    }

    /// <summary>
    /// Retrieves all knowledge graphs associated with the user.
    /// </summary>
    /// <returns>A list of KnowledgeGraph objects associated with the user.</returns>
    [Authorize]
    [HttpGet("get-user-graphs", Name = "Get All User's Graphs")]
    public async Task<ActionResult<List<KnowledgeGraph>>> GetUserGraphs(string text) {
        _logger.Log(LogLevel.Information, $"Got an GetUserGraphs request.");
        
        var IdResult = GetUserIdFromRequest();
        if (!IdResult.IsSuccess) {
            BadRequest($"Error occured: {IdResult.ErrorMessage}");
        }
        
        var graphs = await _graphRepository.FindByOwnerIdAsync(IdResult.Value);
        return Ok(graphs);
    }

    /// <summary>
    /// Uploads a file, extracts knowledge from its content, and returns the knowledge graph.
    /// </summary>
    /// <param name="file">The file to upload and process.</param>
    /// <returns>An ActionResult containing the extracted knowledge graph.</returns>
    [Authorize]
    [HttpPost("file-upload", Name = "FileUpload")]
    public async Task<ActionResult> UploadFile(IFormFile file) {
        // Currently, we save these files to /tmp location on the server.
        // It is obviously far from a good solution. Better way to store them would probably be using some S3.
        // But for simplicity let's keep it simple for now.
        var tempFilePath = Path.GetTempFileName();

        if (file is not null && file.Length > 0)
        {
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
                await file.CopyToAsync(stream);
        }
        else {
            return BadRequest("No file uploaded or the file is empty.");
        }

        byte[] fileContent;
        using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
        {
            fileContent = new byte[fileStream.Length];
            await fileStream.ReadAsync(fileContent, 0, (int)fileStream.Length);
        }

        // Do we want to delete a file?? Not sure.
        // System.IO.File.Delete(tempFilePath);

        var contentAsString = Encoding.UTF8.GetString(fileContent);

        var jwtToken = Request.Cookies[_jwtOptions.Value.CookiesKey];  

        if (jwtToken is null)  // If user was autorized this shouldn't be null.
            throw new Exception("Unexpected behaviour. User has to have its JWT token in cookies.");

        var id = _userService.GetUserIdByToken(jwtToken);

        return Ok(_knowledgeExtractorService.ExtractKnowledgeGraph(id, contentAsString));
    }

    private Result<string> GetUserIdFromRequest() {
        _logger.Log(LogLevel.Information, $"Got an GetUserGraphs request.");
        var jwtToken = Request.Cookies[_jwtOptions.Value.CookiesKey];  

        if (jwtToken is null)  {// If user was autorized this shouldn't be null.
            string message = "Unexpected behaviour. User has to have its JWT token in cookies.";
            _logger.Log(LogLevel.Error, message);
            return Result<string>.Failure(message);
        }

        var id = _userService.GetUserIdByToken(jwtToken);
        if (id is null) {
            string message = "Unexpected behaviour. User has to have its JWT token in cookies.";
            _logger.Log(LogLevel.Error, message);
            return Result<string>.Failure(message);
        }
        _logger.Log(LogLevel.Information, $"Successfuly got UserId from JWT Token: {id}.");

        return Result<string>.Success(id);
    }

    private void LogGraphStorageResult(StorageResult result) {
        if (result.MongoResult.Type == OperationResultType.Error) {
            _logger.Log(LogLevel.Error, $"Failed to store graph in Mongo. Error message: {result.MongoResult.ErrorMessage}");
        }
        else if (result.MongoResult.Type == OperationResultType.Success) {
            _logger.Log(LogLevel.Information, "Successfuly stored graph in Mongo");
        }

        if (result.MemcachedResult.Type == OperationResultType.Error) {
            _logger.Log(LogLevel.Error, $"Failed to store graph in Memcached. Error message: {result.MongoResult.ErrorMessage}");
        }
        else if (result.MongoResult.Type == OperationResultType.Success) {
            _logger.Log(LogLevel.Information, "Successfuly stored graph in Memcached");
        }
    }
}