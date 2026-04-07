using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Logic;


using ProjectBeta.Access;

public class UserLogic
{
    private readonly UserAccess _userAccess;

    public UserLogic(UserAccess userAccess)
    {
        _userAccess = userAccess;
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
        if (_userAccess.UsernameExists(username))
        {
            return (false, new Dictionary<string, string[]> { { "Username", ["Username is already taken. Please choose another."] } });
        }
        if (_userAccess.EmailExists(email))
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
            _userAccess.AddUser(user);
            return (true, null);
        }
        catch (Exception)
        {
            return (false, new Dictionary<string, string[]> { { "General", new[] { "An error occurred while saving the user. Please try again." } } });
        }
    }

    public (bool Success, Dictionary<string, string[]>? FieldErrors, User? User) SearchUser(
        string? username,
        string? email,
        string? password
    )
    {
        if (username is null || email is null || password is null || Utils.ValidationHelper.AnyNullOrWhiteSpace(username, email, password))
        {
            return (false, new Dictionary<string, string[]> { { "General", ["Please fill in all required fields."] } }, null);
        }

        var user = _userAccess.FindUserByUsernameOrEmail(username!, email!);

        if (user is null || user.PasswordHash != password)
        {
            return (false, new Dictionary<string, string[]> { { "General", ["Invalid credentials."] } }, null);
        }

        return (true, null, user);
    }
}
