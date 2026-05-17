using ProjectBeta.Access;
using ProjectBeta.Model;

namespace ProjectBeta.Logic;

public class BookingSnackLogic
{
    private readonly BookingSnackAccess _bookingSnackAccess;
    private readonly SnackAccess _snackAccess;

    public BookingSnackLogic(BookingSnackAccess bookingSnackAccess, SnackAccess snackAccess)
    {
        _bookingSnackAccess = bookingSnackAccess;
        _snackAccess = snackAccess;
    }

    public List<BookingSnack> GetAll()
    {
        return _bookingSnackAccess.GetAll();
    }
    public List<BookingSnack> GetAllByBookingId(int bookingId)
    {
        return _bookingSnackAccess.GetAllByBookingId(bookingId);
    }

    public BookingSnack? GetById(int id)
    {
        return _bookingSnackAccess.GetById(id);
    }

    public void Add(BookingSnack bookingSnack)
    {
        if (bookingSnack.BookedQuantity <= 0)
            throw new ArgumentException(l10n("booking_snacks.errors.quantity_positive"));

        var snack = _snackAccess.GetById(bookingSnack.SnackId);
        if (snack == null)
            throw new Exception(l10n("booking_snacks.errors.snack_not_found"));

        _bookingSnackAccess.Add(bookingSnack);
    }

    public void Update(BookingSnack bookingSnack, User currentUser)
    {
        if (!currentUser.IsAdmin())
            throw new UnauthorizedAccessException(l10n("booking_snacks.errors.update_unauthorized"));

        if (bookingSnack.BookedQuantity <= 0)
            throw new ArgumentException(l10n("booking_snacks.errors.quantity_positive"));

        _bookingSnackAccess.Update(bookingSnack);
    }

    public void Delete(int id, User currentUser)
    {
        if (!currentUser.IsAdmin())
            throw new UnauthorizedAccessException(l10n("booking_snacks.errors.delete_unauthorized"));

        var bookingSnack = _bookingSnackAccess.GetById(id);
        if (bookingSnack == null)
            throw new Exception(l10n("booking_snacks.errors.not_found"));

        _bookingSnackAccess.Delete(id);
    }
}
