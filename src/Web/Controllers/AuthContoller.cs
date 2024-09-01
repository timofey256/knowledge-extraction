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

    /// <summary>
    /// Registers a new user with the provided email and password.
    /// Returns HTTP 200 OK on success, HTTP 400 Bad Request if the email is already used,
    /// or HTTP 500 Internal Server Error if there is an issue with MongoDB insertion.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model) {
        var registrationResult = await _userService.RegisterAsync(model.Email, model.Password);
        return registrationResult.Match<IActionResult>(
            success: () => Ok(),
            usedEmailError: () => BadRequest("Email is already used."),
            failedInsertionError: (errorMessage) => Problem($"Error occured while trying to insert in MongoDB. ErrorMessage:\n{errorMessage}")
        );
    }

    /// <summary>
    /// Authenticates a user and generates a JWT token if the credentials are valid.
    /// Stores the JWT token in cookies (not recommended for production) and returns it.
    /// Returns HTTP 200 OK with the token on success or HTTP 401 Unauthorized if authentication fails.
    /// </summary>
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
