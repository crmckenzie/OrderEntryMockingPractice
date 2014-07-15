using System;
using System.Collections.Generic;
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
            if (order == null) throw new ArgumentNullException();
            CheckIfOrderIsValid(order);
            var confirmation = FulfillmentService.Fulfill(order);
            SendConfirmationEmail(order, confirmation);
            return CreateOrderSummary(order, confirmation);
        }

        private decimal CompoundTaxes(OrderConfirmation confirmation)
        {
            var customerInfo = CustomerRepository.Get(confirmation.CustomerId);
            var taxRateEntries = TaxRateService.GetTaxEntries(customerInfo.PostalCode,
                                                              customerInfo.Country);
            return taxRateEntries.Sum(taxRate => taxRate.Rate);
        }


        private OrderSummary CreateOrderSummary(Order order, OrderConfirmation confirmation)
        {
            var customer = CustomerRepository.Get(confirmation.CustomerId);
            var orderSummary = new OrderSummary
                {
                    OrderId = confirmation.OrderId,
                    OrderNumber = confirmation.OrderNumber,
                    CustomerId = confirmation.CustomerId,
                    OrderItems = order.OrderItems,
                    NetTotal = order.GetNetTotal(),
                    Taxes = TaxRateService.GetTaxEntries(customer.PostalCode, customer.Country),
                    Total = order.GetOrderTotalWithTaxes(CompoundTaxes(confirmation)),
                    EstimatedDeliveryDate = confirmation.EstimatedDeliveryDate
                };
            return orderSummary;
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
            if (order.OrderItemsIsEmpty())
            {
                reasonsForInvalidity.Add("OrderItems Is Empty");
            }
            if (!order.OrderItemsIsEmpty() && order.ContainsDuplicateProducts())
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

         
    }
}
