using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace sem2_lab2;

public record NamedObject<T>(string DisplayName, T Value);
public record CollectionValues(IEnumerable<NamedObject<object>> Values);

public static class Collections
{
    public static IReadOnlyList<NamedObject<CollectionValues>> All
    {
        get
        {
            var list = new List<NamedObject<CollectionValues>>();
            Add(new PersonAsCollection());
            Add(new StudentAsCollection());
            Add(new PersonAsList());
            Add(new AnimalAsEnumerable());
            Add(new EnumerableShark().AsEnumerable(), nameof(EnumerableShark));

    
            void Add(IEnumerable v, string? name = null)
            {
                name ??= v.GetType().Name;
                if (v is not IEnumerable<NamedObject<object>> c)
                    c = Wrap(v);
                list.Add(new(name, new CollectionValues(c)));
            }

            return list;
        }
    }

    private static IEnumerable<NamedObject<object>> Wrap(IEnumerable e)
    {
        foreach (var obj in e)
            yield return (NamedObject<object>) obj;
    }
}


// Enumeration of properties as object with yield
// Will need to be wrapper, since it doesn't implement
// the IEnumerable interface my other code expects.
public sealed class PersonAsCollection : IEnumerable
{
    public string Name { get; set; } = "Anton";
    public string Surname { get; set; } = "Smith";
    public int Age { get; set; } = 10;
    public string Occupation { get; set; } = "Student";
    
    public IEnumerator GetEnumerator()
    {
        yield return new NamedObject<object>(nameof(Name), Name);
        yield return new NamedObject<object>(nameof(Surname), Surname);
        yield return new NamedObject<object>(nameof(Age), Age);
        yield return new NamedObject<object>(nameof(Occupation), Occupation);
    }
}

// Enumeration of properties with a custom enumerator
// Will need to be wrapper, since it doesn't implement
// the IEnumerable interface my other code expects.
public sealed class StudentAsCollection : IEnumerable
{
    public string Name { get; set; } = "Anton";
    public string Surname { get; set; } = "Smith";
    public string University { get; set; } = "BSUIR";
    public string Faculty { get; set; } = "FIT";
    public float AverageGrade { get; set; } = 4.5f;
    public int Year { get; set; } = 3;
    
    public IEnumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    private class Enumerator : IEnumerator
    {
        private const int _PropCount = 6;
        private int _propIndex = -1;
        private StudentAsCollection _object;

        public Enumerator(StudentAsCollection o)
        {
            _object = o;
        }

        public bool MoveNext()
        {
            _propIndex++;
            return _propIndex < _PropCount;            
        }

        public void Reset()
        {
            _propIndex = -1;
        }

        public object Current => _propIndex switch
        {
            0 => new NamedObject<object>(nameof(Name), _object.Name),
            1 => new NamedObject<object>(nameof(Surname), _object.Surname),
            2 => new NamedObject<object>(nameof(University), _object.University),
            3 => new NamedObject<object>(nameof(Faculty), _object.Faculty),
            4 => new NamedObject<object>(nameof(AverageGrade), _object.AverageGrade),
            5 => new NamedObject<object>(nameof(Year), _object.Year),
            _ => throw new InvalidOperationException()
        };
    }
}

// Inheriting from a list of objects
public sealed class PersonAsList : List<NamedObject<object>>
{
    private static string Name { get; set; } = "John";
    private static string Surname { get; set; } = "Smith";
    private static int Age { get; set; } = 18;
    private static string Occupation { get; set; } = "Hard Worker";

    public PersonAsList()
    {
        Add(new NamedObject<object>(nameof(Name), Name));
        Add(new NamedObject<object>(nameof(Surname), Surname));
        Add(new NamedObject<object>(nameof(Age), Age));
        Add(new NamedObject<object>(nameof(Occupation), Occupation));
    }
}


// Implementing the generic IEnumerable interface
public sealed class AnimalAsEnumerable : IEnumerable<NamedObject<object>>
{
    public string Species { get; set; } = "Dog";
    public string Name { get; set; } = "Bob";
    public int Age { get; set; } = 3;
    public string Owner { get; set; } = "John";
    
    public IEnumerator<NamedObject<object>> GetEnumerator()
    {
        yield return new NamedObject<object>(nameof(Species), Species);
        yield return new NamedObject<object>(nameof(Name), Name);
        yield return new NamedObject<object>(nameof(Age), Age);
        yield return new NamedObject<object>(nameof(Owner), Owner);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}


// Just a custom enumerator without the IEnumerable interface,
// so use structural typing to support foreach, which we later wrap to implement the interface.
public sealed class EnumerableShark
{
    public string Name { get; set; } = "Sharky";
    public int Age { get; set; } = 5;
    public string Species { get; set; } = "Great White";
    public int PeopleEaten { get; set; } = 10;
    public bool IsPeaceful => PeopleEaten == 0;
    
    public Enumerator GetEnumerator()
    {
        return new(this);
    }
    
    public class Enumerator
    {
        private const int _PropCount = 5;
        private int _propIndex = -1;
        private EnumerableShark _object;

        public Enumerator(EnumerableShark o)
        {
            _object = o;
        }

        public bool MoveNext()
        {
            _propIndex++;
            return _propIndex < _PropCount;            
        }

        public NamedObject<object> Current => _propIndex switch
        {
            0 => new NamedObject<object>(nameof(Name), _object.Name),
            1 => new NamedObject<object>(nameof(Age), _object.Age),
            2 => new NamedObject<object>(nameof(Species), _object.Species),
            3 => new NamedObject<object>(nameof(PeopleEaten), _object.PeopleEaten),
            4 => new NamedObject<object>(nameof(IsPeaceful), _object.IsPeaceful),
            _ => throw new InvalidOperationException()
        };
    }
    
    // Just provide the wrapped version here
    public IEnumerable<NamedObject<object>> AsEnumerable()
    {
        foreach (var item in this)
            yield return item;
    }
}