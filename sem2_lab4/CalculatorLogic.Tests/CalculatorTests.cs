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

    [Fact]
    public void IntegrationTest()
    {
        var model = new CalculatorDataModel();
        
        model.NumberInputModel.AddDigit('1');
        model.NumberInputModel.AddDigit('2');
        
        model.ApplyOperation(Registry.Operations["+"]);
        Assert.Equal(12, model.StoredNumber);
        Assert.False(model.NumberInputModel.HasBeenTouchedSinceFlushing);
        Assert.Equal("12", model.GetInputDisplayString());
        
        model.NumberInputModel.AddDigit('3');
        Assert.Equal("3", model.GetInputDisplayString());

        model.NumberInputModel.AddDigit('4');
        model.NumberInputModel.AddDot();
        model.NumberInputModel.AddDigit('5');
        
        model.FlushInput();
        Assert.Equal(46.5, model.StoredNumber);
        Assert.False(model.NumberInputModel.HasBeenTouchedSinceFlushing);
        Assert.Equal("46.5", model.GetInputDisplayString());
        
        model.NumberInputModel.AddDigit('4');
        model.NumberInputModel.ClearLastInput();
        Assert.Equal("", model.GetInputDisplayString());
    }

    [Fact]
    public void FlushAfterOperation_StoresTheNumberAndSomeMoreStuffICantPutIntoWords()
    {
        var model = new CalculatorDataModel();
        model.NumberInputModel.ReplaceWithString("12");
        model.ApplyOperation(Registry.Operations["+"]);
        model.NumberInputModel.ReplaceWithString("24");
        model.FlushInput();
        Assert.Equal(36, model.StoredNumber);
        model.ApplyOperation(Registry.Operations["+"]);
        Assert.Equal(36, model.StoredNumber);
        model.NumberInputModel.ReplaceWithString("24");
        model.FlushInput();
        Assert.Equal(60, model.StoredNumber);
        Assert.Equal("60", model.GetInputDisplayString());
    }

    [Fact]
    public void ApplyingUnaryOperationTwice_AppliesItTwice()
    {
        var model = new CalculatorDataModel();
        model.NumberInputModel.ReplaceWithString("16");
        model.ApplyOperation(Registry.Operations["√"]);
        Assert.Equal(4, model.StoredNumber);
        model.ApplyOperation(Registry.Operations["√"]);
        Assert.Equal(2, model.StoredNumber);
    }
}