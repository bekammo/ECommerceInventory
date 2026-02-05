using System.ComponentModel.DataAnnotations;

namespace ECommerceInventory.Application.DTOs.Auth;

public record LoginRequest(
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    string Email,
    
    [Required(ErrorMessage = "Password is required.")]
    string Password,
    
    string? DeviceInfo,
    string? IpAddress);
