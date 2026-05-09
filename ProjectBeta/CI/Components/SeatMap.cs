using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class SeatMap : Component
{
    private const int RowsCount = 14;
    private const int SeatCount = 12;

    private readonly List<List<SeatCell>> _rows = [];
    private int _selectedRow;
    private int _selectedSeat;
    public List<string> _selectedSeats = new();
    public List<int> _selectedTypes = new();
    private string? _statusMessage;
    private static readonly HashSet<string> VipSeats = new()
    {
        "D6","D7",
        "E5","E6","E7","E8",
        "F4","F5","F8","F9",
        "G4","G5","G8","G9",
        "H4","H5","H8","H9",
        "I4","I5","I8","I9",
        "J5","J6","J7","J8",
        "K6","K7"
    };

    private static readonly HashSet<string> KingSeats = new()
    {
        "F6","F7",
        "G6","G7",
        "H6","H7",
        "I6","I7",
    };

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

    // 1 = seat exists, 0 = empty space
    private static readonly bool[,] SeatLayout =
    {
        // A  B  C  D  E  F  G  H  I  J  K  L  M  N
        {false,false,true,true,true,true,true,true,true,true,false,false}, // A
        {false,true,true,true,true,true,true,true,true,true,true,false}, // B
        {false,true,true,true,true,true,true,true,true,true,true,false}, // C
        {true,true,true,true,true,true,true,true,true,true,true,true}, // D
        {true,true,true,true,true,true,true,true,true,true,true,true}, // E
        {true,true,true,true,true,true,true,true,true,true,true,true}, // F
        {true,true,true,true,true,true,true,true,true,true,true,true}, // G
        {true,true,true,true,true,true,true,true,true,true,true,true}, // H
        {true,true,true,true,true,true,true,true,true,true,true,true}, // I
        {true,true,true,true,true,true,true,true,true,true,true,true}, // J
        {true,true,true,true,true,true,true,true,true,true,true,true}, // K
        {false,true,true,true,true,true,true,true,true,true,true,false}, // L
        {false,true,true,true,true,true,true,true,true,true,true,false}, // M
        {false,false,true,true,true,true,true,true,true,true,false,false}, // N
    };

    public SeatMap AddRow(char rowLabel, params SeatState[] seats)
    {
        var rowIndex = rowLabel - 'A';
        var row = new List<SeatCell>();

        int seatIndex = 0;

        for (int i = 0; i < SeatCount; i++)
        {
            bool exists = SeatLayout[rowIndex, i];

            if (!exists)
            {
                row.Add(new SeatCell(rowLabel, i + 1, SeatState.Available, false));
                continue;
            }

            var state = seatIndex < seats.Length
                ? seats[seatIndex++]
                : SeatState.Available;

            row.Add(new SeatCell(rowLabel, i + 1, state, true));
        }

        _rows.Add(row);
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

    public string? SelectedSeatCode =>
        CurrentSeat is null ? null : $"{CurrentSeat.Row}{CurrentSeat.Number}";

    public string? StatusMessage => _statusMessage;

    public void ReserveHighlightedSeat() => TryReserveSelectedSeat();

    public override int Render(ComponentRenderContext context)
    {
        var buf = context.Buffer;

        var titleStyle = IsFocused ? Style.Highlight : Style.Primary;

        buf.WriteLine("╔══════════════════════════════════════════════════════════════════════╗", Style.Muted);
        buf.WriteLine($"║ {l10n("components.seat_map.title")} ║", titleStyle);
        buf.WriteLine("╚══════════════════════════════════════════════════════════════════════╝", Style.Muted);
        buf.WriteLine();

        buf.Write($" {l10n("components.seat_map.labels.movie")} ", Style.Primary);
        buf.WriteLine(MovieTitle, Style.Default.WithBold());
        buf.Write($" {l10n("components.seat_map.labels.hall")}  ", Style.Primary);
        buf.WriteLine(AuditoriumName, Style.Default);
        buf.Write($" {l10n("components.seat_map.labels.time")}  ", Style.Primary);
        buf.WriteLine(Showtime, Style.Default);
        buf.Write($" {l10n("components.seat_map.labels.pick")}  ", Style.Primary);
        buf.WriteLine(SelectedSeatCode ?? l10n("components.seat_map.none"), Style.Success.WithBold());
        buf.WriteLine();

        buf.WriteLine("                     ╭────────────────────────────╮", Style.Warning);
        buf.WriteLine("                     │           SCREEN           │", Style.Warning.WithBold());
        buf.WriteLine("                     ╰────────────────────────────╯", Style.Warning);
        buf.WriteLine();

        RenderSeatNumbers(buf);

        foreach (var row in _rows)
            RenderRow(buf, row);

        buf.WriteLine();

        buf.Write($"{l10n("components.seat_map.labels.legend")} ", Style.Primary);
        WriteSeatChip(buf, "  ", SeatState.Available, false, false, false);
        buf.Write($" {l10n("components.seat_map.legend.available")}   ", Style.Muted);

        WriteSeatChip(buf, "XX", SeatState.Reserved, false, false, false);
        buf.Write($" {l10n("components.seat_map.legend.reserved")}   ", Style.Muted);
       
        WriteSeatChip(buf, "👑", SeatState.Available, false, false, true);
        buf.Write($" {l10n("components.seat_map.legend.king")}   ", Style.Muted);

        WriteSeatChip(buf, "🔥", SeatState.Available, false, true, false);
        buf.Write($" {l10n("components.seat_map.legend.vip")}   ", Style.Muted);

        WriteSeatChip(buf, "<>", SeatState.Selected, true, false, false);
        buf.Write($" {l10n("components.seat_map.legend.selected")}", Style.Muted);

        buf.WriteLine();

        if (!string.IsNullOrWhiteSpace(_statusMessage))
        {
            buf.WriteLine();
            buf.WriteLine($" {_statusMessage}", Style.Success.WithBold());
        }

        return Math.Max(18, _rows.Count + 15);
    }

    public override bool ProcessKey(ConsoleKeyInfo key)
    {
        if (_rows.Count == 0)
            return false;

        return key.Key switch
        {
            ConsoleKey.LeftArrow => MoveSelection(0, -1),
            ConsoleKey.RightArrow => MoveSelection(0, 1),
            ConsoleKey.UpArrow => MoveSelection(-1, 0),
            ConsoleKey.DownArrow => MoveSelection(1, 0),
            ConsoleKey.Enter or ConsoleKey.Spacebar => TryReserveSelectedSeat(),
            _ => false
        };
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
        var nextSeat = Math.Clamp(_selectedSeat + seatDelta, 0, SeatCount - 1);

        if (FindNearestAvailableFrom(nextRow, nextSeat, out var row, out var seat))
        {
            _selectedRow = row;
            _selectedSeat = seat;
            _statusMessage = l10n("components.seat_map.status.hovering", new Dictionary<string, string>
            {
                ["seat"] = SelectedSeatCode ?? l10n("components.seat_map.none")
            });
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
            _statusMessage = l10n("components.seat_map.status.already_taken", new Dictionary<string, string>
            {
                ["seat"] = $"{seat.Row}{seat.Number}"
            });
            return true;
        }

        seat.State = SeatState.Reserved;
        _statusMessage = l10n("components.seat_map.status.reserved", new Dictionary<string, string>
        {
            ["seat"] = $"{seat.Row}{seat.Number}"
        });
        _selectedSeats.Add($"{seat.Row}{seat.Number}");
        if (KingSeats.Contains($"{seat.Row}{seat.Number}")){
            _selectedTypes.Add(3);
        }
        else if (VipSeats.Contains($"{seat.Row}{seat.Number}")){
            _selectedTypes.Add(2);
        }
        else {
            _selectedTypes.Add(1);
        }
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
                var cell = _rows[row][seat];

                if (!cell.Exists || cell.State == SeatState.Reserved)
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
        buf.Write("   ");

        for (int i = 1; i <= SeatCount; i++)
            buf.Write($" {i,2}  ", Style.Muted);

        buf.WriteLine();
    }

    private void RenderRow(TerminalBuffer buf, List<SeatCell> row)
    {
        var rowLabel = row[0].Row;

        buf.Write($" {rowLabel} ", Style.Primary.WithBold());
        buf.Write(" ");

        foreach (var seat in row)
        {
            if (!seat.Exists)
            {
                buf.Write("[  ]", Style.Muted);
                buf.Write(" ");
                continue;
            }

            var isSelected = CurrentSeat == seat;

            WriteSeatChip(
                buf,
                GetSeatText(seat, isSelected),
                seat.State,
                isSelected,
                seat.IsVIPSeat,
                seat.IsKingSeat
            );

            buf.Write(" ");
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
            SeatState.Available when seat.IsVIPSeat => "🔥",
            SeatState.Available when seat.IsKingSeat => "👑",
            _ => "  "
        };
    }

    private static void WriteSeatChip(
        TerminalBuffer buf,
        string innerText,
        SeatState state,
        bool isSelected,
        bool IsVIPSeat,
        bool IsKingSeat)
    {
        var style = state switch
        {
            SeatState.Reserved => new Style { Fg = ConsoleColor.White, Bg = ConsoleColor.Black, Bold = true },
            SeatState.Selected => new Style { Fg = ConsoleColor.Black, Bg = ConsoleColor.Green, Bold = true },
            _ when IsVIPSeat => new Style { Fg = ConsoleColor.White, Bg = ConsoleColor.Yellow, Bold = true },
            _ when IsKingSeat => new Style { Fg = ConsoleColor.White, Bg = ConsoleColor.DarkRed, Bold = true },
            _ => new Style { Fg = ConsoleColor.Black, Bg = ConsoleColor.DarkCyan, Bold = true },
        };

        var borderStyle = isSelected ? Style.Success.WithBold() : Style.Muted;

        buf.Write("[", borderStyle);
        buf.Write(innerText.PadRight(2).Substring(0, 2), style);
        buf.Write("]", borderStyle);
    }

    private sealed class SeatCell(char row, int number, SeatState state, bool exists)
    {
        public char Row { get; } = row;
        public int Number { get; } = number;
        public SeatState State { get; set; } = state;
        public bool Exists { get; } = exists;

        public bool IsVIPSeat =>
            VipSeats.Contains($"{Row}{Number}");

        public bool IsKingSeat =>
            KingSeats.Contains($"{Row}{Number}");    
    }
}

public enum SeatState
{
    Available,
    Reserved,
    Selected,
}
