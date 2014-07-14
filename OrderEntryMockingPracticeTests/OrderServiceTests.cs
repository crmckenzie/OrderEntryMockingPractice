


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
            //Act
            var result = orderService.PlaceOrder(order);
            //Assert
            // ** EXCEPTION SHOULD BE THROWN **
        }

        [Test]
        [ExpectedException(typeof(InvalidDataException), ExpectedMessage = "OrderItems Is Empty")]
        public static void TestOrderSummaryOrderHasEmptyOrderItemsThrowsException()
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

        [Test]
        [ExpectedException(typeof(InvalidDataException), ExpectedMessage = "CustomerId Is Null, OrderItems Is Empty")]
        public static void TestOrderSummaryOrderHasEmptyOrderItemsAndEmptyOrderItemsThrowsException()
        {
            //Arrange
            var orderService = new OrderService();
            var order = new Order
            {
                CustomerId = null,
            };
            //Act
            var result = orderService.PlaceOrder(order);
            //Assert
            // ** EXCEPTION SHOULD BE THROWN **
        }

        [Test]
        [ExpectedException(typeof (InvalidDataException), ExpectedMessage = "OrderItems Contains Duplicate Products")]
        public static void TestOrderSummaryOrderItemsContainsDuplicateProductsThrowsException()
        {
            // Arrange
            var orderService = new OrderService();
            var order = new Order
                {
                    CustomerId = 1
                };
            AddDuplicateProductToOrder(order);
            // Act
            var result = orderService.PlaceOrder(order);
            // Assert
            // THAT EXCEPTION WAS THROWN
        }

        private static void AddDuplicateProductToOrder(Order order)
        {
            var apple = new Product
                {
                    Sku = "Apple"
                };
            var banana = new Product
                {
                    Sku = "Banana"
                };
            var duplicateBanana = new Product()
                {
                    Sku = "Banana"
                };
            var appleOrderItem = new OrderItem
                {
                    Product = apple
                };
            var bananaOrderItem = new OrderItem
                {
                    Product = banana
                };
            var dupBananaOrderItem = new OrderItem
                {
                    Product = duplicateBanana
                };
            order.OrderItems.Add(appleOrderItem);
            order.OrderItems.Add(bananaOrderItem);
            order.OrderItems.Add(dupBananaOrderItem);
        }
    }
}
