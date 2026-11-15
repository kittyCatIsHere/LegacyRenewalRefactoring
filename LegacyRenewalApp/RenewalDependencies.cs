
namespace LegacyRenewalApp
{
    internal sealed class RenewalDependencies
    {
        public RenewalValidator Validator { get; init; }

        public CustomerRepository CustomerRepository { get; init; }

        public SubscriptionPlanRepository PlanRepository { get; init; }

        public DiscountCalculator DiscountCalculator { get; init; }

        public FeeCalculator FeeCalculator { get; init; }

        public TaxCalculator TaxCalculator { get; init; }

        public InvoiceFactory InvoiceFactory { get; init; }

        public IBillingGateway BillingGateway { get; init; }

        public static RenewalDependencies CreateDefault()
        {
            IPaymentFeeStrategy[] paymentStrategies =
            {
                new CardPaymentStrategy(),
                new BankTransferPaymentStrategy(),
                new PayPalPaymentStrategy(),
                new InvoicePaymentStrategy()
            };

            return new RenewalDependencies
            {
                Validator = new RenewalValidator(),

                CustomerRepository = new CustomerRepository(),

                PlanRepository = new SubscriptionPlanRepository(),

                DiscountCalculator = new DiscountCalculator(),

                FeeCalculator = new FeeCalculator(
                    paymentStrategies),

                TaxCalculator = new TaxCalculator(),

                InvoiceFactory = new InvoiceFactory(),

                BillingGateway =
                    new LegacyBillingGatewayAdapter()
            };
        }
    }
}
