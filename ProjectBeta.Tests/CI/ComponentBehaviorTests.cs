using ProjectBeta.CI.Components;

namespace ProjectBeta.Tests.CI;

[TestClass]
public class ComponentBehaviorTests
{
    [TestMethod]
    public void InputText_Validate_ReportsRequiredMinAndPatternErrors()
    {
        var input = new InputText("Username")
            .Required()
            .Min(3)
            .Pattern("^[a-z]+$");

        CollectionAssert.AreEqual(
            new[] { l10n("validation.common.required", new Dictionary<string, string> { ["field"] = "Username" }) },
            input.Validate());

        input.Value = "A1";
        var errors = input.Validate();
        Assert.AreEqual(2, errors.Count);
    }

    [TestMethod]
    public void InputText_ProcessKey_RespectsMaxLengthAndSupportsHomeDelete()
    {
        var input = new InputText("Code").Max(2);
        input.ProcessKey(new ConsoleKeyInfo('A', ConsoleKey.A, false, false, false));
        input.ProcessKey(new ConsoleKeyInfo('B', ConsoleKey.B, false, false, false));

        Assert.IsFalse(input.ProcessKey(new ConsoleKeyInfo('C', ConsoleKey.C, false, false, false)));

        input.ProcessKey(new ConsoleKeyInfo('\0', ConsoleKey.Home, false, false, false));
        input.ProcessKey(new ConsoleKeyInfo('\0', ConsoleKey.Delete, false, false, false));

        Assert.AreEqual("B", input.Value);
    }

    [TestMethod]
    public void NumberInput_Validate_ReportsRequiredInvalidAndRangeErrors()
    {
        var input = new NumberInput("Age").Required().Min(10).Max(20);

        CollectionAssert.AreEqual(
            new[] { l10n("validation.common.required", new Dictionary<string, string> { ["field"] = "Age" }) },
            input.Validate());

        input.ProcessKey(new ConsoleKeyInfo('-', ConsoleKey.OemMinus, false, false, false));
        input.ProcessKey(new ConsoleKeyInfo('.', ConsoleKey.OemPeriod, false, false, false));
        CollectionAssert.AreEqual(
            new[] { l10n("validation.common.invalid_number", new Dictionary<string, string> { ["field"] = "Age" }) },
            input.Validate());

        var ranged = new NumberInput("Age").Min(10).Max(20).Default(9);
        CollectionAssert.AreEqual(
            new[] { l10n("validation.common.min_value", new Dictionary<string, string> { ["field"] = "Age", ["min"] = "10" }) },
            ranged.Validate());
    }

    [TestMethod]
    public void NumberInput_ReadOnlyOverride_IgnoresEditsAndUsesOverrideValue()
    {
        var input = new NumberInput("Price")
            .Default(5)
            .ReadOnly(() => true, 42);

        Assert.IsFalse(input.ProcessKey(new ConsoleKeyInfo('9', ConsoleKey.D9, false, false, false)));
        Assert.AreEqual(5d, input.Value);
        Assert.AreEqual(42d, input.EffectiveValue);
    }

    [TestMethod]
    public void DateInput_Validate_ReportsRequiredAndRangeErrors()
    {
        var required = new DateInput("Start date").Required();
        CollectionAssert.AreEqual(
            new[] { l10n("validation.common.required", new Dictionary<string, string> { ["field"] = "Start date" }) },
            required.Validate());

        var ranged = new DateInput("Start date")
            .Default(new DateOnly(2024, 1, 1))
            .Min(new DateOnly(2024, 2, 1))
            .Max(new DateOnly(2024, 12, 31));

        CollectionAssert.AreEqual(
            new[] { l10n("validation.common.date_on_or_after", new Dictionary<string, string> { ["field"] = "Start date", ["date"] = "2024-02-01" }) },
            ranged.Validate());
    }

    [TestMethod]
    public void DateInput_UpArrowOnLastDay_RollsIntoNextMonth()
    {
        var input = new DateInput("Date").Default(new DateOnly(2024, 1, 31));

        input.ProcessKey(new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false));
        input.ProcessKey(new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false));
        input.ProcessKey(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));

        Assert.AreEqual(new DateOnly(2024, 2, 1), input.Value);
    }

    [TestMethod]
    public void Select_RequiredWithoutSelection_ReturnsValidationError()
    {
        var input = new Select("Seat").Required().AddOption("A").AddOption("B");

        CollectionAssert.AreEqual(
            new[] { l10n("validation.common.required", new Dictionary<string, string> { ["field"] = "Seat" }) },
            input.Validate());
    }

    [TestMethod]
    public void RadioGroup_DefaultAndWraparound_SelectExpectedValue()
    {
        var input = new RadioGroup("Role")
            .AddOption("A")
            .AddOption("B")
            .Default("B");

        input.ProcessKey(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
        input.ProcessKey(new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false));

        Assert.AreEqual("A", input.Value);
    }

    [TestMethod]
    public void MultiSelect_DefaultsAndSpace_TogglesSelection()
    {
        var input = new MultiSelect("Seats")
            .AddOption("A")
            .AddOption("B")
            .Defaults("A", "B");

        input.ProcessKey(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));

        CollectionAssert.AreEqual(new[] { "B" }, input.Value);
    }

    [TestMethod]
    public void Checkbox_DefaultAndIgnoredKey_BehaveAsExpected()
    {
        var input = new Checkbox("Enabled").Default(true);

        Assert.IsFalse(input.ProcessKey(new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false)));
        Assert.IsTrue(input.Value);
    }
}
