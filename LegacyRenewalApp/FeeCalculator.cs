
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegacyRenewalApp
{
    public class FeeCalculator
    {
        private readonly IReadOnlyDictionary<string, IPaymentFeeStrategy>
            _paymentStrategies;

        public FeeCalculator()
            : this(new IPaymentFeeStrategy[]
            {
                new CardPaymentStrategy(),
                new BankTransferPaymentStrategy(),
                new PayPalPaymentStrategy(),
                new InvoicePaymentStrategy()
            })
        {
        }

        public FeeCalculator(
            IEnumerable<IPaymentFeeStrategy> paymentStrategies)
        {
            if (paymentStrategies == null)
            {
                throw new ArgumentNullException(
                    nameof(paymentStrategies));
            }

            _paymentStrategies = paymentStrategies.ToDictionary(
                strategy => strategy.PaymentMethod,
                strategy => strategy,
                StringComparer.Ordinal);
        }

        public (decimal Amount, string Notes) CalculateSupportFee(
            string planCode,
            bool includePremiumSupport)
        {
            decimal supportFee = 0m;
            string notes = string.Empty;

            if (includePremiumSupport)
            {
                if (planCode == "START")
                {
                    supportFee = 250m;
                }
                else if (planCode == "PRO")
                {
                    supportFee = 400m;
                }
                else if (planCode == "ENTERPRISE")
                {
                    supportFee = 700m;
                }

                notes += "premium support included; ";
            }

            return (supportFee, notes);
        }

        public (decimal Amount, string Notes) CalculatePaymentFee(
            string paymentMethod,
            decimal subtotalAfterDiscount,
            decimal supportFee)
        {
            if (!_paymentStrategies.TryGetValue(
                    paymentMethod,
                    out var strategy))
            {
                throw new ArgumentException(
                    "Unsupported payment method");
            }

            decimal amount =
                subtotalAfterDiscount + supportFee;

            decimal paymentFee = strategy.Calculate(amount);

            return (paymentFee, strategy.Notes);
        }
    }
}
