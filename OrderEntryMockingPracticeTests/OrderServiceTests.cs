


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
        public static void TestPlaceOrderOrderIsNullThrowsException()
        {
            // Arrange
            var orderService = new OrderService();
            var succeeded = false;
            // Act
            try
            {
                var result = orderService.PlaceOrder(null);
            }
            catch (NullReferenceException exc)
            {
                succeeded = true;
            }
            // Assert
            Assert.That(succeeded, Is.True, "The Expected NullReferenceException was not caught.");
        }

        [Test]
        public static void TestPlaceOrderHasNullCustomerIdThrowsException()
        {
            // Arrange
            var orderService = new OrderService();
            var order = new Order
            {
                CustomerId = null,
            };
            var succeeded = false;
            // Act
            try
            {
                var result = orderService.PlaceOrder(order);
            }
            catch (InvalidOrderException exc)
            {
                Assert.That(exc.ExceptionMessages, Has.Member("CustomerId Is Null"));
                succeeded = true;
            }
            // Assert
            Assert.That(succeeded, Is.True, "The Expected InvalidOrderException was not caught.");
        }

        [Test]
        public static void TestPlaceOrderOrderHasEmptyOrderItemsThrowsException()
        {
            //Arrange
            var orderService = new OrderService();
            var order = new Order
            {
                CustomerId = 1,
            };
            var succeeded = false;
            //Act
            try
            {
                var result = orderService.PlaceOrder(order);
            }
            catch (InvalidOrderException exc)
            {
                Assert.That(exc.ExceptionMessages, Has.Member("OrderItems Is Empty"));
                succeeded = true;
            }
            // Assert
            Assert.That(succeeded, Is.True, "The Expected InvalidOrderException was not caught.");
        }

        [Test]
        public static void TestPlaceOrderOrderHasEmptyOrderItemsAndEmptyOrderItemsThrowsException()
        {
            //Arrange
            var orderService = new OrderService();
            var order = new Order
            {
                CustomerId = null,
            };
            var succeeded = false;
            //Act
            try
            {
                var result = orderService.PlaceOrder(order);
            }
            catch (InvalidOrderException exc)
            {
                Assert.That(exc.ExceptionMessages, Has.Member("OrderItems Is Empty"));
                Assert.That(exc.ExceptionMessages, Has.Member("CustomerId Is Null"));
                succeeded = true;
            }
            // Assert
            Assert.That(succeeded, Is.True, "The Expected InvalidOrderException was not caught.");
        }

        [Test]
        public static void TestPlaceOrderOrderItemsContainsDuplicateProductsThrowsException()
        {
            // Arrange
            var orderService = new OrderService();
            var order = new Order
                {
                    CustomerId = 1
                };
            AddDuplicateProductToOrder(order);
            var succeeded = false;
            // Act
            try
            {
                var result = orderService.PlaceOrder(order);
            }
            catch (InvalidOrderException exc)
            {
                Assert.That(exc.ExceptionMessages, Has.Member("OrderItems Contains Duplicate Products"));
                succeeded = true;
            }
            // Assert
            Assert.That(succeeded, Is.True, "The Expected InvalidOrderException was not caught.");

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
