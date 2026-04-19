namespace ProjectBeta.Model;

public class Movie
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Genres { get; set; } = [];
    public double? Rating { get; set; }
    public int? RuntimeSeconds { get; set; }
}
