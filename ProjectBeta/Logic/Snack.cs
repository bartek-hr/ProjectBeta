using ProjectBeta.Access;
using ProjectBeta.Model;

namespace ProjectBeta.Logic;

public class SnackLogic
{
    private readonly SnackAccess _snackAccess;

    public SnackLogic(SnackAccess snackAccess)
    {
        _snackAccess = snackAccess;
    }

    public List<Snack> GetAll()
    {
        return _snackAccess.GetAll();
    }

    public Snack? GetById(int id)
    {
        return _snackAccess.GetById(id);
    }

    public void Add(Snack snack, User currentUser)
    {
        if (currentUser.Role != "Admin" && currentUser.Role != "SuperAdmin")
            throw new UnauthorizedAccessException("Only admins can add snacks.");

        if (string.IsNullOrWhiteSpace(snack.Name))
            throw new ArgumentException("Name is required.");

        if (snack.Price < 0)
            throw new ArgumentException("Price cannot be negative.");

        if (snack.Quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.");

        _snackAccess.Add(snack);
    }

    public void Update(Snack snack, User currentUser)
    {
        if (currentUser.Role != "Admin" && currentUser.Role != "SuperAdmin")
            throw new UnauthorizedAccessException("Only admins can update snacks.");

        if (string.IsNullOrWhiteSpace(snack.Name))
            throw new ArgumentException("Name is required.");

        if (snack.Price < 0)
            throw new ArgumentException("Price cannot be negative.");

        if (snack.Quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.");

        _snackAccess.Update(snack);
    }

    public void Delete(int id, User currentUser)
    {
        if (currentUser.Role != "Admin" && currentUser.Role != "SuperAdmin")
            throw new UnauthorizedAccessException("Only admins can delete snacks.");

        var snack = _snackAccess.GetById(id);
        if (snack == null)
            throw new Exception("Snack not found.");

        _snackAccess.Delete(id);
    }
}
