using Xunit;

namespace UnitTests
{
    public class CalculatorTests
    {
        [Fact]
        public void AddShouldReturnCorrectSum()
        {
            // Arrange
            Calculator calculator = new Calculator();
            int a = 5;
            int b = 10;
            int expectedResult = 15;

            // Act
            int result = calculator.Add(a, b);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}