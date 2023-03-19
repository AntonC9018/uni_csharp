namespace CalculatorLogic.Tests;

public class CalculatorTests
{
    [Fact]
    public void MultiplicationTest()
    {
        var model = new CalculatorDataModel();
        // 0 *
        model.ApplyOperation(Registry.Operations["*"]);
        
        Assert.Equal(0, model.StoredNumber);
        Assert.NotNull(model.QueuedOperation);

        model.StoredNumber = 2;
        model.NumberInputModel.AddDigit('1');
        model.NumberInputModel.AddDot();
        model.NumberInputModel.AddDigit('5');
        
        model.FlushInput();
        
        Assert.Equal(3, model.StoredNumber);
        Assert.Null(model.QueuedOperation);
    }
}