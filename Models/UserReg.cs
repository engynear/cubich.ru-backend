using System.ComponentModel.DataAnnotations;

namespace cubichi.Models;

public class UserReg
{
    [Required, MinLength(3)]
    public string UserName { get; set; } = string.Empty;
    [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = string.Empty;
    [Required, Compare("Password", ErrorMessage = "Passwords do not match")]
    public string PasswordConfirm { get; set; } = string.Empty;
}