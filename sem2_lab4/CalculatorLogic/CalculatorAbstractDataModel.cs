using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace CalculatorLogic;

public interface ICalculatorDataModel
{
    [NotNullIfNotNull(nameof(QueuedOperation))]
    public double? StoredNumber { get; set; }
    public INumberInputModel NumberInputModel { get; }
    public Operation? QueuedOperation { get; set; }
}

// public record CalculationHistory
// {
//     public record struct Item(double Value, Operation? Operation)
//     {
//         public bool IsValue => Operation is null;
//         public bool IsOperation => Operation is not null;
//     }
    
//     public List<Item> Items { get; } = new();
// }

public enum DisplayMode
{
    ShowingResults,
    ShowingInputs,
}

public interface INumberInputModel
{
    IList<char> DigitsBeforeDot { get; }
    IList<char> DigitsAfterDot { get; }
    bool IsSigned { get; set; }
    bool HasDot { get; set; }
    bool HasBeenTouchedSinceFlushing { get; set; }
}
