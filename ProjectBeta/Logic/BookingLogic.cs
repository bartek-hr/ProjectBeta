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
            throw new Exception(l10n("reservations.errors.booking_not_found"));

        return booking;
    }


    public decimal DetermineTotalPrice(List<int> selectedSeatTypes)
    {
        decimal totalPrice = 0.0m;
        foreach (int selectedSeatType in selectedSeatTypes){
            if (selectedSeatType == 1) {
                totalPrice = totalPrice + 15.00m;
            }
            if (selectedSeatType == 2) {
                totalPrice = totalPrice + 17.50m;
            }
            if (selectedSeatType == 3) {
                totalPrice = totalPrice + 20.00m;
            }                        
        }
        return totalPrice;
    }
    public void CreateBooking(Booking booking)
    {
        if (booking.TotalPrice <= 0)
            throw new Exception(l10n("reservations.errors.total_price_positive"));

        if (booking.UserId <= 0)
            throw new Exception(l10n("reservations.errors.invalid_user"));

        if (booking.AuditoriumId <= 0)
            throw new Exception(l10n("reservations.errors.invalid_screening"));
    
        booking.CreatedAt = DateTime.Now;

           
        booking.Paid = false;

        _bookingAccess.Add(booking);
    }
    public void CreateBooking(int userId, decimal totalPrice, int auditoriumId, string seats, int discountID, string movie, DateTime createdAt )
    {
        var booking = new Booking
        {
            UserId = userId,
            Seats = seats,
            DiscountId = discountID,
            AuditoriumId = auditoriumId,
            TotalPrice = totalPrice,
            CreatedAt = createdAt,
            ScreeningId = 1,
            Movie = movie,
            Paid = false
        };

        _bookingAccess.Add(booking);
    }
    public void MarkAsPaid(int bookingId)
    {
        var booking = _bookingAccess.GetById(bookingId);

        if (booking == null)
            throw new Exception(l10n("reservations.errors.booking_not_found"));

        booking.Paid = true;

        _bookingAccess.Update(booking);
    }

    public List<Booking> GetBookingsByCreatedAtAndAuditoriumID(DateTime createdAt, int auditoriumId)
    {
        return _bookingAccess.GetAll()
            .Where(b => b.AuditoriumId == auditoriumId && b.CreatedAt == createdAt)
            .ToList();
    }

    public List<Booking> GetBookingsByUserId(int userId)
    {
        return _bookingAccess.GetAll()
            .Where(b => b.UserId == userId)
            .ToList();
    }

    public void DeleteBooking(int id)
    {
        _bookingAccess.Delete(id);
    }
}
