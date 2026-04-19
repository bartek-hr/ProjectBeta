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

    public List<Auditorium> GetByCinemaId(int cinemaId)
    {
        return _auditoriumAccess.GetByCinemaId(cinemaId);
    }

    public void UpdateName(int auditoriumId, string newName, User currentUser)
    {
        if (currentUser.Role != "Admin")
            throw new UnauthorizedAccessException("Only admins can update auditorium names.");

        _auditoriumAccess.UpdateName(auditoriumId, newName);
    }

    public void UpdateCapacity(int auditoriumId, int newCapacity, User currentUser)
    {
        if (currentUser.Role != "Admin")
            throw new UnauthorizedAccessException("Only admins can update auditorium capacity.");

        _auditoriumAccess.UpdateCapacity(auditoriumId, newCapacity);
    }

    public void UpdateCinema(int auditoriumId, int newCinemaId, User currentUser)
    {
        if (currentUser.Role != "Admin")
            throw new UnauthorizedAccessException("Only admins can reassign auditoriums.");

        _auditoriumAccess.UpdateCinema(auditoriumId, newCinemaId);
    }
}
