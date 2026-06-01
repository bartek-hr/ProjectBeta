namespace ProjectBeta.Model;

using System.ComponentModel.DataAnnotations;

public class User
{
    private const string AdminRole = "Admin";
    private const string UserRole = "User";
    private const string SuperAdminRole = "SuperAdmin";
    private const string SuperAdminUsername = "admin";

    public int Id { get; set; }

    [Required(ErrorMessage = "validation.user.username.required")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "validation.user.username.length")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "validation.user.username.pattern")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "validation.user.password.required")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required(ErrorMessage = "validation.user.role.required")]
    public string Role { get; set; } = "User";

    [Required(ErrorMessage = "validation.user.email.required")]
    [EmailAddress(ErrorMessage = "validation.user.email.invalid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "validation.user.first_name.required")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "validation.user.last_name.required")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "validation.user.date_of_birth.required")]
    public DateOnly DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool HasSubscription { get; set; } = false;
    public int? SubscriptionSeatType { get; set; }

    public bool IsSuperAdmin()
    {
        return string.Equals(Role, SuperAdminRole, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(Username, SuperAdminUsername, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsAdmin()
    {
        return IsSuperAdmin() || string.Equals(Role, AdminRole, StringComparison.OrdinalIgnoreCase);
    }
}
