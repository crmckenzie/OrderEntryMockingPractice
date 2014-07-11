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
            //var reasonsForInvalidity = new List<string>();
            if (order.CustomerId.Equals(null))
            {
                //reasonsForInvalidity.Add("CustomerId does not exist");
                throw new InvalidDataException("CustomerId Is Null");
            }
            if (order.OrderItems.Equals(null) || !order.OrderItems.Any())
            {
                throw new InvalidDataException("OrderItems Is Empty");
            }
        }
    }
}
