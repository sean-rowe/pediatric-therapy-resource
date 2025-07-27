using System.ComponentModel.DataAnnotations;

namespace UPTRMS.Api.Models.DTOs;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}