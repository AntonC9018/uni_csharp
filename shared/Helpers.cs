using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Shared;

public static class RandomHelper
{
    public static void Shuffle<T>(this Random rand, Span<T> array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int j = rand.Next(0, array.Length);
            Helper.Swap(ref array[i], ref array[j]);
        }
    }
}

public enum ArrayInitializationKind
{
    Random,
    Console,
}

public static class InputHelper
{
    public static ArrayInitializationKind? ReadInitializationKind()
    {
        while (true)
        {
            Console.Write("Should I generate the values randomly, or read them from the console? (random / console): ");
            string? input = Console.ReadLine();
            switch (input)
            {
                case "random":
                    return ArrayInitializationKind.Random;
                case "console":
                    return ArrayInitializationKind.Console;
                case "q" or null:
                    return null;
                default:
                    Console.WriteLine($"Invalid input: '{input}'");
                    continue;
            }
        }
    }

    public readonly struct OptionSet
    {
        public readonly IContains<string> Set;
        public readonly string VisualRepresentation;

        public OptionSet(IContains<string> options)
        {
            Set = options;
            VisualRepresentation = string.Join(" / ", options);
        }

        public OptionSet(params string[] options)
            : this(new HashSetWrapper<string>(options))
        {
        }
    }

    public static string? ReadOption(string prompt, OptionSet options, string exitCue = "q")
    {
        if (options.Set.Contains(exitCue))
            Debug.Fail($"The exit cue {exitCue} cannot be one of the options {options.VisualRepresentation}");
        prompt = $"{prompt} ({options.VisualRepresentation}, {exitCue} to exit): ";
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (input is null || input == exitCue)
                return null;
            if (options.Set.Contains(input))
                return input;
            Console.WriteLine(input + " is not one of the options.");
        }
    }

    public sealed class PositiveNumberConstraint : IInputConstraint<int>
    {
        public static readonly PositiveNumberConstraint Instance = new();
        private PositiveNumberConstraint(){}
        public bool Check(int parsedValue)
        {
            return parsedValue >= 0;
        }

        public string FormatError(string rawValue, int parsedValue)
        {
            return rawValue + " should be positive.";
        }
    }

    public sealed class LowerBoundConstraint : IInputConstraint<int>
    {
        public int LowerBound { get; set; }

        public LowerBoundConstraint(int lowerBound)
        {
            LowerBound = lowerBound;
        }

        public bool Check(int parsedValue)
        {
            return parsedValue >= LowerBound;
        }

        public string FormatError(string rawValue, int parsedValue)
        {
            return rawValue + " should be larger than " + LowerBound;
        }
    }

    public sealed class UpperBoundConstraint : IInputConstraint<int>
    {
        public int UpperBound { get; set; }

        public UpperBoundConstraint(int upperBound)
        {
            UpperBound = upperBound;
        }

        public bool Check(int parsedValue)
        {
            return parsedValue <= UpperBound;
        }

        public string FormatError(string rawValue, int parsedValue)
        {
            return rawValue + " should be smaller or equal to " + UpperBound;
        }
    }

    private static readonly IInputConstraint<int>[] PositiveNumberConstraints = { PositiveNumberConstraint.Instance };
    public static int? GetPositiveNumber(string message)
    {
        return GetNumberWithConstraints(message, PositiveNumberConstraints);
    }

    public static int? GetNumberWithConstraints(string message, ReadOnlySpan<IInputConstraint<int>> constraints)
    {
        while (true)
        {
            Console.Write(message);
            string? input = Console.ReadLine();
            switch (input)
            {
                case "q" or null:
                    return null;
                default:
                {
                    if (int.TryParse(input, out int value))
                    {
                        bool bad = false;
                        foreach (var constraint in constraints)
                        {
                            if (!constraint.Check(value))
                            {
                                bad = true;
                                var error = constraint.FormatError(input, value);
                                Console.WriteLine(error);
                            }
                        }
                        if (bad)
                            continue;
                        return value;
                    }

                    Console.WriteLine($"'{input}' is not a number");
                    continue;
                }
            }
        }
    }

    public interface IInputConstraint<in T>
    {
        string FormatError(string rawValue, T? parsedValue);
        bool Check(T parsedValue);
    }
}

public interface IContains<TValue> : IEnumerable<TValue> 
{
    bool Contains(TValue item);
}

public class HashSetWrapper<TValue> : HashSet<TValue>, IContains<TValue>
{
    public HashSetWrapper()
    {
    }

    public HashSetWrapper(IEnumerable<TValue> collection) : base(collection)
    {
    }

    public HashSetWrapper(IEqualityComparer<TValue>? comparer) : base(comparer)
    {
    }

    public HashSetWrapper(int capacity) : base(capacity)
    {
    }

    public HashSetWrapper(IEnumerable<TValue> collection, IEqualityComparer<TValue>? comparer) : base(collection, comparer)
    {
    }

    public HashSetWrapper(int capacity, IEqualityComparer<TValue>? comparer) : base(capacity, comparer)
    {
    }

    protected HashSetWrapper(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

public static class Helper
{
    public static void Swap<T>(ref T a, ref T b)
    {
        T t = a;
        a = b;
        b = t;
    }

    public static int GetNumDecimalDigits(int value)
    {
        int numDigits = 1;
        int current = value;
        while ((current /= 10) != 0)
            numDigits++;
        return numDigits;
    }
}
