using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        public IProductRepository ProductRepository { get; set; }
        public IOrderFulfillmentService OrderFulfill { get; set; }
        public IEmailService EmailService { get; set; }

        public OrderService (IProductRepository productRepository, 
            IOrderFulfillmentService orderFulfill, IEmailService emailService)
        {
            ProductRepository = productRepository;
            OrderFulfill = orderFulfill;
            EmailService = emailService;
        }

        public OrderSummary PlaceOrder(Order order)
        {
            if (order == null) throw new NullReferenceException();
            CheckIfOrderIsValid(order);
            OrderConfirmation confirmation = OrderFulfill.Fulfill(order);
            SendConfirmationEmail(order, confirmation);
            return new OrderSummary();
        }

        private void SendConfirmationEmail(Order order, OrderConfirmation confirmation)
        {
            if (order.CustomerId != null)
            {
                EmailService.SendOrderConfirmationEmail((int) order.CustomerId, confirmation.OrderId);
            }
        }

        private void CheckIfOrderIsValid(Order order)
        {
            var reasonsForInvalidity = new List<string>();
            if (order.CustomerId.Equals(null))
            {
                reasonsForInvalidity.Add("CustomerId Is Null");
            }
            if (order.OrderItems.Equals(null) || !order.OrderItems.Any())
            {
                reasonsForInvalidity.Add("OrderItems Is Empty");
            }
            if (ContainsDuplicateProducts(order))
            {
                reasonsForInvalidity.Add("OrderItems Contains Duplicate Products");
            }
            if (!ProductsInStock(order))
            {
                reasonsForInvalidity.Add("Item Not In Stock In OrderItems");
            }
            if (reasonsForInvalidity.Any())
            {
                throw new InvalidOrderException(reasonsForInvalidity);
            }
        }

        private bool ProductsInStock(Order order)
        {
            if (order.OrderItems == null || order.OrderItems.Count == 0) return false;
            return order.OrderItems.All(item => ProductRepository.IsInStock(item.Product.Sku));
        }

        private bool ContainsDuplicateProducts(Order order)
        {
            if (order.OrderItems == null || order.OrderItems.Count == 0) return false;
            var productsInOrderItems = new HashSet<string>();
            foreach (OrderItem item in order.OrderItems)
            {
                if (productsInOrderItems.Contains(item.Product.Sku)) return true;
                else productsInOrderItems.Add(item.Product.Sku);
            }
            return false;
        }
    }

    public class InvalidOrderException : Exception
    {
        public List<string> ExceptionMessages { get; set; }
        public new virtual string Message { get; set;  }

        public InvalidOrderException(List<string> exceptionMessages)
        {
            ExceptionMessages = exceptionMessages;
            GenerateErrorMessage();
        }

        private void GenerateErrorMessage()
        {
            Message = ExceptionMessages[0];
            if (ExceptionMessages.Count > 1)
            {
                for (int i = 1; i < ExceptionMessages.Count; i++)
                {
                    Message += ", " + ExceptionMessages[i];
                }
            }
        }
    }
}
