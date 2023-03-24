using System.Diagnostics;

namespace CalculatorLogic;

public record Operation(string Text, Delegate Action, bool IsBinary)
{
    public bool IsUnary => !IsBinary;

    public static Operation Create(string text, Func<double, double> unaryFunc)
    {
        return new Operation(text, unaryFunc, false);
    }

    public static Operation Create(string text, Func<double, double, double> binaryFunc)
    {
        return new Operation(text, binaryFunc, true);
    }

    public double GetResult(double arg)
    {
        Debug.Assert(IsUnary);
        return ((Func<double, double>) Action)(arg);
    }

    public double GetResult(double lhs, double rhs)
    {
        Debug.Assert(IsBinary);
        return ((Func<double, double, double>) Action)(lhs, rhs);
    }
}

public static class Registry
{
    public static readonly IReadOnlyDictionary<string, Operation> Operations = GetOperations().ToDictionary(op => op.Text);
    
    private static IEnumerable<Operation> GetOperations()
    {
        yield return Operation.Create("+", (a, b) => a + b);
        yield return Operation.Create("-", (a, b) => a - b);
        yield return Operation.Create("*", (a, b) => a * b);
        yield return Operation.Create("/", (a, b) => a / b);
        yield return Operation.Create("√", Math.Sqrt);
        yield return Operation.Create("%", a => a * 100);
        yield return Operation.Create("x⁻¹", a => 1 / a);
    }
}
