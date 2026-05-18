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
    private int _locationId;

    private MovieSchedule _movie;
    private readonly BookingLogic _bookingLogic;
    private Auditorium _auditorium;
    private HashSet<string>? _reservedSeats;

    public MovieSeatBookingView(BookingLogic bookingLogic, IServiceProvider serviceProvider, AppLoop appLoop)
    {
        _user = new User();
        _appLoop = appLoop;
        _serviceProvider = serviceProvider;
        _bookingLogic = bookingLogic;
    }

    private HashSet<string>? GetReservedSeats()
    {
        List<Booking> reservations = _bookingLogic.GetBookingsByCreatedAtAndAuditoriumID(_movie.ScheduleDate.ToDateTime(_movie.StartTime), _auditorium.Id);
        return reservations
            .Where(b => !string.IsNullOrWhiteSpace(b.Seats))
            .SelectMany(b => b.Seats.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(s => s.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
    private SeatState GetSeatState(string seat)
    {
        return _reservedSeats.Contains(seat)
            ? SeatState.Reserved
            : SeatState.Available;
    }
    public void SetView(User user, MovieSchedule movie, Auditorium auditorium, int locationId)
    {
        _user = user;
        _movie = movie;
        _locationId = locationId;
        _auditorium = auditorium;
        _reservedSeats = GetReservedSeats();
        ClearChildren();
        InitializeForm();
    }
    private void InitializeForm()
    {
        Heading(l10n("movies.seat_booking.heading"));
        Label(l10n("movies.seat_booking.tagline"));
        Label(l10n("movies.seat_booking.instructions"));
        Divider();
        var seatMap = new SeatMap(
            auditoriumName: _auditorium.Name,
            movieTitle: _movie.Movie.Title,
            showtime: $"{_movie.ScheduleDate:yyyy-MM-dd} {_movie.StartTime:HH:mm}",
            capacity: $"{_auditorium.Capacity}"
        );
        if (_auditorium.Capacity == 150){
            BuildAuditorium(seatMap, 'N');
        }
        if (_auditorium.Capacity == 300){
            BuildAuditorium(seatMap, 'S');
        }
        if (_auditorium.Capacity == 500){
            BuildAuditorium(seatMap, 'T');
        }        
        Button("Confirm selected seats").OnClick(() =>         
        {
            Console.Clear();
            var reservationView = _serviceProvider.GetRequiredService<ReservationView>();
            reservationView.SetView(_user, _movie, seatMap._selectedSeats, seatMap._selectedTypes, _auditorium.Id, _locationId);
            _appLoop.Display(reservationView);
        });
        Button(l10n("movies.seat_booking.actions.back")).OnClick(() => Close());
    }
    
    public void BuildAuditorium(SeatMap seatMap, char EndRow)
    {
        char startRow = 'A';
        char endRow = EndRow;
        for (char row = startRow; row <= endRow; row++)
        {  
            seatMap = BuildSeats(row, seatMap);  
        }
        Add(seatMap);
        Spacer();
        Message(() => seatMap.StatusMessage);
    }


    public SeatMap BuildSeats(char row, SeatMap seatMap)
    {
        (int StartSeat, int EndSeat) StartEndSeats = DetermineStartEndSeatsForAuditorium(row);
        var seats = new List<SeatState>();
        for (int i = StartEndSeats.StartSeat; i <= StartEndSeats.EndSeat; i++)
        {
            string seatLabel = $"{row}{i}";
            if (_reservedSeats == null || _reservedSeats.Count == 0)
            {
                seats.Add(SeatState.Available);
            }
            else
            {
                seats.Add(GetSeatState(seatLabel));
            }
        }
        seatMap.AddRow(row, seats.ToArray());
        return seatMap;
    }
    public (int StartSeat, int EndSeat) DetermineStartEndSeatsForAuditorium(char row)
    {
        if (_auditorium.Capacity == 500) {
            return DetermineStartEndSeatsForBigAuditorium(row);
        }else if(_auditorium.Capacity == 300) {
            return DetermineStartEndSeatsForMediumAuditorium(row);
        }
        return DetermineStartEndSeatsForSmallAuditorium(row);
    }
    public (int StartSeat, int EndSeat) DetermineStartEndSeatsForSmallAuditorium(char row)
    {
        return row switch
        {
            'A' or 'N' => (3, 10),
            'B' or 'C' or 'L' or 'M' => (2, 11),
            _ => (1, 12)
        };
    }
    public (int StartSeat, int EndSeat) DetermineStartEndSeatsForMediumAuditorium(char row)
    {
        return row switch
        {
            'A' or 'B' or 'C'or 'D' or 'E' or 'F' or 'L' or 'M' or 'N'  => (2, 17),
            'O' or 'P' or 'Q' => (3, 16),
            'R' or 'S' => (4, 15),
            _ => (1, 18)
        };
    }
    public (int StartSeat, int EndSeat) DetermineStartEndSeatsForBigAuditorium(char row)
    {
        return row switch
        {
            'A'  => (5, 26),
            'R' => (6, 25),
            'S' => (8, 23),
            'T' => (9, 22),
            'B' or 'C' or 'D' or 'E' or 'P' or 'Q' => (4, 27),
            'F' or 'N' or 'O' => (3, 28),
            'G' or 'M' => (2, 29),
            _ => (1, 30)
        };
    }
}
