using System.Diagnostics.CodeAnalysis;
using MAppBnB;

public class BookingPersonComparer : IEqualityComparer<BookingPerson>
{
    public bool Equals(BookingPerson? x, BookingPerson? y)
    {
        if (Object.ReferenceEquals(x, y)) return true;
        if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null)) return false;
        return x.BookingID == y.BookingID && x.PersonID == y.PersonID;
    }

    public int GetHashCode([DisallowNull] BookingPerson obj)
    {
        return obj.BookingID.GetHashCode() ^ obj.PersonID.GetHashCode();
    }
}