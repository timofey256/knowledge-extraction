using Microsoft.AspNetCore.Mvc;
using KnowledgeExtractionTool.Infra.Services;
using KnowledgeExtractionTool.Infra.Services.InfraDomain;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        if (await _userService.RegisterAsync(model.Email, model.Password))
        {
            return Ok();
        }
        return BadRequest("Registration failed.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var token = await _userService.AuthenticateAsync(model.Email, model.Password);
        if (token != null)
        {
            return Ok(token); // Here, you might want to return a JWT or some other token
        }
        return Unauthorized();
    }
}
