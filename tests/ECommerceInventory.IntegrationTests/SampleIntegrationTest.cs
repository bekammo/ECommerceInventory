namespace ECommerceInventory.IntegrationTests;

public class SampleIntegrationTest
{
    [Fact]
    public void SampleTest_ShouldPass()
    {
        // Arrange
        var expected = 4;

        // Act
        var actual = 2 + 2;

        // Assert
        Assert.Equal(expected, actual);
    }
}
