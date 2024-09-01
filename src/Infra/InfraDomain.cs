namespace KnowledgeExtractionTool.Infra.Services.InfraDomain;

using System.ComponentModel.DataAnnotations;

public class RegisterRequest {
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}

public class LoginRequest {
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}
