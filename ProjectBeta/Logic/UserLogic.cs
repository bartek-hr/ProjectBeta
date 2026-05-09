using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Logic;


using ProjectBeta.Access;

public class UserLogic
{
    private readonly UserAccess _userAccess;
    private const string GeneralKey = "general";
    private const string IdentityKey = "identity";
    private const string UsernameKey = "username";
    private const string EmailKey = "email";

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
            return (false, CreateErrors(GeneralKey, l10n("auth.register.errors.required_fields")));
        }

        // Uniqueness checks
        if (_userAccess.UsernameExists(username))
        {
            return (false, CreateErrors(UsernameKey, l10n("auth.register.errors.username_taken")));
        }
        if (_userAccess.EmailExists(email))
        {
            return (false, CreateErrors(EmailKey, l10n("auth.register.errors.email_registered")));
        }

        // TODO: Hash password securely
        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = password,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth!.Value,
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
            var fieldErrors = NormalizeValidationErrors(validationResults);
            return (false, fieldErrors);
        }

        try
        {
            _userAccess.AddUser(user);
            return (true, null);
        }
        catch (Exception)
        {
            return (false, CreateErrors(GeneralKey, l10n("auth.register.errors.save_failed")));
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
            return (false, CreateErrors(GeneralKey, l10n("auth.login.errors.required_fields")), null);
        }

        var user = _userAccess.FindUserByUsernameOrEmail(username!, email!);

        if (user is null || user.PasswordHash != password)
        {
            return (false, CreateErrors(IdentityKey, l10n("auth.login.errors.invalid_credentials")), null);
        }

        return (true, null, user);
    }

    public (bool Success, List<User>? Users, Dictionary<string, string[]>? FieldErrors) GetAllUsers()
    {
        try
        {
            var users = _userAccess.GetAllUsers();

            if (users == null || !users.Any())
            {
                return (false, null, CreateErrors(GeneralKey, l10n("admin.users.errors.none_found")));
            }

            return (true, users, null);
        }
        catch (Exception)
        {
            return (false, null, CreateErrors(GeneralKey, l10n("admin.users.errors.load_failed")));
        }
    }

    public (bool Success, Dictionary<string, string[]>? FieldErrors) UpdateUser(
        int id,
        string username,
        string email,
        string? newPassword,
        string firstName,
        string lastName,
        DateOnly? dateOfBirth
    )
    {
        var user = _userAccess.GetUserById(id);
        if (user is null)
            return (false, CreateErrors(GeneralKey, l10n("account.profile.errors.user_not_found")));

        if (Utils.ValidationHelper.AnyNullOrWhiteSpace(username, email, firstName, lastName) || dateOfBirth is null)
            return (false, CreateErrors(GeneralKey, l10n("account.profile.errors.required_fields")));

        if (_userAccess.UsernameExistsForOther(username, id))
            return (false, CreateErrors(UsernameKey, l10n("account.profile.errors.username_taken")));

        if (_userAccess.EmailExistsForOther(email, id))
            return (false, CreateErrors(EmailKey, l10n("account.profile.errors.email_registered")));

        user.Username = username;
        user.Email = email;
        user.FirstName = firstName;
        user.LastName = lastName;
        user.DateOfBirth = dateOfBirth!.Value;

        if (!string.IsNullOrWhiteSpace(newPassword))
            user.PasswordHash = newPassword;

        // Model annotation validation
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(user);
        bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            user, validationContext, validationResults, true
        );
        if (!isValid)
        {
            var fieldErrors = NormalizeValidationErrors(validationResults);
            return (false, fieldErrors);
        }

        try
        {
            _userAccess.UpdateUser(user);
            return (true, null);
        }
        catch (Exception)
        {
            return (false, CreateErrors(GeneralKey, l10n("account.profile.errors.update_failed")));
        }
    }

    public (bool Success, Dictionary<string, string[]>? FieldErrors) DeleteUser(int id)
    {
        var user = _userAccess.GetUserById(id);
        if (user is null)
            return (false, CreateErrors(GeneralKey, l10n("account.profile.errors.user_not_found")));

        try
        {
            _userAccess.DeleteUser(user);
            return (true, null);
        }
        catch (Exception)
        {
            return (false, CreateErrors(GeneralKey, l10n("account.profile.errors.delete_failed")));
        }
    }

    private static Dictionary<string, string[]> CreateErrors(string key, params string[] messages)
    {
        return new Dictionary<string, string[]>
        {
            [key] = messages
        };
    }

    private static Dictionary<string, string[]> NormalizeValidationErrors(IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> validationResults)
    {
        return validationResults
            .SelectMany(result =>
            {
                var message = TranslateValidationError(result.ErrorMessage);
                return result.MemberNames.Select(memberName => new
                {
                    Field = NormalizeFieldKey(memberName),
                    Error = message
                });
            })
            .GroupBy(item => item.Field)
            .ToDictionary(
                group => group.Key,
                group => group.Select(item => item.Error).ToArray()
            );
    }

    private static string TranslateValidationError(string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return l10n("validation.common.invalid");
        }

        return errorMessage.StartsWith("validation.", StringComparison.Ordinal)
            ? l10n(errorMessage)
            : errorMessage;
    }

    private static string NormalizeFieldKey(string memberName)
    {
        return memberName switch
        {
            nameof(User.PasswordHash) => "password",
            _ => ToSnakeCase(memberName)
        };
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralKey;
        }

        var builder = new System.Text.StringBuilder();
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            if (char.IsUpper(character) && i > 0)
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }
}
