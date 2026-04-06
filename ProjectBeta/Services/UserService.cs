using System.ComponentModel.DataAnnotations;
using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Services;


public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public (bool Success, Dictionary<string, string[]>? FieldErrors) Register(
        string username,
        string email,
        string password,
        string firstName,
        string lastName,
        DateOnly? dateOfBirth
    )
    {
        if (Utils.ValidationHelper.AnyNullOrWhiteSpace(username, email, password, firstName, lastName) || dateOfBirth is null)
        {
            return (false, new Dictionary<string, string[]> { { "General", ["Please fill in all required fields."] } });
        }

        // Uniqueness checks
        if (_context.Users.Any(u => u.Username == username))
        {
            return (false, new Dictionary<string, string[]> { { "Username", ["Username is already taken. Please choose another."] } });
        }
        if (_context.Users.Any(u => u.Email == email))
        {
            return (false, new Dictionary<string, string[]> { { "Email", ["Email is already registered. Please use a different email."] } });
        }

        // TODO: Hash password securely
        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = password,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            IsActive = true,
            Role = "User" // enforce regular user role
        };

        // Model annotation validation
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(user);
        bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            user, validationContext, validationResults, true
        );
        if (!isValid)
        {
            var fieldErrors = validationResults
                .SelectMany(r => r.MemberNames.Select(m => new { Field = m, Error = r.ErrorMessage }))
                .GroupBy(x => x.Field)
                .ToDictionary(
                    g => g.Key,
                    g => g.Where(x => x.Error != null).Select(x => x.Error!).ToArray()
                );
            return (false, fieldErrors);
        }

        try
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return (true, null);
        }
        catch (Exception ex)
        {
            // Optionally log ex
            return (false, new Dictionary<string, string[]> { { "General", new[] { "An error occurred while saving the user. Please try again." } } });
        }
    }
    public (bool Success, User? User, Dictionary<string, string[]>? FieldErrors) SearchUser(
    string username,
    string email,
    string password
)
{
        // Validation for inputs
        if (Utils.ValidationHelper.AnyNullOrWhiteSpace(username, email, password))
        {
            return (false, null, new Dictionary<string, string[]> { { "General", new[] { "Please fill in all required fields." } } });
        }

        // Try to find user by either username or email
        var user = _context.Users
            .FirstOrDefault(u => u.Username == username || u.Email == email);

        if (user == null)
        {
            return (false, null, new Dictionary<string, string[]> { { "General", new[] { "User not found." } } });
        }

        // TODO: Compare the hashed password here
        if (user.PasswordHash != password) // In real use, you should compare hashed passwords, not plaintext.
        {
            return (false, null, new Dictionary<string, string[]> { { "General", new[] { "Invalid password." } } });
        }

        // Return user if all conditions are satisfied
        return (true, user, null);
    }

    public List<User> GetAllUsers()
    {
        return _context.Users.ToList();
    }
}
