namespace KnowledgeExtractionTool.Infra.Services.InfraDomain;

using AspNetCore.Identity.MongoDbCore.Models;
using System.ComponentModel.DataAnnotations;

public class RegisterModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}

public class LoginModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}
