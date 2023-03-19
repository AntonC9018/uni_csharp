using CalculatorLogic;

namespace BlazorFrontend;

public record Control(string Text, Action<ICalculatorDataModel> Operation)
{
    public static Control CreateOperationControl(string op)
        => new Control(op, c => c.ApplyOperation(CalculatorLogic.Registry.Operations[op]));
    public static Control CreateOperationControl(Operation op)
        => new Control(op.Text, c => c.ApplyOperation(op));
    public static Control CreateDigitControl(char digit)
        => new Control(digit.ToString(), c => c.NumberInputModel.AddDigit(digit));
    public static Control CreateDotControl()
        => new Control(".", c => c.NumberInputModel.AddDot());
    public static Control CreateSignControl()
        => new Control("±", c => c.NumberInputModel.ChangeSign());
    public static Control CreateClearControl()
        => new Control("C", c => c.NumberInputModel.Clear());
    public static Control CreateClearAllControl()
        => new Control("CE", c => c.ClearInput());
}