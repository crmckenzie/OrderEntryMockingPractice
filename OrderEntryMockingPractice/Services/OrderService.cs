using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
    public class OrderService
    {
        public OrderSummary PlaceOrder(Order order)
        {
            // if not valid, throw exception why not valid
                // order valid if, customer exists
                // items are in stock

            // else
            // OrderSummary Returned
            return new OrderSummary();
        }
    }
}
