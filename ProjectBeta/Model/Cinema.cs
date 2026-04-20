namespace ProjectBeta.Model;

using System.ComponentModel.DataAnnotations;

public class Cinema
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    public ICollection<Auditorium> Auditoriums { get; set; } = [];
}
