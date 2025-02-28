using System.Diagnostics.CodeAnalysis;
using MAppBnB;

public class PersonComparer : IEqualityComparer<Person>
{
        public bool Equals(Person? x, Person? y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null)) return false;
            return x.id==y.id;
        }

        public int GetHashCode([DisallowNull] Person person)
        {
            return person.id.GetHashCode();
        }
    }