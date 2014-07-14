﻿using System;
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
        public OrderSummary PlaceOrder(Order order)
        {
            if (order == null) throw new NullReferenceException();
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
            if (!productsInStock(order))
            {
                reasonsForInvalidity.Add("Item Not In Stock In OrderItems");
            }
            if (reasonsForInvalidity.Any())
            {
                throw new InvalidOrderException(reasonsForInvalidity);
            }
        }

        private bool productsInStock(Order order)
        {
            if (order.OrderItems == null || order.OrderItems.Count == 0) return false;
            var productRepo = Substitute.For<IProductRepository>();
            productRepo.IsInStock("Steak").Returns(false);
            productRepo.IsInStock("Apple").Returns(true);
            productRepo.IsInStock("Banana").Returns(true);
            return order.OrderItems.All(item => productRepo.IsInStock(item.Product.Sku));
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
