


using NUnit.Framework;
using OrderEntryMockingPractice.Services;

namespace OrderEntryMockingPracticeTests
{
    [TestFixture]
    public class OrderServiceTests
    {
      
        [Test]
        public static void TestOrderSummaryDoesNotReturnNull()
        {
            //Arrange
            OrderService orderService = new OrderService();
            //Act
            var result = orderService.PlaceOrder(null);
            //Assert
            Assert.That(result,!Is.EqualTo(null));
        }
    }
}
