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
            throw new UnauthorizedAccessException("Only admins can update cinema names.");

        _cinemaAccess.UpdateName(cinemaId, newName);
    }

    public void UpdateCity(int cinemaId, string newCity, User currentUser)
    {
        if (currentUser.Role != "Admin")
            throw new UnauthorizedAccessException("Only admins can update cinema city.");

        _cinemaAccess.UpdateCity(cinemaId, newCity);
    }
}
