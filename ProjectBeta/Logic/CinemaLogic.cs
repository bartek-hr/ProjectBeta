using ProjectBeta.Access;
using ProjectBeta.Model;

namespace ProjectBeta.Logic;

public class CinemaLogic
{
    private readonly CinemaAccess _cinemaAccess;

    public CinemaLogic(CinemaAccess cinemaAccess)
    {
        _cinemaAccess = cinemaAccess;
    }

    public List<Cinema> GetAll()
    {
        return _cinemaAccess.GetAll();
    }

    public Cinema? GetById(int id)
    {
        return _cinemaAccess.GetById(id);
    }

    public void UpdateName(int cinemaId, string newName, User currentUser)
    {
        if (currentUser.Role != "Admin")
            throw new UnauthorizedAccessException(l10n("admin.cinemas.edit.errors.update_name_unauthorized"));

        _cinemaAccess.UpdateName(cinemaId, newName);
    }

    public void UpdateCity(int cinemaId, string newCity, User currentUser)
    {
        if (currentUser.Role != "Admin")
            throw new UnauthorizedAccessException(l10n("admin.cinemas.edit.errors.update_city_unauthorized"));

        _cinemaAccess.UpdateCity(cinemaId, newCity);
    }
}
