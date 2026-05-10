using ProjectBeta.CI.Rendering;

namespace ProjectBeta.CI.Components;

public sealed class SeatMap : Component
{
    private int RowsCount;
    private int SeatCount;

    private readonly List<List<SeatCell>> _rows = [];
    private int _selectedRow;
    private int _selectedSeat;
    public List<string> _selectedSeats = new();
    public List<int> _selectedTypes = new();
    private string? _statusMessage;
    private static readonly HashSet<string> VipSeatsSmall = new()
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

    private static readonly HashSet<string> KingSeatsSmall = new()
    {
        "F6","F7",
        "G6","G7",
        "H6","H7",
        "I6","I7",
    };

    private static readonly HashSet<string> VipSeatsMedium = new()
    {
        "B7","B8","B9","B10","B11","B12",//18
        "C6","C7","C8","C9","C10","C11","C12","C13",//17
        "D6","D7","D8","D9","D10","D11","D12","D13",//16
        "E5","E6","E7","E8","E9","E10","E11","E12","E13","E14",//15
        "F5","F6","F7","F8","F11","F12","F13","F14",//14
        "G4","G5","G6","G7","G12","G13","G14","G15",//13
        "H4","H5","H6","H13","H14","H15",//12
        "I3","I4","I5","I6","I13","I14","I15","I16",//11
        "J3","J4","J5","J6","J13","J14","J15","J16",//10
        "K3","K4","K5","K6","K13","K14","K15","K16",//9
        "L4","L5","L6","L7","L12","L13","L14","L15",//8
        "M5","M6","M7","M8","M11","M12","M13","M14",//7
        "N6","N7","N8","N9","N10","N11","N12","N13",//6
        "O7","O8","O9","O10","O11","O12",//5
        "P7","P8","P9","P10","P11","P12",//4
    };

    private static readonly HashSet<string> KingSeatsMedium = new()
    {
        "F9","F10",
        "G8","G9","G10","G11",
        "H7","H8","H9","H10","H11","H12",
        "I7","I8","I9","I10","I11","I12",
        "J7","J8","J9","J10","J11","J12",
        "K7","K8","K9","K10","K11","K12",
        "L8","L9","L10","L11",
        "M9","M10",
    };


    private static readonly HashSet<string> VipSeatsBig = new()
    {
        "B10","B11","B12","B13","B14","B15","B16","B17","B18","B19","B20","B21",//19
        "C9","C10","C11","C12","C13","C14","C15","C16","C17","C18","C19","C20","C21","C22",//18
        "D9","D10","D11","D12","D13","D14","D15","D16","D17","D18","D19","D20","D21","D22",//17
        "E8","E9","E10","E11","E12","E13","E18","E19","E20","E21","E22","E23",//16
        "F8","F9","F10","F11","F12","F19","F20","F21","F22","F23",//15
        "G7","G8","G9","G10","G11","G20","G21","G22","G23","G24",//14
        "H7","H8","H9","H10","H11","H20","H21","H22","H23","H24",//13
        "I6","I7","I8","I9","I10","I11","I20","I21","I22","I23","I24","I25",//12
        "J6","J7","J8","J9","J10","J11","J20","J21","J22","J23","J24","J25",//11
        "K7","K8","K9","K10","K11","K20","K21","K22","K23","K24",//10
        "L8","L9","L10","L11","L20","L21","L22","L23",//9
        "M9","M10","M11","M12","M13","M18","M19","M20","M21","M22",//8
        "N9","N10","N11","N12","N13","N14","N15","N16","N17","N18","N19","N20","N21","N22",//7
        "O10","O11","O12","O13","O14","O15","O16","O17","O18","O19","O20","O21",//6
        "P11","P12","P13","P14","P15","P16","P17","P18","P19","P20",//5
        "Q13","Q14","Q15","Q16","Q17","Q18",//4
    };

    private static readonly HashSet<string> KingSeatsBig = new()
    {
        "E14","E15","E16","E17",//16
        "F13","F14","F15","F16","F17","F18",//15
        "G12","G13","G14","G15","G16","G17","G18","G19",//14
        "H12","H13","H14","H15","H16","H17","H18","H19",//13
        "I12","I13","I14","I15","I16","I17","I18","I19",//12
        "J12","J13","J14","J15","J16","J17","J18","J19",//11
        "K12","K13","K14","K15","K16","K17","K18","K19",//10
        "L12","L13","L14","L15","L16","L17","L18","L19",//9
        "M14","M15","M16","M17",//8
    };

    private HashSet<string> KingSeats;
    private HashSet<string> VipSeats;

    public SeatMap(string auditoriumName, string movieTitle, string showtime, string capacity)
    {
        AuditoriumName = auditoriumName;
        MovieTitle = movieTitle;
        Showtime = showtime;
        Capacity = capacity;

        RowsCount = determineRowCount();
        SeatCount = determineSeatCount();
    }

    public override bool IsFocusable => true;

    public string AuditoriumName { get; }
    public string MovieTitle { get; }
    public string Showtime { get; }
    public string Capacity { get; }

    // 1 = seat exists, 0 = empty space
    private static readonly bool[,] SeatLayoutSmall =
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

    private static readonly bool[,] SeatLayoutMedium =
    {
        // A  B  C  D  E  F  G  H  I  J  K  L  M  N
        {false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false}, // A
        {false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false}, // B
        {false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false}, // C
        {false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false}, // D
        {false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false}, // E
        {false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false}, // F
        {true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true}, // G
        {true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true}, // H
        {true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true}, // I
        {true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true}, // J
        {true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true}, // K
        {false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false}, // L
        {false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false}, // M
        {false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false}, // N
        {false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false}, // O
        {false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false}, // P
        {false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false}, // Q
        {false,false,false,true,true,true,true,true,true,true,true,true,true,true,true,false,false,false}, // R
        {false,false,false,true,true,true,true,true,true,true,true,true,true,true,true,false,false,false}, // S
    };


    private static readonly bool[,] SeatLayoutBig =
    {
        // A  B  C  D  E  F  G  H  I  J  K  L  M  N
        {false,false,false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false,false,false}, // A
        {false,false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false,false}, // B
        {false,false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false,false}, // C
        {false,false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false,false}, // D
        {false,false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false,false}, // E
        {false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false}, // F
        {false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false}, // G
        {true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true}, // H
        {true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true}, // I
        {true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true}, // J
        {true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true}, // K
        {true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true}, // L
        {false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false}, // M
        {false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false}, // N
        {false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false}, // O
        {false,false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false,false}, // P
        {false,false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false,false}, // Q
        {false,false,false,false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false,false,false,false}, // R
        {false,false,false,false,false,false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false,false,false,false,false,false}, // S
        {false,false,false,false,false,false,false,false,true,true,true,true,true,true,true,true,true,true,true,true,true,true,false,false,false,false,false,false,false,false}, // T
    };

    public int determineRowCount()
    {
        if (Capacity == "300") {
            return 19;
        }
        if (Capacity == "500") {
            return 20;
        }
        return 14;
    }
    public int determineSeatCount()
    {
        if (Capacity == "300") {
            return 18;
        }
        if (Capacity == "500") {
            return 30;
        }
        return 12;
    }
    public bool[,] DetermineSeatLayout()
    {
        if (Capacity == "300") {
            return SeatLayoutMedium;
        }
        if (Capacity == "500") {
            return SeatLayoutBig;
        }
        return SeatLayoutSmall;
    }
   
    public SeatMap AddRow(char rowLabel, params SeatState[] seats)
    {
        var rowIndex = rowLabel - 'A';
        var row = new List<SeatCell>();

        int seatIndex = 0;
        bool[,] SeatLayout = DetermineSeatLayout();
        for (int i = 0; i < SeatCount; i++)
        {
            bool exists = SeatLayout[rowIndex, i];

            if (!exists)
            {
                row.Add(new SeatCell(rowLabel, i + 1, SeatState.Available, false, Capacity));
                continue;
            }

            var state = seatIndex < seats.Length
                ? seats[seatIndex++]
                : SeatState.Available;

            row.Add(new SeatCell(rowLabel, i + 1, state, true, Capacity));
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
        WriteSeatChip(buf, "  ", SeatState.Available, false, false, false);
        buf.Write(" Available   ", Style.Muted);

        WriteSeatChip(buf, "XX", SeatState.Reserved, false, false, false);
        buf.Write(" Reserved   ", Style.Muted);
       
        WriteSeatChip(buf, "👑", SeatState.Available, false, false, true);
        buf.Write(" King   ", Style.Muted);

        WriteSeatChip(buf, "🔥", SeatState.Available, false, true, false);
        buf.Write(" VIP   ", Style.Muted);

        WriteSeatChip(buf, "<>", SeatState.Selected, true, false, false);
        buf.Write(" Selected", Style.Muted);

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
            _statusMessage = $"Hovering {SelectedSeatCode}";
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
        _statusMessage = $"Seat {seat.Row}{seat.Number} reserved.";
        _selectedSeats.Add($"{seat.Row}{seat.Number}");
        AddSelectedTypeOnCapacity($"{seat.Row}{seat.Number}");
        SnapSelectionToNearestAvailable();
        return true;
    }
    private void AddSelectedTypeOnCapacity(string seatPosition)
    {
        if (Capacity == "500") {
            AddSelectedTypeOnAuditorium(seatPosition, KingSeatsBig, VipSeatsBig);
        } else if (Capacity == "300") {
            AddSelectedTypeOnAuditorium(seatPosition, KingSeatsMedium, VipSeatsMedium);
        } else {
            AddSelectedTypeOnAuditorium(seatPosition, KingSeatsSmall, VipSeatsSmall);
        }

    }
    private void AddSelectedTypeOnAuditorium(string seatPosition, HashSet<string> KingSeats, HashSet<string> VIPSeats)
    {
            if (KingSeats.Contains(seatPosition)){
                _selectedTypes.Add(3);
            }
            else if (VIPSeats.Contains(seatPosition)){
                _selectedTypes.Add(2);
            }
            else {
                _selectedTypes.Add(1);
            }
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

    private sealed class SeatCell(char row, int number, SeatState state, bool exists, string capacity)
    {
        public char Row { get; } = row;
        public int Number { get; } = number;
        public SeatState State { get; set; } = state;
        public bool Exists { get; } = exists;

        public bool IsVIPSeat {
            get
            {
                if (capacity == "300"){
                    return VipSeatsMedium.Contains($"{Row}{Number}");
                }
                else if(capacity == "500"){
                    return VipSeatsBig.Contains($"{Row}{Number}");
                }
                return VipSeatsSmall.Contains($"{Row}{Number}");
            }
        }
        public bool IsKingSeat {
            get
            {
                if (capacity == "300"){
                    return KingSeatsMedium.Contains($"{Row}{Number}");
                }
                else if(capacity == "500"){
                    return KingSeatsBig.Contains($"{Row}{Number}");
                }
                return KingSeatsSmall.Contains($"{Row}{Number}");
            }
        }    
    }
}

public enum SeatState
{
    Available,
    Reserved,
    Selected,
}