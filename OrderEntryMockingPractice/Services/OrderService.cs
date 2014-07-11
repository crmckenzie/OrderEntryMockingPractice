using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        public OrderSummary PlaceOrder(Order order)
        {
            CheckIfOrderIsValid(order);
            // if not valid, throw exception why not valid
                // order valid if, customer exists
                // items are in stock
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
            if (reasonsForInvalidity.Any())
            {
                string errorMessage = reasonsForInvalidity[0];
                if (reasonsForInvalidity.Count > 1)
                {
                    for (int i = 1; i < reasonsForInvalidity.Count; i++)
                    {
                        errorMessage += ", " + reasonsForInvalidity[i];
                    }
                }
                throw new InvalidDataException(errorMessage);
            }
        }
    }
}
