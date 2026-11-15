
namespace LegacyRenewalApp
{
    public class CardPaymentStrategy : IPaymentFeeStrategy
    {
        public string PaymentMethod => "CARD";

        public decimal Calculate(decimal amount)
        {
            return amount * 0.02m;
        }

        public string Notes => "card payment fee; ";
    }

    public class BankTransferPaymentStrategy : IPaymentFeeStrategy
    {
        public string PaymentMethod => "BANK_TRANSFER";

        public decimal Calculate(decimal amount)
        {
            return amount * 0.01m;
        }

        public string Notes => "bank transfer fee; ";
    }

    public class PayPalPaymentStrategy : IPaymentFeeStrategy
    {
        public string PaymentMethod => "PAYPAL";

        public decimal Calculate(decimal amount)
        {
            return amount * 0.035m;
        }

        public string Notes => "paypal fee; ";
    }

    public class InvoicePaymentStrategy : IPaymentFeeStrategy
    {
        public string PaymentMethod => "INVOICE";

        public decimal Calculate(decimal amount)
        {
            return 0m;
        }

        public string Notes => "invoice payment; ";
    }
}
