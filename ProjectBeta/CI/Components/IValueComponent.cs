namespace ProjectBeta.CI.Components;

public interface IValueComponent
{
    string Label { get; }
    string FieldKey { get; }
    object? Value { get; }
}
