using ProjectBeta.CI;
using ProjectBeta.CI.Views;
using ProjectBeta.CI.Components;
using ProjectBeta.Logic;
using ProjectBeta.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ProjectBeta.CI.Views;

public sealed class MovieSeatBookingView : Form
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AppLoop _appLoop;
    private User _user;
    private MovieSchedule _movie;
    private readonly BookingLogic _bookingLogic;

    public MovieSeatBookingView(BookingLogic bookingLogic, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _user = new User();
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
        _bookingLogic = bookingLogic;

    }
    
    public bool isNotReserved(List<Booking> Reservations, string SeatRow)
    {
        return false;
    }

    private HashSet<string> GetReservedSeats(List<Booking> bookings)
    {
        return bookings
            .Where(b => !string.IsNullOrWhiteSpace(b.Seats))
            .SelectMany(b => b.Seats.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(s => s.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
    private SeatState GetSeatState(string seat, HashSet<string> reservedSeats)
    {
        return reservedSeats.Contains(seat)
            ? SeatState.Reserved
            : SeatState.Available;
    }
    public void SetView(User user, MovieSchedule movie)
    {
        _user = user;
        _movie = movie;
        ClearChildren();
        InitializeForm();
    }
    private void InitializeForm()
    {
        Heading(l10n("movies.seat_booking.heading"));
        Label(l10n("movies.seat_booking.tagline"));
        Label(l10n("movies.seat_booking.instructions"));
        Divider();
        DateTime startDateTime = _movie.ScheduleDate.ToDateTime(_movie.StartTime);
        List<Booking> reservations = _bookingLogic.GetBookingsByCreatedAt(startDateTime);
        var reservedSeats = GetReservedSeats(reservations);

        var seatMap = new SeatMap(
            auditoriumName: l10n("movies.seat_booking.auditorium"),
            movieTitle: _movie.Movie.Title,
            showtime: $"{_movie.ScheduleDate:yyyy-MM-dd} {_movie.StartTime:HH:mm}"
        );

        char startRow = 'A';
        char endRow = 'N';

        for (char row = startRow; row <= endRow; row++)
        {
            int startSeat;
            int endSeat;

            if (row == 'A' || row == 'N')
            {
                startSeat = 3;
                endSeat = 10;
            }
            else if (row == 'B' || row == 'C' || row == 'L' || row == 'M')
            {
                startSeat = 2;
                endSeat = 11;
            }
            else
            {
                startSeat = 1;
                endSeat = 12;
            }

            var seats = new List<SeatState>();

            for (int i = startSeat; i <= endSeat; i++)
            {
                string seatLabel = $"{row}{i}";
                if (reservations == null || reservations.Count == 0)
                {
                    seats.Add(SeatState.Available);
                }
                else
                {
                    seats.Add(GetSeatState(seatLabel, reservedSeats));
                }
            }

            seatMap.AddRow(row, seats.ToArray());
        }
        Add(seatMap);
        Spacer();
        Message(() => seatMap.StatusMessage);
        Button(l10n("movies.seat_booking.actions.snacks")).OnClick(() => Close());
        Button(l10n("movies.seat_booking.actions.confirm")).OnClick(() =>         
        {
            Console.Clear();
            var reservationView = _serviceProvider.GetRequiredService<ReservationView>();
            reservationView.SetView(_user, _movie, seatMap._selectedSeats, seatMap._selectedTypes);
            _appLoop.Display(reservationView);
        });
        Button(l10n("movies.seat_booking.actions.back")).OnClick(() => Close());
    }
}
