namespace Shopiva.Abstractions.enums
{
    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        Failed = 2,
        Refunded = 3,
        PartiallyRefunded = 4
    }

    public enum PaymentMethod
    {
        COD = 0   // Cash On Delivery
    }
}
