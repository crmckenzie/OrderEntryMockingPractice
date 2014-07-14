


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
                orderService.PlaceOrder(null);
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
                orderService.PlaceOrder(order);
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
                orderService.PlaceOrder(order);
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
                orderService.PlaceOrder(order);
            }
            catch (InvalidOrderException exc)
            {
                Assert.That(exc.ExceptionMessages, Has.Member("OrderItems Is Empty"));
                Assert.That(exc.ExceptionMessages, Has.Member("CustomerId Is Null"));
                succeeded = true;
            }
            // Assert
            Assert.That(succeeded, Is.True, 
                "The Expected InvalidOrderException was not caught.");
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
                orderService.PlaceOrder(order);
            }
            catch (InvalidOrderException exc)
            {
                Assert.That(exc.ExceptionMessages, 
                    Has.Member("OrderItems Contains Duplicate Products"));
                succeeded = true;
            }
            // Assert
            Assert.That(succeeded, Is.True, 
                "The Expected InvalidOrderException was not caught.");
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
                Assert.That(exc.ExceptionMessages, 
                    Has.Member("Item Not In Stock In OrderItems"));
                succeeded = true;
            }
            // Assert
            Assert.That(succeeded, Is.True, 
                "The Expected InvalidOrderException was not caught.");
        }

        [Test]
        public static void TestOrderFulfillmentCalledWhenOrderValid()
        {
            // Arrange
            var orderService = CreateOrderService();
            var order = CreateValidOrder();
            // Act
            orderService.PlaceOrder(order);
            // Assert
            orderService.OrderFulfill.Received().Fulfill(order);
        }

        [Test]
        public static void TestOrderFulfillmentNotCalledWhenOrderInvalid()
        {
            // Arrange
            var orderService = CreateOrderService();
            var invalidOrder = new Order();
            try
            {
                // Act
                orderService.PlaceOrder(invalidOrder);
            }
            catch (InvalidOrderException exc)
            {
                // Only Care THat Fulfill not called
            }
            // Assert
            orderService.EmailService.DidNotReceive()
                .SendOrderConfirmationEmail(Arg.Any<int>(), Arg.Any<int>());

        }

        [Test]
        public static void TestEmailConfirmationCalledWhenOrderValid()
        {
            // Arrange
            var orderService = CreateOrderService();
            var order = CreateValidOrder();
            // Act
            orderService.PlaceOrder(order);
            // Assert
            orderService.EmailService.Received()
                .SendOrderConfirmationEmail(Arg.Any<int>(),Arg.Any<int>());
        }

        [Test]
        public static void TestEmailConfirmationNotCalledWhenOrderInvalid()
        {
            // Arrange
            var orderService = CreateOrderService();
            var invalidOrder = new Order();
            try
            {
                // Act
                orderService.PlaceOrder(invalidOrder);
            }
            catch (InvalidOrderException)
            {
                // Only care that Fulfill is not called if exception is thrown
            }
            // Assert
            orderService.OrderFulfill.DidNotReceive().Fulfill(invalidOrder);
        }

        [Test]
        public static void TestNetTotalProperlyCalculated()
        {
            // Arrange
            var orderService = CreateOrderService();
            var orderWithPrices = CreateOrderWithPricesAndQuantities();
            // Act
            var result = orderService.GetNetTotal(orderWithPrices);
            // Assert
            Assert.AreEqual(8, result);
        }

        [Test]
        public static void TestOrderTotalProperlyCalculated()
        {
            // Arrange
            var orderService = CreateOrderService();
            var orderWithPrices = CreateOrderWithPricesAndQuantities();
            // Act
            var result = orderService.GetOrderTotal(orderWithPrices);
            // Assert
            Assert.AreEqual(8.64m, result);
        }

        private static Order CreateOrderWithPricesAndQuantities()
        {
            var order = new Order
            {
                CustomerId = 1
            };
            var apple = new Product
            {
                Sku = "Apple",
                Price = 1
            };
            var banana = new Product
            {
                Sku = "Banana",
                Price = 3
            };
            var appleOrderItem = new OrderItem
            {
                Product = apple,
                Quantity = 5
            };
            var bananaOrderItem = new OrderItem
            {
                Product = banana,
                Quantity = 1
            };
            order.OrderItems.Add(appleOrderItem);
            order.OrderItems.Add(bananaOrderItem);
            return order;
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
            var emailService = CreateMockEmailService();
            var customerRepo = CreateMockCustomerRepository();
            var taxRateService = CreateMockTaxRateService();
            var orderFulfill = CreateMockFulfillService();
            return new OrderService(productRepo,orderFulfill,emailService,
                taxRateService,customerRepo);
        }

        private static IOrderFulfillmentService CreateMockFulfillService()
        {
            var fulfillService = Substitute.For<IOrderFulfillmentService>();
            var confirmation = new OrderConfirmation
                {
                    OrderNumber = "fakeOrderNumber",
                    OrderId = 10,
                    CustomerId = 1,
                    EstimatedDeliveryDate = new DateTime(2014,06,06)
                };
            fulfillService.Fulfill(Arg.Any<Order>()).Returns(confirmation);
            return fulfillService;
        }

        private static IEmailService CreateMockEmailService()
        {
            var emailService = Substitute.For<IEmailService>();
            return emailService;
        }

        private static ITaxRateService CreateMockTaxRateService()
        {
            var taxService = Substitute.For<ITaxRateService>();
            var waTaxEntry = new TaxEntry
                {
                    Description = "wa",
                    Rate = .08m
                };
            var taxEntryList = new List<TaxEntry> {waTaxEntry};
            taxService.GetTaxEntries(Arg.Any<String>(), Arg.Any<String>())
                      .Returns(taxEntryList);
            return taxService;
        }

        private static ICustomerRepository CreateMockCustomerRepository()
        {
            var customerRepository = Substitute.For<ICustomerRepository>();
            var fakeCustomer = CreateFakeCustomer();
            customerRepository.Get(Arg.Any<int>()).Returns(fakeCustomer);
            return customerRepository;
        }

        private static Customer CreateFakeCustomer()
        {
            var fakeCustomer = new Customer
                {
                    CustomerId = 1,
                    CustomerName = "fakeName",
                    EmailAddress = "fakeEmail",
                    AddressLine1 = "fakeAddress",
                    City = "fakeCity",
                    StateOrProvince = "fakeState",
                    PostalCode = "fakePostalCode",
                    Country = "fakeCountry"
                };
            return fakeCustomer;
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
