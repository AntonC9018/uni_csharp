using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace CalculatorLogic;

public static class InputMutations
{
    public static void ApplyOperation(this ICalculatorDataModel c, Operation op)
    {
        if (c.NumberInputModel.HasBeenTouchedSinceFlushing == false)
        {
            // (a +) --> (a -)
            if (op.IsBinary)
            {
                // Haven't started on the first input, default to 0
                c.StoredNumber ??= 0;
                c.QueuedOperation = op;
                return;
            }
            
            // (a +) --> (sqrt(a)) --> (b)
            // if (op.IsUnary)
            {
                double a = c.StoredNumber ?? 0;
                double b = op.GetResult(a);
                c.QueuedOperation = null;
                c.StoredNumber = b;
                return;
            }
        }
        
        double currentValue = c.NumberInputModel.GetValue();

        // (a + b) --> (a + sqrt(b)) --> (a + c) --> (d)
        if (op.IsUnary)
            currentValue = op.GetResult(currentValue);
        
        // (a + b) --> (c)
        if (c.QueuedOperation is not null)
            currentValue = c.QueuedOperation.GetResult(c.StoredNumber!.Value, currentValue);

        c.StoredNumber = currentValue;
        c.QueuedOperation = op.IsBinary ? op : null; // (c) --> (c *)
        c.NumberInputModel.Clear();
    }
    
    public static void AddDigit(this INumberInputModel input, char digit)
    {
        if (input.HasDot)
            input.DigitsAfterDot.Add(digit);
        else if (input.DigitsBeforeDot is ['0'])
            input.DigitsBeforeDot[0] = digit;
        else
            input.DigitsBeforeDot.Add(digit);
        
        input.HasBeenTouchedSinceFlushing = true;
    }

    public static void AddDot(this INumberInputModel input)
    {
        input.HasBeenTouchedSinceFlushing = true;
        input.HasDot = true;

        if (input.DigitsBeforeDot.Count == 0)
            input.DigitsBeforeDot.Add('0');
    }

    public static void ChangeSign(this INumberInputModel input)
    {
        input.IsSigned = !input.IsSigned;
        input.HasBeenTouchedSinceFlushing = true;

        if (input.DigitsBeforeDot.Count == 0)
            input.DigitsBeforeDot.Add('0');
    }

    public static void ClearInput(this ICalculatorDataModel c)
    {
        c.NumberInputModel.Clear();
        c.QueuedOperation = null;
        c.StoredNumber = null;
    }

    public static void FlushInput(this ICalculatorDataModel c)
    {
        if (c.NumberInputModel.HasBeenTouchedSinceFlushing == false)
            return;
        
        double currentValue = c.NumberInputModel.GetValue();
        c.NumberInputModel.Clear();

        if (c.QueuedOperation is not null)
            currentValue = c.QueuedOperation.GetResult(c.StoredNumber!.Value, currentValue);
        
        c.StoredNumber = currentValue;
        c.QueuedOperation = null;
    }
    
    public static void ClearLastInput(this INumberInputModel input)
    {
        if (input.HasBeenTouchedSinceFlushing == false)
            return;

        if (input.HasDot)
        {
            if (input.DigitsAfterDot.Count == 0)
            {
                input.HasDot = false;
                return;
            }

            input.DigitsAfterDot.RemoveAt(input.DigitsAfterDot.Count - 1);
            return;
        }

        if (input.DigitsBeforeDot.Count == 0)
        {
            Debug.Assert(input.IsSigned);
            input.IsSigned = false;
            return;
        }

        input.DigitsBeforeDot.RemoveAt(input.DigitsBeforeDot.Count - 1);
    }
    
    
    public static void Clear(this INumberInputModel input)
    {
        input.HasBeenTouchedSinceFlushing = false;
        input.HasDot = false;
        input.IsSigned = false;
        input.DigitsBeforeDot.Clear();
        input.DigitsAfterDot.Clear();
    }
    
    public static void ReplaceWithString(this INumberInputModel input, ReadOnlySpan<char> replacement)
    {
        input.Clear();
        
        if (replacement.Length == 0)
            return;
        
        input.HasBeenTouchedSinceFlushing = true;

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
        {
            if (input.DigitsBeforeDot.Count == 0 && digit == '0')
                continue;
            input.DigitsBeforeDot.Add(digit);
        }

        if (input.DigitsBeforeDot.Count == 0)
            input.DigitsBeforeDot.Add('0');
    }
}


public static class NumberInputHelper
{
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
        if (input.HasBeenTouchedSinceFlushing == false)
            return "";
        
        var sb = _StringBuilder;
        // Just in case there are multiple threads
        lock (sb)
        {
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
            
            var result = sb.ToString();
            sb.Clear();
            return result;
        }
    }
}

public static class DisplayHelper
{
    public static string GetInputDisplayString(this ICalculatorDataModel c)
    {
        if (c.NumberInputModel.HasBeenTouchedSinceFlushing)
            return c.NumberInputModel.GetDisplayString();
        if (c.StoredNumber is null)
            return "";
        // G means ignore trailing zeros
        return c.StoredNumber.Value.ToString("G", CultureInfo.InvariantCulture);
    }
}