using WebMonitor.Extensions;

namespace WebMonitorTests;

public class VersionExtensionsTests
{
    [Theory]
    [InlineData("1.0.0.0", "1.0")]
    [InlineData("1.0.1.0", "1.0.1")]
    [InlineData("1.0.0.1", "1.0.0.1")]
    [InlineData("0.1", "0.1")]
    [InlineData("1.1.0", "1.1")]
    public void ToShortStringReturnsCorrectValue(string value, string expected)
    {
        var shortString = new Version(value).ToShortString();
        
        Assert.Equal(expected, shortString);
    }
}