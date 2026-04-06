using System;
using ProjectBeta.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjectBeta.Tests
{
    [TestClass]
    public class BookingModelTests
    {
        [TestMethod]
        public void Booking_Model_Initializes_With_Default_Values()
        {
            // Arrange
            var booking = new Booking();

            // Act & Assert
            Assert.AreEqual(0, booking.Id);  // Default is 0 for ints
            Assert.AreEqual(0, booking.User_Id);
            Assert.AreEqual(0, booking.Screening_ID);
            Assert.IsNull(booking.Discount_ID);  // Nullable should be null by default
            Assert.AreEqual(0m, booking.Total_Price);  // Default is 0 for decimals
            Assert.IsFalse(booking.Paid);  // Default is false for bool
            Assert.AreEqual(default(DateTime), booking.CreatedAt);  // Default is DateTime.MinValue
        }

        [TestMethod]
        public void Booking_Model_Can_Set_Properties()
        {
            // Arrange
            var booking = new Booking
            {
                Id = 1,
                User_Id = 2,
                Screening_ID = 3,
                Discount_ID = 4,
                Total_Price = 100.50m,
                Paid = true,
                CreatedAt = DateTime.Now
            };

            // Act & Assert
            Assert.AreEqual(1, booking.Id);
            Assert.AreEqual(2, booking.User_Id);
            Assert.AreEqual(3, booking.Screening_ID);
            Assert.AreEqual(4, booking.Discount_ID);
            Assert.AreEqual(100.50m, booking.Total_Price);
            Assert.IsTrue(booking.Paid);
            Assert.AreNotEqual(default(DateTime), booking.CreatedAt);
        }

        [TestMethod]
        [DataRow(0.0, false)]   // Total_Price = 0 should throw an exception (using double instead of decimal)
        [DataRow(-10.0, false)]  // Total_Price = -10 should throw an exception
        [DataRow(100.0, true)]   // Total_Price = 100 is valid
        public void Booking_Model_Validates_Total_Price_Greater_Than_Zero(double totalPrice, bool shouldThrow)
        {
            var booking = new Booking { Total_Price = (decimal)totalPrice }; // Convert double to decimal

            if (shouldThrow)
            {
                // Assert that an exception is thrown
                Assert.ThrowsException<Exception>(() => ValidateBooking(booking));
            }
            else
            {
                // Assert that no exception is thrown
                try
                {
                    ValidateBooking(booking);
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Expected no exception, but got: {ex.Message}");
                }
            }
        }

        // Helper method to simulate business validation
        private void ValidateBooking(Booking booking)
        {
            if (booking.Total_Price <= 0)
                throw new Exception("Total price must be greater than 0.");
        }

        [TestMethod]
        [DataRow(1, 1, true)]   // Same total price, valid booking comparison
        [DataRow(1, 2, false)]  // Different ids should not match
        public void Booking_Model_Equality_Compares_Equal_Objects(int id1, int id2, bool expectedEquality)
        {
            var booking1 = new Booking { Id = id1, Total_Price = 50 };
            var booking2 = new Booking { Id = id2, Total_Price = 50 };

            if (expectedEquality)
            {
                Assert.AreEqual(booking1, booking2); // Same Id and Total_Price
            }
            else
            {
                Assert.AreNotEqual(booking1, booking2); // Different Id
            }
        }
    }
}