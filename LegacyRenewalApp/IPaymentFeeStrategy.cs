
namespace LegacyRenewalApp
{
    public interface IPaymentFeeStrategy
    {
        string PaymentMethod { get; }

        decimal Calculate(decimal amount);

        string Notes { get; }
    }
}
