namespace ProjectBeta.Model;

using System.ComponentModel.DataAnnotations;

public class Location
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")]
    public int Capacity { get; set; }

    public ICollection<Auditorium> Auditoriums { get; set; } = [];
}
