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

    public bool Register(string username, string password)
    {
        bool exists = _context.Users.Any(u => u.Username == username);
        if (exists) return false;

        var user = new User { Username = username, PasswordHash = password };
        _context.Users.Add(user);
        _context.SaveChanges();
        return true;
    }

    public List<User> GetAllUsers()
    {
        return _context.Users.ToList();
    }
}
