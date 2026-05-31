using ProjectBeta.Access;
using ProjectBeta.Data;
using ProjectBeta.Logic;
using ProjectBeta.Model;
using ProjectBeta.Tests.Helpers;

namespace ProjectBeta.Tests;

[TestClass]
public class SnackLogicTests
{
    private static User Admin => new() { Id = 1, Username = "admin", Role = "Admin", Email = "a@a.com", FirstName = "A", LastName = "A", DateOfBirth = new DateOnly(1990, 1, 1), PasswordHash = "x" };
    private static User SuperAdmin => new() { Id = 2, Username = "superadmin", Role = "SuperAdmin", Email = "b@b.com", FirstName = "B", LastName = "B", DateOfBirth = new DateOnly(1990, 1, 1), PasswordHash = "x" };
    private static User RegularUser => new() { Id = 3, Username = "user", Role = "User", Email = "c@c.com", FirstName = "C", LastName = "C", DateOfBirth = new DateOnly(1990, 1, 1), PasswordHash = "x" };

    private static (SnackLogic logic, AppDbContext context) CreateLogic()
    {
        var context = TestDbContext.Create();
        var access = new SnackAccess(context);
        return (new SnackLogic(access), context);
    }

    // --- GetAll ---

    [TestMethod]
    public void GetAll_ReturnsAllSnacks()
    {
        var (logic, context) = CreateLogic();
        context.Snacks.AddRange(
            new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 },
            new Snack { Name = "Cola", Price = 2.00m, Quantity = 50 });
        context.SaveChanges();

        var result = logic.GetAll();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GetAll_ReturnsEmpty_WhenNoSnacks()
    {
        var (logic, _) = CreateLogic();

        var result = logic.GetAll();

        Assert.AreEqual(0, result.Count);
    }

    // --- GetById ---

    [TestMethod]
    public void GetById_ReturnsCorrectSnack()
    {
        var (logic, context) = CreateLogic();
        context.Snacks.Add(new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 });
        context.SaveChanges();
        var id = context.Snacks.First().Id;

        var result = logic.GetById(id);

        Assert.IsNotNull(result);
        Assert.AreEqual("Popcorn", result.Name);
    }

    [TestMethod]
    public void GetById_ReturnsNull_WhenNotFound()
    {
        var (logic, _) = CreateLogic();

        var result = logic.GetById(999);

        Assert.IsNull(result);
    }

    // --- Add ---

    [TestMethod]
    public void Add_AdminUser_AddsSnack()
    {
        var (logic, context) = CreateLogic();
        var snack = new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 };

        logic.Add(snack, Admin);

        Assert.AreEqual(1, context.Snacks.Count());
    }

    [TestMethod]
    public void Add_SuperAdminUser_AddsSnack()
    {
        var (logic, context) = CreateLogic();
        var snack = new Snack { Name = "Nachos", Price = 4.00m, Quantity = 80 };

        logic.Add(snack, SuperAdmin);

        Assert.AreEqual(1, context.Snacks.Count());
    }

    [TestMethod]
    public void Add_RegularUser_ThrowsUnauthorized()
    {
        var (logic, _) = CreateLogic();
        var snack = new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 };

        Assert.ThrowsException<UnauthorizedAccessException>(() => logic.Add(snack, RegularUser));
    }

    [TestMethod]
    public void Add_NegativePrice_ThrowsArgumentException()
    {
        var (logic, _) = CreateLogic();
        var snack = new Snack { Name = "Popcorn", Price = -1m, Quantity = 100 };

        Assert.ThrowsException<ArgumentException>(() => logic.Add(snack, Admin));
    }

    [TestMethod]
    public void Add_NegativeQuantity_ThrowsArgumentException()
    {
        var (logic, _) = CreateLogic();
        var snack = new Snack { Name = "Popcorn", Price = 3.50m, Quantity = -1 };

        Assert.ThrowsException<ArgumentException>(() => logic.Add(snack, Admin));
    }

    [TestMethod]
    public void Add_EmptyName_ThrowsArgumentException()
    {
        var (logic, _) = CreateLogic();
        var snack = new Snack { Name = "", Price = 3.50m, Quantity = 100 };

        Assert.ThrowsException<ArgumentException>(() => logic.Add(snack, Admin));
    }

    // --- Update ---

    [TestMethod]
    public void Update_AdminUser_UpdatesSnack()
    {
        var (logic, context) = CreateLogic();
        context.Snacks.Add(new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 });
        context.SaveChanges();
        var snack = context.Snacks.First();
        snack.Price = 4.00m;

        logic.Update(snack, Admin);

        Assert.AreEqual(4.00m, context.Snacks.First().Price);
    }

    [TestMethod]
    public void Update_RegularUser_ThrowsUnauthorized()
    {
        var (logic, context) = CreateLogic();
        context.Snacks.Add(new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 });
        context.SaveChanges();
        var snack = context.Snacks.First();

        Assert.ThrowsException<UnauthorizedAccessException>(() => logic.Update(snack, RegularUser));
    }

    [TestMethod]
    public void Update_NegativePrice_ThrowsArgumentException()
    {
        var (logic, context) = CreateLogic();
        context.Snacks.Add(new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 });
        context.SaveChanges();
        var snack = context.Snacks.First();
        snack.Price = -5m;

        Assert.ThrowsException<ArgumentException>(() => logic.Update(snack, Admin));
    }

    // --- Delete ---

    [TestMethod]
    public void Delete_AdminUser_DeletesSnack()
    {
        var (logic, context) = CreateLogic();
        context.Snacks.Add(new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 });
        context.SaveChanges();
        var id = context.Snacks.First().Id;

        logic.Delete(id, Admin);

        Assert.AreEqual(0, context.Snacks.Count());
    }

    [TestMethod]
    public void Delete_RegularUser_ThrowsUnauthorized()
    {
        var (logic, context) = CreateLogic();
        context.Snacks.Add(new Snack { Name = "Popcorn", Price = 3.50m, Quantity = 100 });
        context.SaveChanges();
        var id = context.Snacks.First().Id;

        Assert.ThrowsException<UnauthorizedAccessException>(() => logic.Delete(id, RegularUser));
    }

    [TestMethod]
    public void Delete_NonExistentSnack_ThrowsException()
    {
        var (logic, _) = CreateLogic();

        Assert.ThrowsException<Exception>(() => logic.Delete(999, Admin));
    }
}
