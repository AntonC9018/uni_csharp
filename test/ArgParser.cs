using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class OptionAttribute : Attribute
{
    public string? Name;
    public string HelpMessage;

    public OptionAttribute(string helpMessage)
    {
        HelpMessage = helpMessage;
    }

    public OptionAttribute(string name, string helpMessage)
    {
        Name = name;
        HelpMessage = helpMessage;
    }
}

public class ArgumentModel
{
#pragma warning disable CS0649 // xxx is never assigned to
    [Option("Help message for Hello")]
    public int Hello;

    [Option("OtherName", "A renamed option")]
    public string? Stuff;
#pragma warning restore CS0649
}

public static class ArgumentParser
{
    internal static IEnumerable<(FieldInfo, OptionAttribute)> GetFieldsWithOptions(System.Type type)
    {
        foreach (var field in type.GetFields())
        {
            var optionAttribute = field.GetCustomAttribute<OptionAttribute>();
            if (optionAttribute is null)
                continue;
            
            yield return (field, optionAttribute);
        }
    }

    public static bool TryParse<T>(
        ReadOnlySpan<string> args,
        [NotNullWhen(returnValue: true)] out T? result)
        where T : new()
    {
        result = default;

        // --name value
        Dictionary<string, string> values = new();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--"))
            {
                var name = args[i]["--".Length ..];
                i++;
                if (i >= args.Length)
                {
                    Console.WriteLine("Argument without value: " + name);
                    return false;
                }
                var value = args[i];
                values[name] = value;
                continue;
            }
            Console.WriteLine("Error at " + args[i] + ". Start your arguments with '--'.");
            return false;
        }

        bool isError = false;
        var obj = new T();
        foreach (var (field, optionAttribute) in GetFieldsWithOptions(typeof(T)))
        {
            var optionName = optionAttribute.Name ?? field.Name;
            if (!values.TryGetValue(optionName, out var strValue))
            {
                isError = true;
                Console.WriteLine("No value found for option " + optionName);
                continue;
            }

            if (isError)
                continue;

            if (field.FieldType == typeof(int))
            {
                if (!int.TryParse(strValue, out int v))
                {
                    Console.WriteLine("Could not parse " + strValue + " as int.");
                    isError = true;
                    continue;
                }
                field.SetValue(obj, v);
            }
            else if (field.FieldType == typeof(string))
            {
                field.SetValue(obj, strValue);
            }
            else
            {
                isError = true;
                Console.WriteLine("Unsupported type: " + field.FieldType.FullName);
            }
        }

        if (isError)
            return false;

        result = (T) obj;
        return true;
    }

    public static void GetHelp<T>(StringBuilder builder)
    {
        foreach (var (field, optionAttribute) in GetFieldsWithOptions(typeof(T)))
        {
            builder.Append(optionAttribute.Name ?? field.Name);
            builder.Append(", tipul '");
            builder.Append(field.FieldType.Name);
            builder.Append("', help '");
            builder.Append(optionAttribute.HelpMessage);
            builder.Append("'");
            builder.AppendLine();
        }
    }
}

class Program
{
    static int Main(string[] args)
    {
        if (!ArgumentParser.TryParse(args, out ArgumentModel? model))
        {
            var builder = new StringBuilder();
            ArgumentParser.GetHelp<ArgumentModel>(builder);
            Console.WriteLine(builder.ToString());
            return 1;
        }

        Console.WriteLine("Hello = " + model.Hello);
        Console.WriteLine("Stuff = " + model.Stuff);
        return 0;
    }
}