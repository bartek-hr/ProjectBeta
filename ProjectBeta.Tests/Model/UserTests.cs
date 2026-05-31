using ProjectBeta.Model;

namespace ProjectBeta.Tests.Model;

[TestClass]
public class UserTests
{
    [TestMethod]
    public void IsSuperAdmin_ReturnsTrueForSuperAdminRole()
    {
        var user = new User { Username = "someone", Role = "SuperAdmin" };
        Assert.IsTrue(user.IsSuperAdmin());
    }

    [TestMethod]
    public void IsSuperAdmin_ReturnsTrueForAdminUsername()
    {
        var user = new User { Username = "admin", Role = "User" };
        Assert.IsTrue(user.IsSuperAdmin());
    }

    [TestMethod]
    public void IsAdmin_ReturnsTrueForAdminRole()
    {
        var user = new User { Username = "manager", Role = "Admin" };
        Assert.IsTrue(user.IsAdmin());
    }

    [TestMethod]
    public void IsAdmin_ReturnsFalseForRegularUser()
    {
        var user = new User { Username = "member", Role = "User" };
        Assert.IsFalse(user.IsAdmin());
    }
}
