using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        private readonly IProductRepository _productRepository;

        public OrderService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public OrderSummary PlaceOrder(Order order)
        {
            var y = order.OrderItems.Select(x => x.Product.Sku);

            if (StringsAreUnique(y))
            {
                foreach (var sku in y)
                    if (!_productRepository.IsInStock(sku))
                        return null;

                return new OrderSummary();
            }
            return null;
        }

        private bool StringsAreUnique(IEnumerable<string> strings)
        {
            return strings.Distinct().Count() == strings.Count();
        }
    }
}
