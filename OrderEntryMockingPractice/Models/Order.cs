using System.Collections.Generic;
using System.Linq;

namespace OrderEntryMockingPractice.Models
{
    public class Order
    {
        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

        public int? CustomerId { get; set; }
        public List<OrderItem> OrderItems { get; set; }

        public decimal GetNetTotal()
        {
            decimal netTotal = OrderItems.Sum(item => item.Product.Price*item.Quantity);
            return netTotal;
        }

        public decimal GetOrderTotalWithTaxes(decimal totalTaxRate)
        {
            var netTotal = GetNetTotal();
            return (netTotal*totalTaxRate) + netTotal;
        }

        public bool OrderItemsIsEmpty()
        {
           // Debug.WriteLine("Seems like OrderItemsIsEmpty would work better as a method on the order object. Perhaps rename to HasOrderItems")

            return OrderItems.Equals(null) || !OrderItems.Any();
        }

        

        public bool ContainsDuplicateProducts()
        {
           // Debug.WriteLine(
              //  "Seems like ContainsDuplicateProducts would work better as a method on the order object. Perhaps rename to ProductsAreUnique");
            var productsInOrderItems = new HashSet<string>();
            foreach (OrderItem item in OrderItems)
            {
                if (productsInOrderItems.Contains(item.Product.Sku)) return true;
                productsInOrderItems.Add(item.Product.Sku);
            }
            return false;
        } 


    }
}
