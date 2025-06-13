using System.ComponentModel.DataAnnotations;

namespace TherapyDocs.Api.Models.DTOs;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 12)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(2)]
    public string LicenseState { get; set; } = string.Empty;

    [Required]
    public string LicenseType { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    public bool AcceptedTerms { get; set; }
}