


using System;
using System.Collections.Generic;
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
        public  void ValidOrder()
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
        public  void PlaceOrderOrderIsNullThrowsException()
        {
            // Arrange
            var orderService = CreateOrderService();
            var succeeded = false;

            // Act
            Assert.Throws<ArgumentNullException>(() => orderService.PlaceOrder(null));
        }

        [Test]
        public  void PlaceOrderHasNullCustomerIdIsInvalid()
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
        public  void PlaceOrderOrderHasEmptyOrderItemsThrowsException()
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
        public void MultipleValidationErrors()
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
        public  void DuplicateProducts()
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
        public  void ProductNotInStock()
        {
            // Arrange
            var productRepo = Substitute.For<IProductRepository>();
            productRepo.IsInStock("Steak").Returns(false);
            productRepo.IsInStock("Apple").Returns(true);
            productRepo.IsInStock("Banana").Returns(true);
            var orderService = CreateOrderService();
            var order = new Order
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
                orderService.PlaceOrder(order);
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
        public  void OrderFulfillmentCalledWhenOrderValid()
        {
            // Arrange
            var orderService = CreateOrderService();
            var order = CreateValidOrder();
            // Act
            orderService.PlaceOrder(order);
            // Assert
            orderService.FulfillmentService.Received().Fulfill(order);
        }

        [Test]
        public  void OrderFulfillmentNotCalledWhenOrderInvalid()
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
                // Only Care THat Fulfill not called
            }
            // Assert
            orderService.FulfillmentService.DidNotReceive().Fulfill(invalidOrder);
        }

        [Test]
        public  void EmailConfirmationCalledWhenOrderValid()
        {
            // Arrange
            var orderService = CreateOrderService();
            var order = CreateValidOrder();
            // Act
            orderService.PlaceOrder(order);

            // Assert
            Assert.Fail("Arg.Any<int>() is not appropriate here. You'll need to mimic the db behavior of assigning an id and assert that the correct ids are passed.");
            orderService.EmailService.Received()
                .SendOrderConfirmationEmail(554, 214234);
        }

        [Test]
        public  void EmailConfirmationNotCalledWhenOrderInvalid()
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
            Assert.Fail("Arg.Any<int>() is not appropriate here. You'll need to mimic the db behavior of assigning an id and assert that the correct ids are passed.");
            orderService.EmailService.DidNotReceive()
                .SendOrderConfirmationEmail(Arg.Any<int>(), Arg.Any<int>());
        }

        [Test]
        public  void NetTotalProperlyCalculated()
        {
            // Arrange
            var orderService = CreateOrderService();
            var orderWithPrices = CreateOrderWithPricesAndQuantities();
            // Act
            var result = orderService.GetNetTotal(orderWithPrices);

            // Assert
            Assert.Fail("8 is a magic number. Give it context to make it meaningful.");
            Assert.AreEqual(8, result);
        }

        [Test]
        public  void OrderTotalProperlyCalculated()
        {
            // Arrange
            var orderService = CreateOrderService();
            var orderWithPrices = CreateOrderWithPricesAndQuantities();
            // Act
            var result = orderService.GetOrderTotal(orderWithPrices);
            // Assert
            Assert.Fail("magic number. Give it context to make it meaningful.");
            Assert.AreEqual(8.64m, result);
        }

        [Test]
        public  void ForProperOrderSummary()
        {
            // Arrange
            var orderService = CreateOrderService();
            var orderWithPrices = CreateOrderWithPricesAndQuantities();
            // Act
            var result = orderService.PlaceOrder(orderWithPrices);
            // Assert
            Assert.Fail("magic number. Give it context to make it meaningful.");
            Assert.That(result.OrderId.Equals(10));
            Assert.That(result.OrderNumber.Equals("fakeOrderNumber"));
            Assert.That(result.CustomerId.Equals(1));
            Assert.That(result.OrderItems.Count > 0);
            Assert.That(result.Taxes, Is.Not.Null);
            Assert.That(result.NetTotal.Equals(8m));
            Assert.That(result.Total.Equals(8.64m));
            Assert.That(result.EstimatedDeliveryDate, Is.Not.Null);
        }

        private Order CreateOrderWithPricesAndQuantities()
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

        private Order CreateValidOrder()
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

        private void AddDuplicateProductToOrder(Order order)
        {
            var apple = new Product
                {
                    Sku = "Apple"
                };
            var banana = new Product
                {
                    Sku = "Banana"
                };
            var duplicateBanana = new Product
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

        private OrderService CreateOrderService()
        {
            var productRepo = CreateMockProductRepository();
            var emailService = CreateMockEmailService();
            var customerRepo = CreateMockCustomerRepository();
            var taxRateService = CreateMockTaxRateService();
            var orderFulfill = CreateMockFulfillService();
            return new OrderService(productRepo,orderFulfill,emailService,
                taxRateService,customerRepo);
        }

        private IOrderFulfillmentService CreateMockFulfillService()
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

        private IEmailService CreateMockEmailService()
        {
            var emailService = Substitute.For<IEmailService>();
            return emailService;
        }

        private ITaxRateService CreateMockTaxRateService()
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

        private ICustomerRepository CreateMockCustomerRepository()
        {
            var customerRepository = Substitute.For<ICustomerRepository>();
            var fakeCustomer = CreateFakeCustomer();
            customerRepository.Get(Arg.Any<int>()).Returns(fakeCustomer);
            return customerRepository;
        }

        private Customer CreateFakeCustomer()
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

        private IProductRepository CreateMockProductRepository()
        {
            var productRepo = Substitute.For<IProductRepository>();
            productRepo.IsInStock("Steak").Returns(false);
            productRepo.IsInStock("Apple").Returns(true);
            productRepo.IsInStock("Banana").Returns(true);
            return productRepo;
        }
    }
}
