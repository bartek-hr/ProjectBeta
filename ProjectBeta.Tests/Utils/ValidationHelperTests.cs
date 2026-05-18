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
        Assert.IsTrue(ValidationHelper.AnyNullOrWhiteSpace("a", "   ", "c"));
    }

    [TestMethod]
    public void AnyNullOrWhiteSpace_NoArgs_ReturnsFalse()
    {
        Assert.IsFalse(ValidationHelper.AnyNullOrWhiteSpace());
    }
}
