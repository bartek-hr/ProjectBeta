using ProjectBeta.CI.Components;

namespace ProjectBeta.CI.Views;

public sealed class MovieSeatBookingView : Form
{
    public MovieSeatBookingView()
    {
        Heading("The Grand Cinema");
        Label("A premium seat picker for the movie reservation flow.");
        Label("Tab to focus the buttons. Escape exits the app.");
        Divider();

        var seatMap = new SeatMap(
            auditoriumName: "Hall 1 · Dolby Atmos",
            movieTitle: "Interstellar Re-Release",
            showtime: "Tonight at 19:30")
            .AddRow('A', SeatState.Reserved, SeatState.Reserved, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Reserved, SeatState.Available)
            .AddRow('B', SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Reserved, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Available)
            .AddRow('C', SeatState.Available, SeatState.Reserved, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Reserved, SeatState.Available, SeatState.Available)
            .AddRow('D', SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Reserved, SeatState.Available, SeatState.Available, SeatState.Reserved)
            .AddRow('E', SeatState.Reserved, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Reserved, SeatState.Available)
            .AddRow('F', SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Available)
            .AddRow('G', SeatState.Available, SeatState.Reserved, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Available, SeatState.Reserved, SeatState.Available);

        Add(seatMap);
        Spacer();
        Message(() => seatMap.StatusMessage);
        Button("Confirm highlighted seat").OnClick(() => seatMap.ReserveHighlightedSeat());
        Button("Back").OnClick(() => Close());
    }
}
