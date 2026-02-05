using System.ComponentModel.DataAnnotations;

namespace ECommerceInventory.Application.DTOs.Auth;

public record RegisterRequest(
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
    string Email,
    
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&].*$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    string Password,
    
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters.")]
    string FirstName,
    
    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters.")]
    string LastName);
