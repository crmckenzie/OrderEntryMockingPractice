


using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;

namespace OrderEntryMockingPracticeTests
{
    [TestFixture]
    public class OrderServiceTests
    {
      
        [Test]
        public static void TestOrderSummaryDoesNotReturnNull()
        {
            // Arrange
            var orderService = new OrderService();
            var order = new Order
            {
                CustomerId = 1,
            };
            order.OrderItems.Add(new OrderItem());
            // Act
            var result = orderService.PlaceOrder(order);    
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        [ExpectedException( typeof(InvalidDataException), ExpectedMessage="CustomerId Is Null")]
        public static void TestOrderSummaryOrderHasNullCustomerIdThrowsException()
        {
            //Arrange
            var orderService = new OrderService();
            var order = new Order
                {
                    CustomerId = null,
                };
            order.OrderItems.Add(new OrderItem());
            //Act
            var result = orderService.PlaceOrder(order);
            //Assert
            // ** EXCEPTION SHOULD BE THROWN **
        }

        [Test]
        [ExpectedException(typeof(InvalidDataException), ExpectedMessage = "OrderItems Is Empty")]
        public static void TestOrderSummaryOrderHasEmptyOrderItems()
        {
            //Arrange
            var orderService = new OrderService();
            var order = new Order
            {
                CustomerId = 1,
            };
            //Act
            var result = orderService.PlaceOrder(order);
            //Assert
            // ** EXCEPTION SHOULD BE THROWN **
        }

    }
}
