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
    public List<Snack> GetAllByLocationId(int locationId)
    {
        return _snackAccess.GetAllByLocationId(locationId);
    }

    public List<Snack> Search(int locationId, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _snackAccess.GetAllByLocationId(locationId);

        return _snackAccess.Search(locationId, query);
    }
    public Snack? GetById(int id)
    {
        return _snackAccess.GetById(id);
    }

    public void Add(Snack snack, User currentUser)
    {
        if (!currentUser.IsAdmin())
            throw new UnauthorizedAccessException(l10n("snacks.errors.add_unauthorized"));

        if (string.IsNullOrWhiteSpace(snack.Name))
            throw new ArgumentException(l10n("snacks.errors.name_required"));

        if (snack.Price < 0)
            throw new ArgumentException(l10n("snacks.errors.price_non_negative"));

        if (snack.Quantity < 0)
            throw new ArgumentException(l10n("snacks.errors.quantity_non_negative"));

        _snackAccess.Add(snack);
    }

    public void Update(Snack snack, User currentUser)
    {
        if (!currentUser.IsAdmin())
            throw new UnauthorizedAccessException(l10n("snacks.errors.update_unauthorized"));

        if (string.IsNullOrWhiteSpace(snack.Name))
            throw new ArgumentException(l10n("snacks.errors.name_required"));

        if (snack.Price < 0)
            throw new ArgumentException(l10n("snacks.errors.price_non_negative"));

        if (snack.Quantity < 0)
            throw new ArgumentException(l10n("snacks.errors.quantity_non_negative"));

        _snackAccess.Update(snack);
    }

    public void Delete(int id, User currentUser)
    {
        if (!currentUser.IsAdmin())
            throw new UnauthorizedAccessException(l10n("snacks.errors.delete_unauthorized"));

        var snack = _snackAccess.GetById(id);
        if (snack == null)
            throw new Exception(l10n("snacks.errors.not_found"));

        _snackAccess.Delete(id);
    }
}
