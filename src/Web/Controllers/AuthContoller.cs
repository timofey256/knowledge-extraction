using Microsoft.AspNetCore.Mvc;
using KnowledgeExtractionTool.Infra.Services;
using KnowledgeExtractionTool.Infra.Services.InfraDomain;
using Microsoft.Extensions.Options;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
    private readonly UserService _userService;
    private readonly HttpContext? _httpContext;
    private readonly JwtOptions _jwtOptions;

    public AuthController(UserService userService, IHttpContextAccessor httpContextAccessor, IOptions<JwtOptions> jwtOptions) {
        if (httpContextAccessor.HttpContext is null)
            new NullReferenceException("AuthController: HttpContext is null!");

        _userService = userService;
        _httpContext = httpContextAccessor.HttpContext;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model) {
        var registrationResult = await _userService.RegisterAsync(model.Email, model.Password);
        return registrationResult.Match<IActionResult>(
            success: () => Ok(),
            usedEmailError: () => BadRequest("Email is already used."),
            failedInsertionError: (errorMessage) => Problem($"Error occured while trying to insert in MongoDB. ErrorMessage:\n{errorMessage}")
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model) {
        var token = await _userService.AuthenticateAsync(model.Email, model.Password);
        if (token != null)
        {
            // Here for simplicity we are storing JWT token in cookies
            // which is obviously a bad practice...
            // Should be redone later if any extra time available
            _httpContext.Response.Cookies.Append(_jwtOptions.CookiesKey, token);

            return Ok(token);
        }
        return Unauthorized();
    }
}
