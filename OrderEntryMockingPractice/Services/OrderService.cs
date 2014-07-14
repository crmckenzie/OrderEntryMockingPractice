using System;
using System.Collections.Generic;
using System.Linq;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        public IProductRepository ProductRepository { get; set; }
        public IOrderFulfillmentService OrderFulfill { get; set; }
        public IEmailService EmailService { get; set; }
        public ITaxRateService TaxRateService { get; set; }
        public ICustomerRepository CustomerRepository { get; set; }

        public OrderService (IProductRepository productRepository, 
            IOrderFulfillmentService orderFulfill, 
            IEmailService emailService, 
            ITaxRateService taxRateService, 
            ICustomerRepository customerRepository)
        {
            ProductRepository = productRepository;
            OrderFulfill = orderFulfill;
            EmailService = emailService;
            TaxRateService = taxRateService;
            CustomerRepository = customerRepository;
        }

        public OrderSummary PlaceOrder(Order order)
        {
            if (order == null) throw new NullReferenceException();
            CheckIfOrderIsValid(order);
            var confirmation = OrderFulfill.Fulfill(order);
            SendConfirmationEmail(order, confirmation);
            return CreateOrderSummary(order, confirmation);

        }

        private OrderSummary CreateOrderSummary(Order order, OrderConfirmation confirmation)
        {
            var customer = CustomerRepository.Get((int) order.CustomerId);
            var orderSummary = new OrderSummary
                {
                    OrderId = confirmation.OrderId,
                    OrderNumber = confirmation.OrderNumber,
                    CustomerId = (int) order.CustomerId,
                    OrderItems = order.OrderItems,
                    NetTotal = GetNetTotal(order),
                    Taxes = TaxRateService.GetTaxEntries(customer.PostalCode, customer.Country),
                    Total = GetOrderTotal(order),
                    EstimatedDeliveryDate = confirmation.EstimatedDeliveryDate
                };
            return orderSummary;
        }

        public decimal GetNetTotal(Order order)
        {
            CheckIfOrderIsValid(order);
            decimal netTotal = order.OrderItems.Sum(item => item.Product.Price * item.Quantity);
            return netTotal;
        }

        public decimal GetOrderTotal(Order order)
        {
            CheckIfOrderIsValid(order);
            var customerInfo = CustomerRepository.Get((int) order.CustomerId);

            var netTotal = GetNetTotal(order);
            var taxRateEntries = TaxRateService.GetTaxEntries(customerInfo.PostalCode, 
                customerInfo.Country);
            // just averaging the taxes since this is slightly unclear
            var numberOfTaxRates = 0m;
            var totalOfTaxRates = 0m;
            foreach (var taxRate in taxRateEntries)
            {
                totalOfTaxRates += taxRate.Rate;
                numberOfTaxRates += 1.0m;
            }
            var totalTax = totalOfTaxRates/numberOfTaxRates;
            return (netTotal*totalTax) + netTotal;
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
