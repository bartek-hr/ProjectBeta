using ProjectBeta.Data;
using ProjectBeta.Model;

namespace ProjectBeta.Access;

public class UserAccess
{
    private readonly AppDbContext _context;

    public UserAccess(AppDbContext context)
    {
        _context = context;
    }

    public bool UsernameExists(string username)
    {
        return _context.Users.Any(u => u.Username == username);
    }

    public bool EmailExists(string email)
    {
        return _context.Users.Any(u => u.Email == email);
    }

    public void AddUser(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public User? FindUserByUsernameOrEmail(string username, string email)
    {
        return _context.Users.FirstOrDefault(u => u.Username == username || u.Email == email);
    }

    public List<User> GetAllUsers()
    {
        return _context.Users.ToList();
    }
}
