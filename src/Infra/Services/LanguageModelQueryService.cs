using System.Text;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace KnowledgeExtractionTool.Infra.Services;

/// <summary>
/// Sends requests to LLM provider and returns responses.  
/// </summary>
public class LanguageModelQueryService
{
    private readonly string _modelName;
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly ILogger<LanguageModelQueryService> _logger;

    public LanguageModelQueryService(IOptions<AppSettings> appSettingsOption, HttpClient httpClient, ILogger<LanguageModelQueryService> logger)
    {
        var appSettings = appSettingsOption.Value;
        _apiKey = appSettings.ApiKey;
        _modelName = appSettings.ModelName;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        _logger.Log(LogLevel.Information, "Sending request to LLM...");
        
        var requestBody = new
        {
            model = _modelName,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var requestJson = JsonConvert.SerializeObject(requestBody);
        var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsync("https://api.together.xyz/v1/chat/completions", httpContent);
        
        if (!response.IsSuccessStatusCode)
        {
            string errorMessage = $"Request failed with status code {response.StatusCode}";
            _logger.Log(LogLevel.Error, errorMessage);
            throw new HttpRequestException(errorMessage);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        dynamic responseObject = JsonConvert.DeserializeObject(responseContent);
        
        _logger.Log(LogLevel.Information, $"LLM response: {responseContent}");

        return responseObject.choices[0].message.content;
    }

    public string GetResponseSync(string prompt)
    {
        return Task.Run(() => GetResponseAsync(prompt)).GetAwaiter().GetResult();
    }
}
