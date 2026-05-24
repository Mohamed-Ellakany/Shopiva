namespace Shopiva.Abstractions.consts
{
    public static class OrderErrors
    {
        public static readonly Error OrderNotFound =
            new("Order.NotFound", "Order not found.");

        public static readonly Error EmptyCart =
            new("Order.EmptyCart", "Cannot place an order with an empty cart.");

        public static readonly Error InvalidStatusTransition =
            new("Order.InvalidStatusTransition", "This status transition is not allowed.");

        public static readonly Error CannotCancelOrder =
            new("Order.CannotCancel", "Only Pending or Confirmed orders can be cancelled.");
    }
}
