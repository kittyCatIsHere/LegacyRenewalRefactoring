
using System;

namespace LegacyRenewalApp
{
    public class FeeCalculator
    {
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
            decimal paymentFee = 0m;
            string notes = string.Empty;

            if (paymentMethod == "CARD")
            {
                paymentFee =
                    (subtotalAfterDiscount + supportFee) * 0.02m;

                notes += "card payment fee; ";
            }
            else if (paymentMethod == "BANK_TRANSFER")
            {
                paymentFee =
                    (subtotalAfterDiscount + supportFee) * 0.01m;

                notes += "bank transfer fee; ";
            }
            else if (paymentMethod == "PAYPAL")
            {
                paymentFee =
                    (subtotalAfterDiscount + supportFee) * 0.035m;

                notes += "paypal fee; ";
            }
            else if (paymentMethod == "INVOICE")
            {
                paymentFee = 0m;
                notes += "invoice payment; ";
            }
            else
            {
                throw new ArgumentException(
                    "Unsupported payment method");
            }

            return (paymentFee, notes);
        }
    }
}
