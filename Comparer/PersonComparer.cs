// Import necessary namespaces
using System.Diagnostics.CodeAnalysis; // Provides attributes for nullability enforcement, e.g., [DisallowNull]
using MAppBnB; // Assumes the Person class is defined in this namespace

// Defines a custom equality comparer for the Person class
public class PersonComparer : IEqualityComparer<Person>
{
    // Determines whether two Person instances are equal
    public bool Equals(Person? x, Person? y)
    {
        // If both references point to the same object, they are equal
        if (Object.ReferenceEquals(x, y)) return true;

        // If either object is null, they are not equal
        if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null)) return false;

        // Two Person objects are considered equal if their IDs match
        return x.id == y.id;
    }

    // Generates a hash code for a Person object
    public int GetHashCode([DisallowNull] Person person)
    {
        // Returns the hash code of the person's ID
        // Ensures that equal persons (by ID) will have the same hash code
        return person.id.GetHashCode();
    }
}
