

bool EqualsCheck<T>(T a, T b)
{
    var comparer = EqualityComparer<T>.Default;
    bool result = comparer.Equals(a, b);
    return result;
}

bool r = EqualsCheck(new { Age = 5 }, new { Age = 5 });
Console.WriteLine(r ? "YES" : "NO");