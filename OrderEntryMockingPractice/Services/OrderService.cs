using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        public OrderSummary PlaceOrder(Order order)
        {
            CheckIfOrderIsValid(order);
            return new OrderSummary();
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
            if (!ProductInStock(order))
            {
                
            }
            if (reasonsForInvalidity.Any())
            {
                var errorMessage = GenerateErrorMessage(reasonsForInvalidity);
                throw new InvalidDataException(errorMessage);
            }
        }

        private bool ProductInStock(Order order)
        {
        }

        private Boolean ContainsDuplicateProducts(Order order)
        {
            var productsInOrderItems = new HashSet<string>();
            foreach (OrderItem item in order.OrderItems)
            {
                if (productsInOrderItems.Contains(item.Product.Sku)) return true;
                else productsInOrderItems.Add(item.Product.Sku);
            }
            return false;
        }

        private string GenerateErrorMessage(List<string> reasonsForInvalidity)
        {
            string errorMessage = reasonsForInvalidity[0];
            if (reasonsForInvalidity.Count > 1)
            {
                for (int i = 1; i < reasonsForInvalidity.Count; i++)
                {
                    errorMessage += ", " + reasonsForInvalidity[i];
                }
            }
            return errorMessage;
        }
    }
}
