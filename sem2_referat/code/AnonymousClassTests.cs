

using Xunit;

public class AnonymousClassTests
{
    [Fact]
    public static void AnonymousClasses_OverloadEqualsOperator()
    {
        static bool EqualsCheck<T>(T a, T b) => a!.Equals(b);
        bool r = EqualsCheck(new { Age = 5 }, new { Age = 5 });
        Assert.True(r);
    }
    
    [Fact]
    public static void AnonymousClasses_EqualityComparerDoesNotJustCheckReferences()
    {
        static bool EqualsCheckComparer<T>(T a, T b)
        {
            var comparer = EqualityComparer<T>.Default;
            bool result = comparer.Equals(a, b);
            return result;
        }
        bool r = EqualsCheckComparer(new { Age = 5 }, new { Age = 5 });
        Assert.True(r);
    }
}