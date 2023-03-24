using System.Diagnostics;
using CalculatorLogic;

namespace BlazorFrontend;
public static class Registry
{
    public static readonly IReadOnlyDictionary<string, Control> Controls = GetControls().ToDictionary(c => c.Text);
    
    private static IEnumerable<Control> GetControls()
    {
        for (char ch = '0'; ch <= '9'; ch++)
            yield return Control.CreateDigitControl(ch);
        
        yield return Control.CreateDotControl();
        yield return Control.CreateSignControl();
        yield return Control.CreateClearControl();
        yield return Control.CreateClearAllControl();
        
        foreach (var op in CalculatorLogic.Registry.Operations.Values)
            yield return Control.CreateOperationControl(op);

        yield return new Control("←", c => c.NumberInputModel.ClearLastInput());
        yield return new Control("=", c => c.FlushInput());
    }
}