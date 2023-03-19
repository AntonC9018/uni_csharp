using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace CalculatorLogic;

public static class InputMutations
{
    public static void ApplyOperation(this ICalculatorDataModel c, Operation op)
    {
        if (c.NumberInputModel.IsEmpty())
        {
            // (a +) --> (a -)
            // Done first input previously, haven't started the second one yet,
            // replace the binary operation.
            if (op.IsBinary)
            {
                c.QueuedOperation = op;
                if (c.QueuedOperation is null)
                    c.StoredNumber = 0;
                return;
            }
            
            // (a +) --> (sqrt(a)) --> (b)
            // if (op.IsUnary)
            {
                double a = c.QueuedOperation is null ? 0 : c.StoredNumber;
                double result = op.GetResult(a);
                c.QueuedOperation = null;
                c.StoredNumber = result;
                return;
            }
        }

        double currentValue = c.NumberInputModel.GetValue();

        if (op.IsUnary)
            currentValue = op.GetResult(currentValue);
        
        if (c.QueuedOperation is not null)
            currentValue = c.QueuedOperation.GetResult(c.StoredNumber, currentValue);

        c.StoredNumber = currentValue;
        c.QueuedOperation = op.IsBinary ? op : null;
        c.NumberInputModel.Clear();
    }

    public static void AddDigit(this INumberInputModel input, char digit)
    {
        if (input.HasDot)
            input.DigitsAfterDot.Add(digit);
        else
            input.DigitsBeforeDot.Add(digit);
    }

    public static void AddDot(this INumberInputModel input)
    {
        input.HasDot = true;

        if (input.DigitsBeforeDot.Count == 0)
            input.DigitsBeforeDot.Add('0');
    }

    public static void ChangeSign(this INumberInputModel input)
    {
        input.IsSigned = !input.IsSigned;
        
        if (input.DigitsBeforeDot.Count == 0)
            input.DigitsBeforeDot.Add('0');
    }

    public static void ClearInput(this ICalculatorDataModel c)
    {
        c.NumberInputModel.Clear();
        c.QueuedOperation = null;
        c.StoredNumber = 0;
    }

    public static void FlushInput(this ICalculatorDataModel c)
    {
        if (c.NumberInputModel.IsEmpty())
            return;
        
        double currentValue = c.NumberInputModel.GetValue();
        c.NumberInputModel.Clear();

        if (c.QueuedOperation is not null)
            currentValue = c.QueuedOperation.GetResult(c.StoredNumber, currentValue);
        
        c.StoredNumber = currentValue;
        c.QueuedOperation = null;
    }
    
    public static void ClearLastInput(this INumberInputModel input)
    {
        if (input.IsEmpty())
            return;

        if (input.HasDot)
        {
            if (input.DigitsAfterDot.Count == 0)
            {
                input.HasDot = false;
                return;
            }

            input.DigitsAfterDot.RemoveAt(input.DigitsAfterDot.Count - 1);
        }

        if (input.DigitsBeforeDot is [_])
            input.DigitsBeforeDot[0] = '0';
        else
            input.DigitsBeforeDot.RemoveAt(input.DigitsBeforeDot.Count - 1);
    }
    
    
    public static void Clear(this INumberInputModel input)
    {
        input.IsSigned = false;
        input.DigitsBeforeDot.Clear();
        input.DigitsAfterDot.Clear();
    }
    
    public static void ReplaceWithString(this INumberInputModel input, ReadOnlySpan<char> replacement)
    {
        input.Clear();
        
        if (replacement.Length == 0)
            return;

        if (replacement[0] == '-')
        {
            input.IsSigned = true;
            replacement = replacement[1..];
            if (replacement.Length == 0)
                input.DigitsBeforeDot.Add('0');
        }
            
        int dotIndex = replacement.IndexOf('.');
        ReadOnlySpan<char> digitsBeforeDot;
        if (dotIndex != -1)
        {
            foreach (char digit in replacement[(dotIndex + 1)..])
                input.DigitsAfterDot.Add(digit);
            input.HasDot = true;
            digitsBeforeDot = replacement[..dotIndex];
        }
        else
        {
            digitsBeforeDot = replacement;
        }
        
        foreach (var digit in digitsBeforeDot)
            input.DigitsBeforeDot.Add(digit);
    }
}


public static class NumberInputHelper
{
    public static bool IsEmpty(this INumberInputModel input)
    {
        return input.DigitsBeforeDot.Count == 0;
    }

    public static double GetValue(this INumberInputModel input)
    {
        double whole = 0;
        foreach (var digit in input.DigitsBeforeDot)
        {
            whole *= 10;
            whole += digit - '0';
        }

        double fraction = 0;
        foreach (var digit in input.DigitsAfterDot.Reverse())
        {
            fraction += digit - '0';
            fraction /= 10;
        }

        double number = whole + fraction;
        if (input.IsSigned)
            number = -number;
        
        return number;
    }
    

    private static readonly StringBuilder _StringBuilder = new();
    
    public static string GetDisplayString(this INumberInputModel input)
    {
        if (input.IsEmpty())
            return "0";

        var sb = _StringBuilder;
        if (input.IsSigned)
            sb.Append('-');
        foreach (var digit in input.DigitsBeforeDot)
            sb.Append(digit);
        
        if (input.HasDot)
        {
            sb.Append('.');
            foreach (var digit in input.DigitsAfterDot)
                sb.Append(digit);
        }

        return sb.ToString();
    }
}

public static class DisplayHelper
{
    public static string GetInputDisplayString(this ICalculatorDataModel c)
    {
        if (c.NumberInputModel.IsEmpty())
            return c.StoredNumber.ToString(CultureInfo.InvariantCulture);
        return c.NumberInputModel.GetDisplayString();
    }
}