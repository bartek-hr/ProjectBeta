namespace ProjectBeta.CI.Components;

public interface IValidatable
{
    List<string> Validate();
    void SetErrors(List<string> errors);
}
