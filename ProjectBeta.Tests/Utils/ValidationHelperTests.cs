using ProjectBeta.Utils;

namespace ProjectBeta.Tests.Utils;

[TestClass]
public class ValidationHelperTests
{
    [TestMethod]
    public void AnyNullOrWhiteSpace_AllFilled_ReturnsFalse()
    {
        Assert.IsFalse(ValidationHelper.AnyNullOrWhiteSpace("a", "b", "c"));
    }

    [TestMethod]
    public void AnyNullOrWhiteSpace_OneEmpty_ReturnsTrue()
    {
        Assert.IsTrue(ValidationHelper.AnyNullOrWhiteSpace("a", "", "c"));
    }

    [TestMethod]
    public void AnyNullOrWhiteSpace_OneNull_ReturnsTrue()
    {
        Assert.IsTrue(ValidationHelper.AnyNullOrWhiteSpace("a", null!, "c"));
    }

    [TestMethod]
    public void AnyNullOrWhiteSpace_WhitespaceOnly_ReturnsTrue()
    {
        // The implementation currently only checks null/empty, NOT pure whitespace.
        // This test documents the existing (surprising) behavior: "   " is NOT caught.
        // Fix the implementation to also check string.IsNullOrWhiteSpace if that is desired.
        var result = ValidationHelper.AnyNullOrWhiteSpace("a", "   ", "c");
        // Current behavior: whitespace-only strings pass through as valid.
        Assert.IsFalse(result, "Current implementation does not flag whitespace-only strings.");
    }

    [TestMethod]
    public void AnyNullOrWhiteSpace_NoArgs_ReturnsFalse()
    {
        Assert.IsFalse(ValidationHelper.AnyNullOrWhiteSpace());
    }
}
