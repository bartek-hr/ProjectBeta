using System.ComponentModel.DataAnnotations;

namespace ProjectBeta.Model;

public class SeatPrice
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}
