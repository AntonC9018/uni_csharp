using System.Diagnostics;

namespace CalculatorLogic;

public record NumberInputModel : INumberInputModel
{
    public bool IsSigned { get; set; }
    public bool HasDot { get; set; }
    public bool HasBeenTouchedSinceFlushing { get; set; }
    public List<char> DigitsBeforeDot { get; } = new();
    public List<char> DigitsAfterDot { get; } = new();
    
    IList<char> INumberInputModel.DigitsBeforeDot => DigitsBeforeDot;
    IList<char> INumberInputModel.DigitsAfterDot => DigitsAfterDot;
}

public sealed class CalculatorDataModel : ICalculatorDataModel
{
    public double? StoredNumber { get; set; }
    public NumberInputModel NumberInputModel { get; } = new();
    public Operation? QueuedOperation { get; set; }
    
    INumberInputModel ICalculatorDataModel.NumberInputModel => NumberInputModel;
}