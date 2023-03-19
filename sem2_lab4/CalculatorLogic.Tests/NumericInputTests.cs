namespace CalculatorLogic.Tests;

public class NumericInputTests
{
    private readonly NumberInputModel _input = new();

    [Fact]
    public void InitialValueIsZero()
    {
        Assert.Equal(0, _input.GetValue());
    }

    private void Add12_34()
    {
        _input.AddDigit('1');
        _input.AddDigit('2');
        _input.AddDot();
        _input.AddDigit('3');
        _input.AddDigit('4');
    }

    [Fact]
    public void ConversionToDoubleWorksCorrectly()
    {
        Add12_34();
        double value = _input.GetValue();
        Assert.Equal(12.34, value);
    }

    [Fact]
    public void ClearWorksCorrectly()
    {
        Add12_34();
        string value = _input.GetDisplayString();
        Assert.Equal("12.34", value);
    }
    
    [Fact]
    public void SettingFromStringWorks()
    {
        _input.ReplaceWithString("12.34");
        string value = _input.GetDisplayString();
        Assert.Equal("12.34", value);
    }
    
    [Fact]
    public void SettingFromString_MultipleTimes_SetsTheValueToLast()
    {
        _input.ReplaceWithString("12.34");
        _input.ReplaceWithString("56.78");
        string value = _input.GetDisplayString();
        Assert.Equal("56.78", value);
    }
}