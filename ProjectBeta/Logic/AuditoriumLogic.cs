using ProjectBeta.Access;
using ProjectBeta.Model;

namespace ProjectBeta.Logic;

public class AuditoriumLogic
{
    private readonly AuditoriumAccess _auditoriumAccess;

    public AuditoriumLogic(AuditoriumAccess auditoriumAccess)
    {
        _auditoriumAccess = auditoriumAccess;
    }

    public List<Auditorium> GetAll()
    {
        return _auditoriumAccess.GetAll();
    }

    public Auditorium? GetById(int id)
    {
        return _auditoriumAccess.GetById(id);
    }

    public List<Auditorium> GetByLocationId(int locationId)
    {
        return _auditoriumAccess.GetByLocationId(locationId);
    }

    public void UpdateName(int auditoriumId, string newName, User currentUser)
    {
        if (!currentUser.IsAdmin())
            throw new UnauthorizedAccessException(l10n("admin.auditoriums.edit.errors.update_name_unauthorized"));

        _auditoriumAccess.UpdateName(auditoriumId, newName);
    }

    public void UpdateCapacity(int auditoriumId, int newCapacity, User currentUser)
    {
        if (!currentUser.IsAdmin())
            throw new UnauthorizedAccessException(l10n("admin.auditoriums.edit.errors.update_capacity_unauthorized"));

        _auditoriumAccess.UpdateCapacity(auditoriumId, newCapacity);
    }

    public void UpdateLocation(int auditoriumId, int newLocationId, User currentUser)
    {
        if (!currentUser.IsAdmin())
            throw new UnauthorizedAccessException(l10n("admin.auditoriums.edit.errors.reassign_unauthorized"));

        _auditoriumAccess.UpdateLocation(auditoriumId, newLocationId);
    }

    public void Add(Auditorium auditorium, User currentUser)
    {
        if (!currentUser.IsAdmin())
            throw new UnauthorizedAccessException("Only admins can add auditoriums.");

        if (string.IsNullOrWhiteSpace(auditorium.Name))
            throw new ArgumentException("Name is required.");

        if (auditorium.Capacity <= 0)
            throw new ArgumentException("Capacity must be greater than 0.");

        _auditoriumAccess.Add(auditorium);
    }

    public void Delete(int auditoriumId, User currentUser)
    {
        if (!currentUser.IsAdmin())
            throw new UnauthorizedAccessException("Only admins can delete auditoriums.");

        _auditoriumAccess.Delete(auditoriumId);
    }
}
