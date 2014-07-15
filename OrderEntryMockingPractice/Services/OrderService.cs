using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        public IProductRepository ProductRepository { get; set; }
        public IOrderFulfillmentService FulfillmentService { get; set; }
        public IEmailService EmailService { get; set; }
        public ITaxRateService TaxRateService { get; set; }
        public ICustomerRepository CustomerRepository { get; set; }

        public OrderService (IProductRepository productRepository, 
            IOrderFulfillmentService fulfillmentService, 
            IEmailService emailService, 
            ITaxRateService taxRateService, 
            ICustomerRepository customerRepository)
        {
            ProductRepository = productRepository;
            FulfillmentService = fulfillmentService;
            EmailService = emailService;
            TaxRateService = taxRateService;
            CustomerRepository = customerRepository;
        }

        public OrderSummary PlaceOrder(Order order)
        {
            if (order == null) throw new NullReferenceException();
            CheckIfOrderIsValid(order);
            var confirmation = FulfillmentService.Fulfill(order);
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
            //Debug.WriteLine("Seems like GetNetTotal would work better as a method on the order object.")
            CheckIfOrderIsValid(order);
            decimal netTotal = order.OrderItems.Sum(item => item.Product.Price * item.Quantity);
            return netTotal;
        }

        public decimal GetOrderTotal(Order order)
        {
            CheckIfOrderIsValid(order);
            //Debug.WriteLine("Seems like GetOrderTotal (minus the Validation part) would work better as a method on the order object.")
            var customerInfo = CustomerRepository.Get((int) order.CustomerId);

            var netTotal = GetNetTotal(order);
            var taxRateEntries = TaxRateService.GetTaxEntries(customerInfo.PostalCode, 
                customerInfo.Country);
            // just averaging the taxes
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
            if (order.CustomerId == null)
            {
                reasonsForInvalidity.Add("CustomerId Is Null");
            }
            if (OrderItemsIsEmpty(order))
            {
                reasonsForInvalidity.Add("OrderItems Is Empty");
            }
            if (!OrderItemsIsEmpty(order) && ContainsDuplicateProducts(order))
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

        private static bool OrderItemsIsEmpty(Order order)
        {
            //Debug.WriteLine("Seems like OrderItemsIsEmpty would work better as a method on the order object. Perhaps rename to HasOrderItems")

            return order.OrderItems.Equals(null) || !order.OrderItems.Any();
        }

        private bool ProductsInStock(Order order)
        {
            if (order.OrderItems == null || order.OrderItems.Count == 0) return false;
            return order.OrderItems.All(item => ProductRepository.IsInStock(item.Product.Sku));
        }

        private bool ContainsDuplicateProducts(Order order)
        {
            //Debug.WriteLine(
                "Seems like ContainsDuplicateProducts would work better as a method on the order object. Perhaps rename to ProductsAreUnique");
            var productsInOrderItems = new HashSet<string>();
            foreach (OrderItem item in order.OrderItems)
            {
                if (productsInOrderItems.Contains(item.Product.Sku)) return true;
                else productsInOrderItems.Add(item.Product.Sku);
            }
            return false;
        }  
    }
}
