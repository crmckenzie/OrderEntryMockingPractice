using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.Internal;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;
using Rhino.Mocks;

namespace OrderEntryMockingPracticeTests
{

    [TestFixture]
    public class OrderServiceTests
    {
        
        public Order MakeOrders()
        {
            var order = new Order
            {
                CustomerId = 1,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Product = new Product
                        {
                            Description = "Product",
                            Name = "Name of Product.",
                            Price = 10.0m,
                            ProductId = 5,
                            Sku = "ABCDE"
                        },
                        Quantity = 10
                    },
                    new OrderItem
                    {
                        Product = new Product
                        {
                            Description = "Product",
                            Name = "Name of Product.",
                            Price = 10.0m,
                            ProductId = 5,
                            Sku = "BCDEF"
                        },
                        Quantity = 10
                    }

                }
            
            };
            return order;
        }

        [Test]
        public void OrderItemsAreUniqueByProductSku()
        {

            var order = MakeOrders();

            /// populate it

            var rhinoVersion = MockRepository.GenerateMock<IProductRepository>();

            rhinoVersion.Stub(a => a.IsInStock(Arg<string>.Is.Anything)).Return(true);
            
            var orderService = new OrderService(rhinoVersion);
            var result = orderService.PlaceOrder(order);
            // valid
            Assert.IsNotNull(result);
        }

        [Test]
        public void OrderItemsNotUniqueByProductSkuReturnsNull()
        {
            var order = MakeOrders();
            order.OrderItems[1].Product.Sku = order.OrderItems[0].Product.Sku;

            /// populate it
            var MIProductRepository = MockRepository.GenerateMock<IProductRepository>();

            MIProductRepository.Stub(a => a.IsInStock(Arg<string>.Is.Anything)).Return(false);

            var orderService = new OrderService(MIProductRepository);
            var result = orderService.PlaceOrder(order);
            // invalid
            Assert.IsNull(result);
            Assert.That(exceptionList, Is.EqualTo(""));
        }

        [Test]
        public void AllOrderItemsMustBeInStock()
        {

            var order = MakeOrders();

            var MIProductRepository = MockRepository.GenerateMock<IProductRepository>();
            MIProductRepository.Stub(a => a.IsInStock("ABCDE")).Return(true);
            MIProductRepository.Stub(a => a.IsInStock("BCDEF")).Return(true);

            var orderService = new OrderService(MIProductRepository);


            var result = orderService.PlaceOrder(order);
            // valid
            Assert.IsNotNull(result);
        }

        [Test]
        public void OrderItemsNotInStockReturnNull()
        {

            var order = MakeOrders();
            

            /// populate it

            var MIProductRepository = MockRepository.GenerateMock<IProductRepository>();
            MIProductRepository.Stub(a => a.IsInStock("ABCDE")).Return(false);

            var orderService = new OrderService(MIProductRepository);


            var result = orderService.PlaceOrder(order);
            // valid
            Assert.IsNull(result);
        }
    }    
}
