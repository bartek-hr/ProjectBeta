using System.ComponentModel.DataAnnotations;

namespace ProjectBeta.Model;

public class Discount
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public int? MinGroupSize { get; set; }
    [Range(0, 120)]
    public int? MaxAge { get; set; }
    [Range(0, 120)]
    public int? MinAge { get; set; }
    [Range(0, 6)]
    public int? RequiredDayOfWeek { get; set; }
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveUntil { get; set; }
    public bool IsActive { get; set; } = true;
}
