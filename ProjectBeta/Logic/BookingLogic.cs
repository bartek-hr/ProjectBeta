using System;
using System.Collections.Generic;
using ProjectBeta.Model;
using ProjectBeta.Access;

namespace ProjectBeta.Logic;

public class BookingLogic
{
    private readonly BookingAccess _bookingAccess;

    public BookingLogic(BookingAccess bookingAccess)
    {
        _bookingAccess = bookingAccess;
    }

    public List<Booking> GetBookings()
    {
        return _bookingAccess.GetAll();
    }

    public Booking GetBooking(int id)
    {
        var booking = _bookingAccess.GetById(id);

        if (booking == null)
            throw new Exception("Booking not found");

        return booking;
    }

    public void CreateBooking(Booking booking)
    {
        if (booking.Total_Price <= 0)
            throw new Exception("Total price must be greater than 0");

        if (booking.User_Id <= 0)
            throw new Exception("Invalid user");

        if (booking.Screening_ID <= 0)
            throw new Exception("Invalid screening");
    
        booking.CreatedAt = DateTime.Now;

           
        booking.Paid = false;

        _bookingAccess.Add(booking);
    }

    public void MarkAsPaid(int bookingId)
    {
        var booking = _bookingAccess.GetById(bookingId);

        if (booking == null)
            throw new Exception("Booking not found");

        booking.Paid = true;

        _bookingAccess.Update(booking);
    }

    public void DeleteBooking(int id)
    {
        _bookingAccess.Delete(id);
    }
}