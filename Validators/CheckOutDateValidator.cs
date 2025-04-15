using System.ComponentModel.DataAnnotations;
using MAppBnB;

public class CheckOutDateValidator : ValidationAttribute
{
    // Method for validating that the checkin and checkout dates in the booking are valid.
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // Get the booking from the context.
        Booking booking = (Booking)validationContext.ObjectInstance;
        // Convert the checkout date and get the checkin date from the booking.
        DateTime checkOutDate = (DateTime)value;
        DateTime checkInDate = booking.CheckinDateTime;

        // Calculate days difference between the dates.
        double daysDifference=(checkOutDate.Date - checkInDate.Date).TotalDays;

        // If checkin date earlier or equal as checkout date, return validation error.
        if (daysDifference < 1)
        {
            return new ValidationResult("The checkout date must be at least one day after the checkin date.");
        }

        // If date difference greater than 30 days, return validation error (Italian law forbid short stays longer than 30 days).
        if (daysDifference > 30)
        {
            return new ValidationResult("The booking cannot be longer than 30 days.");
        }

        // If no errors, return validation success.
        return ValidationResult.Success;
    }
}