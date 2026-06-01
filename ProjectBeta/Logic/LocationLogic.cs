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
        if (!currentUser.IsSuperAdmin())
            throw new UnauthorizedAccessException("Only super admins can add locations.");

        _locationAccess.Add(location);
    }

    public void Delete(int id, User currentUser)
    {
        if (!currentUser.IsSuperAdmin())
            throw new UnauthorizedAccessException("Only super admins can delete locations.");

        _locationAccess.Delete(id);
    }

    public void UpdateName(int id, string newName, User currentUser)
    {
        if (!currentUser.IsSuperAdmin())
            throw new UnauthorizedAccessException("Only super admins can update locations.");

        _locationAccess.UpdateName(id, newName);
    }

    public void UpdateCity(int id, string newCity, User currentUser)
    {
        if (!currentUser.IsSuperAdmin())
            throw new UnauthorizedAccessException("Only super admins can update locations.");

        _locationAccess.UpdateCity(id, newCity);
    }

    public void UpdateAddress(int id, string newAddress, User currentUser)
    {
        if (!currentUser.IsSuperAdmin())
            throw new UnauthorizedAccessException("Only super admins can update locations.");

        _locationAccess.UpdateAddress(id, newAddress);
    }

    public List<Location> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _locationAccess.GetAll();

        return _locationAccess.Search(query);
    }
}
