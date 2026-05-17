using ProjectBeta.Access;
using ProjectBeta.Model;

namespace ProjectBeta.Logic;

public class LocationLogic
{
    private readonly LocationAccess _locationAccess;

    public LocationLogic(LocationAccess locationAccess)
    {
        _locationAccess = locationAccess;
    }

    public List<Location> GetAll()
    {
        return _locationAccess.GetAll();
    }

    public Location? GetById(int id)
    {
        return _locationAccess.GetById(id);
    }

    public void Add(Location location, User currentUser)
    {
        if (!currentUser.IsAdmin())
            throw new UnauthorizedAccessException("Only admins can add locations.");

        _locationAccess.Add(location);
    }

    public void Delete(int id, User currentUser)
    {
        if (!currentUser.IsAdmin())
            throw new UnauthorizedAccessException("Only admins can delete locations.");

        _locationAccess.Delete(id);
    }

    public void UpdateCapacity(int id, int newCapacity, User currentUser)
    {
        if (!currentUser.IsAdmin())
            throw new UnauthorizedAccessException("Only admins can update location capacity.");

        _locationAccess.UpdateCapacity(id, newCapacity);
    }

    public List<Location> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _locationAccess.GetAll();

        return _locationAccess.Search(query);
    }
}
