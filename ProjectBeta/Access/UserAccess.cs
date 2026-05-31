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

    public User? GetUserById(int id)
    {
        return _context.Users.Find(id);
    }

    public bool UsernameExistsForOther(string username, int excludeId)
    {
        return _context.Users.Any(u => u.Username == username && u.Id != excludeId);
    }

    public bool EmailExistsForOther(string email, int excludeId)
    {
        return _context.Users.Any(u => u.Email == email && u.Id != excludeId);
    }

    public void UpdateUser(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }

    public void DeleteUser(User user)
    {
        _context.Users.Remove(user);
        _context.SaveChanges();
    }
}
