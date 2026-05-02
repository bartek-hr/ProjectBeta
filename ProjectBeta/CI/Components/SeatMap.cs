using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class SeatMap : Component
{
    private readonly List<List<SeatCell>> _rows = [];
    private int _selectedRow;
    private int _selectedSeat;
    private string? _statusMessage;

    public SeatMap(string auditoriumName, string movieTitle, string showtime)
    {
        AuditoriumName = auditoriumName;
        MovieTitle = movieTitle;
        Showtime = showtime;
    }

    public override bool IsFocusable => true;

    public string AuditoriumName { get; }
    public string MovieTitle { get; }
    public string Showtime { get; }

    public SeatMap AddRow(char rowLabel, params SeatState[] seats)
    {
        _rows.Add(seats
            .Select((state, index) => new SeatCell(rowLabel, index + 1, state))
            .ToList());

        SnapSelectionToNearestAvailable();
        return this;
    }

    public SeatMap Reserve(params string[] seatCodes)
    {
        foreach (var code in seatCodes)
        {
            var seat = FindSeat(code);
            if (seat != null)
                seat.State = SeatState.Reserved;
        }

        SnapSelectionToNearestAvailable();
        return this;
    }

    public string? SelectedSeatCode => CurrentSeat is null ? null : $"{CurrentSeat.Row}{CurrentSeat.Number}";
    public string? StatusMessage => _statusMessage;

    public void ReserveHighlightedSeat() => TryReserveSelectedSeat();

    public override int Render(ComponentRenderContext context)
    {
        var buf = context.Buffer;

        var titleStyle = IsFocused ? Style.Highlight : Style.Primary;
        buf.WriteLine("╔══════════════════════════════════════════════════════════════════════╗", Style.Muted);
        buf.WriteLine("║                         CINEMA SEAT SELECTOR                        ║", titleStyle);
        buf.WriteLine("╚══════════════════════════════════════════════════════════════════════╝", Style.Muted);
        buf.WriteLine();

        buf.Write(" Movie: ", Style.Primary);
        buf.WriteLine(MovieTitle, Style.Default.WithBold());
        buf.Write(" Hall:  ", Style.Primary);
        buf.WriteLine(AuditoriumName, Style.Default);
        buf.Write(" Time:  ", Style.Primary);
        buf.WriteLine(Showtime, Style.Default);
        buf.Write(" Pick:  ", Style.Primary);
        buf.WriteLine(SelectedSeatCode ?? "None", Style.Success.WithBold());
        buf.WriteLine();

        buf.WriteLine("                     ╭────────────────────────────╮", Style.Warning);
        buf.WriteLine("                     │           SCREEN           │", Style.Warning.WithBold());
        buf.WriteLine("                     ╰────────────────────────────╯", Style.Warning);
        buf.WriteLine();

        RenderSeatNumbers(buf);
        foreach (var row in _rows)
            RenderRow(buf, row);

        buf.WriteLine();
        buf.Write(" Legend: ", Style.Primary);
        WriteSeatChip(buf, "  ", SeatState.Available, false, false);
        buf.Write(" Available   ", Style.Muted);
        WriteSeatChip(buf, "XX", SeatState.Reserved, false, false);
        buf.Write(" Reserved   ", Style.Muted);
        WriteSeatChip(buf, "<>", SeatState.Selected, true, false);
        buf.Write(" Selected", Style.Muted);
        buf.WriteLine();
        buf.WriteLine(" Use arrow keys to move, Enter or Space to reserve your highlighted seat.", Style.Muted);

        if (!string.IsNullOrWhiteSpace(_statusMessage))
        {
            buf.WriteLine();
            buf.WriteLine($" { _statusMessage }", Style.Success.WithBold());
        }

        return Math.Max(18, _rows.Count + 15 + (string.IsNullOrWhiteSpace(_statusMessage) ? 0 : 2));
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        if (_rows.Count == 0)
            return false;

        switch (key.Key)
        {
            case ConsoleKey.LeftArrow:
                return MoveSelection(0, -1);
            case ConsoleKey.RightArrow:
                return MoveSelection(0, 1);
            case ConsoleKey.UpArrow:
                return MoveSelection(-1, 0);
            case ConsoleKey.DownArrow:
                return MoveSelection(1, 0);
            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                return TryReserveSelectedSeat();
            default:
                return false;
        }
    }

    private SeatCell? CurrentSeat
    {
        get
        {
            if (_selectedRow < 0 || _selectedRow >= _rows.Count)
                return null;
            if (_selectedSeat < 0 || _selectedSeat >= _rows[_selectedRow].Count)
                return null;
            return _rows[_selectedRow][_selectedSeat];
        }
    }

    private bool MoveSelection(int rowDelta, int seatDelta)
    {
        var nextRow = Math.Clamp(_selectedRow + rowDelta, 0, _rows.Count - 1);
        var nextSeat = Math.Clamp(_selectedSeat + seatDelta, 0, _rows[nextRow].Count - 1);

        if (FindNearestAvailableFrom(nextRow, nextSeat, out var row, out var seat))
        {
            _selectedRow = row;
            _selectedSeat = seat;
            _statusMessage = $"Hovering {SelectedSeatCode} · premium central view";
            return true;
        }

        return false;
    }

    private bool TryReserveSelectedSeat()
    {
        var seat = CurrentSeat;
        if (seat == null)
            return false;

        if (seat.State == SeatState.Reserved)
        {
            _statusMessage = $"{seat.Row}{seat.Number} is already taken.";
            return true;
        }

        seat.State = SeatState.Reserved;
        _statusMessage = $"Seat {seat.Row}{seat.Number} reserved for {MovieTitle}.";
        SnapSelectionToNearestAvailable();
        return true;
    }

    private void SnapSelectionToNearestAvailable()
    {
        if (FindNearestAvailableFrom(_selectedRow, _selectedSeat, out var row, out var seat))
        {
            _selectedRow = row;
            _selectedSeat = seat;
        }
    }

    private bool FindNearestAvailableFrom(int startRow, int startSeat, out int rowIndex, out int seatIndex)
    {
        rowIndex = -1;
        seatIndex = -1;

        var candidates = new List<(int Distance, int Row, int Seat)>();

        for (var row = 0; row < _rows.Count; row++)
        {
            for (var seat = 0; seat < _rows[row].Count; seat++)
            {
                if (_rows[row][seat].State == SeatState.Reserved)
                    continue;

                var distance = Math.Abs(row - startRow) * 10 + Math.Abs(seat - startSeat);
                candidates.Add((distance, row, seat));
            }
        }

        if (candidates.Count == 0)
            return false;

        var best = candidates.OrderBy(c => c.Distance).First();
        rowIndex = best.Row;
        seatIndex = best.Seat;
        return true;
    }

    private SeatCell? FindSeat(string seatCode)
    {
        if (string.IsNullOrWhiteSpace(seatCode) || seatCode.Length < 2)
            return null;

        var rowLabel = char.ToUpperInvariant(seatCode[0]);
        if (!int.TryParse(seatCode[1..], out var number))
            return null;

        return _rows
            .SelectMany(r => r)
            .FirstOrDefault(s => s.Row == rowLabel && s.Number == number);
    }

    private void RenderSeatNumbers(TerminalBuffer buf)
    {
        buf.Write("     ");
        for (var seatNumber = 1; seatNumber <= MaxSeatCount(); seatNumber++)
        {
            if (seatNumber == 5)
                buf.Write("   ");

            buf.Write($" {seatNumber:00} ", Style.Muted);
        }
        buf.WriteLine();
    }

    private void RenderRow(TerminalBuffer buf, List<SeatCell> row)
    {
        var rowLabel = row[0].Row;
        buf.Write($"  {rowLabel}  ", Style.Primary.WithBold());

        foreach (var seat in row)
        {
            if (seat.Number == 5)
                buf.Write("   ");

            var isSelected = CurrentSeat == seat;
            WriteSeatChip(buf, GetSeatText(seat, isSelected), isSelected ? SeatState.Selected : seat.State, isSelected, seat.IsLoveSeat);
        }

        buf.WriteLine();
    }

    private static string GetSeatText(SeatCell seat, bool isSelected)
    {
        if (isSelected)
            return "<>";

        return seat.State switch
        {
            SeatState.Reserved => "XX",
            SeatState.Available when seat.IsLoveSeat => "♥♥",
            _ => "  "
        };
    }

    private static void WriteSeatChip(TerminalBuffer buf, string innerText, SeatState state, bool isSelected, bool isLoveSeat)
    {
        var style = state switch
        {
            SeatState.Reserved => new Style { Fg = ConsoleColor.White, Bg = ConsoleColor.DarkRed, Bold = true },
            SeatState.Selected => new Style { Fg = ConsoleColor.Black, Bg = ConsoleColor.Green, Bold = true },
            _ when isLoveSeat => new Style { Fg = ConsoleColor.White, Bg = ConsoleColor.DarkMagenta, Bold = true },
            _ => new Style { Fg = ConsoleColor.Black, Bg = ConsoleColor.DarkCyan, Bold = true },
        };

        var borderStyle = isSelected ? Style.Success.WithBold() : Style.Muted;
        buf.Write("[", borderStyle);
        buf.Write(innerText, style);
        buf.Write("]", borderStyle);
    }

    private int MaxSeatCount() => _rows.Count == 0 ? 0 : _rows.Max(r => r.Count);

    private sealed class SeatCell(char row, int number, SeatState state)
    {
        public char Row { get; } = row;
        public int Number { get; } = number;
        public SeatState State { get; set; } = state;
        public bool IsLoveSeat => Row >= 'F' && Number is 4 or 5 or 8 or 9;
    }
}

public enum SeatState
{
    Available,
    Reserved,
    Selected,
}
