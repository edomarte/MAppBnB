// Import necessary namespaces
using System.Diagnostics.CodeAnalysis; // Provides attributes that influence analysis tools (e.g., [DisallowNull])
using MAppBnB; // Assuming this namespace contains the BookingPerson class definition

// Defines a custom equality comparer for the BookingPerson class
public class BookingPersonComparer : IEqualityComparer<BookingPerson>
{
    // Determines whether two BookingPerson instances are equal
    public bool Equals(BookingPerson? x, BookingPerson? y)
    {
        // If both references point to the same object, they are equal
        if (Object.ReferenceEquals(x, y)) return true;

        // If either object is null, they are not equal
        if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null)) return false;

        // Two BookingPerson objects are considered equal if both BookingID and PersonID match
        return x.BookingID == y.BookingID && x.PersonID == y.PersonID;
    }

    // Generates a hash code for a BookingPerson object
    public int GetHashCode([DisallowNull] BookingPerson obj)
    {
        // Combines the hash codes of BookingID and PersonID using bitwise XOR (^)
        // This ensures that equal objects will have the same hash code
        return obj.BookingID.GetHashCode() ^ obj.PersonID.GetHashCode();
    }
}
