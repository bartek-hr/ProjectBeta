namespace ProjectBeta.Model;

using System.ComponentModel.DataAnnotations;

public class Auditorium
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000.")]
    public int Capacity { get; set; }

    // public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    // public ICollection<Screening> Screenings { get; set; } = [];
    public int CinemaId { get; set; }
    public Cinema Cinema { get; set; } = null!;

    public int? LocationId { get; set; }
    public Location? Location { get; set; }
}
