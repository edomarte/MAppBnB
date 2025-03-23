using System.ComponentModel.DataAnnotations;
using MAppBnB;

public class CheckOutDateValidator : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        Booking booking = (Booking)validationContext.ObjectInstance;
        DateTime checkOutDate = (DateTime)value;
        DateTime checkInDate = booking.CheckinDateTime;

        double daysDifference=(checkOutDate.Date - checkInDate.Date).TotalDays;

        if (daysDifference < 1)
        {
            return new ValidationResult("The checkout date must be at least one day after the checkin date.");
        }

        if (daysDifference > 30)
        {
            return new ValidationResult("The booking cannot be longer than 30 days.");
        }

        return ValidationResult.Success;
    }
}