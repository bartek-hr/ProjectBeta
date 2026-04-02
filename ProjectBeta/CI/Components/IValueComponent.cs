namespace ProjectBeta.CI.Components;

public interface IValueComponent
{
    string Label { get; }
    object? Value { get; }
}
