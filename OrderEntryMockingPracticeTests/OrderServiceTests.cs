


using System;
using System.Collections.Generic;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;

namespace OrderEntryMockingPracticeTests
{
    [TestFixture]
    public class OrderServiceTests
    {

        [Test]
        public static void TestValidOrder()
        {
            // Arrange
            var orderService = CreateOrderService();
            var order = CreateValidOrder();
            // Act
            var result = orderService.PlaceOrder(order);
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public static void TestPlaceOrderOrderIsNullThrowsException()
        {
            // Arrange
            var orderService = CreateOrderService();
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
            var orderService = CreateOrderService();
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
            var orderService = CreateOrderService();
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
            var orderService = CreateOrderService();
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
            var orderService = CreateOrderService();
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

        [Test]
        public static void TestPlaceOrderItemNotInStockThrowsException()
        {
            // Arrange
            var productRepo = Substitute.For<IProductRepository>();
            productRepo.IsInStock("Steak").Returns(false);
            productRepo.IsInStock("Apple").Returns(true);
            productRepo.IsInStock("Banana").Returns(true);
            var orderService = CreateOrderService();
            var order = new Order()
                {
                    CustomerId = 1
                };
            var steakNotInStock = new Product
            {
                Sku = "Steak"
            };
            var steakOrderItemNotInStock = new OrderItem
            {
                Product = steakNotInStock
            };
            order.OrderItems.Add(steakOrderItemNotInStock);
            var succeeded = false;
            // Act
            try
            {
                var result = orderService.PlaceOrder(order);
            }
            catch (InvalidOrderException exc)
            {
                Assert.That(exc.ExceptionMessages, Has.Member("Item Not In Stock In OrderItems"));
                succeeded = true;
            }
            // Assert
            Assert.That(succeeded, Is.True, "The Expected InvalidOrderException was not caught.");
        }

        private static Order CreateValidOrder()
        {
            var order = new Order
                {
                    CustomerId = 1
                };
            var apple = new Product
                {
                    Sku = "Apple"
                };
            var banana = new Product
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
            order.OrderItems.Add(appleOrderItem);
            order.OrderItems.Add(bananaOrderItem);
            return order;
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

        private static OrderService CreateOrderService()
        {
            var productRepo = CreateMockProductRepository();
            var customerRepo = CreateMockCustomerRepository();
            var emailService = CreateMockEmailService();
            var raxRateService = CreateMockTaxRateService();
            var orderFullfill = CreateMockFullfillService();
            return new OrderService(productRepo);
        }

        private static IOrderFulfillmentService CreateMockFullfillService()
        {
            throw new NotImplementedException();
        }

        private static ITaxRateService CreateMockTaxRateService()
        {
            throw new NotImplementedException();
        }

        private static IEmailService CreateMockEmailService()
        {
            throw new NotImplementedException();
        }

        private static ICustomerRepository CreateMockCustomerRepository()
        {
            throw new NotImplementedException();
        }

        private static IProductRepository CreateMockProductRepository()
        {
            var productRepo = Substitute.For<IProductRepository>();
            productRepo.IsInStock("Steak").Returns(false);
            productRepo.IsInStock("Apple").Returns(true);
            productRepo.IsInStock("Banana").Returns(true);
            return productRepo;
        }
    }
}
