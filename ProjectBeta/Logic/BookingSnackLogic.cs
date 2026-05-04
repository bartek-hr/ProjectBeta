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

    public BookingSnack? GetById(int id)
    {
        return _bookingSnackAccess.GetById(id);
    }

    public void Add(BookingSnack bookingSnack, User currentUser)
    {
        if (currentUser.Role != "Admin" && currentUser.Role != "SuperAdmin")
            throw new UnauthorizedAccessException("Only admins can add booking snacks.");

        if (bookingSnack.BookedQuantity <= 0)
            throw new ArgumentException("Booked quantity must be greater than zero.");

        var snack = _snackAccess.GetById(bookingSnack.SnackId);
        if (snack == null)
            throw new Exception("Snack not found.");

        _bookingSnackAccess.Add(bookingSnack);
    }

    public void Update(BookingSnack bookingSnack, User currentUser)
    {
        if (currentUser.Role != "Admin" && currentUser.Role != "SuperAdmin")
            throw new UnauthorizedAccessException("Only admins can update booking snacks.");

        if (bookingSnack.BookedQuantity <= 0)
            throw new ArgumentException("Booked quantity must be greater than zero.");

        _bookingSnackAccess.Update(bookingSnack);
    }

    public void Delete(int id, User currentUser)
    {
        if (currentUser.Role != "Admin" && currentUser.Role != "SuperAdmin")
            throw new UnauthorizedAccessException("Only admins can delete booking snacks.");

        var bookingSnack = _bookingSnackAccess.GetById(id);
        if (bookingSnack == null)
            throw new Exception("BookingSnack not found.");

        _bookingSnackAccess.Delete(id);
    }
}
