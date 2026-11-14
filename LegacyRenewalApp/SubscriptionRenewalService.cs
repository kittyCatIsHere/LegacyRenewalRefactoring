
using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            // Step 1: Validate input
            var validator = new RenewalValidator();

            validator.Validate(
                customerId,
                planCode,
                seatCount,
                paymentMethod);

            // Step 2: Normalize input
            string normalizedPlanCode =
                planCode.Trim().ToUpperInvariant();

            string normalizedPaymentMethod =
                paymentMethod.Trim().ToUpperInvariant();

            // Step 3: Load customer and plan
            var customerRepository = new CustomerRepository();
            var planRepository = new SubscriptionPlanRepository();

            var customer = customerRepository.GetById(customerId);
            var plan = planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException(
                    "Inactive customers cannot renew subscriptions");
            }

            // Step 4: Calculate base amount
            decimal baseAmount =
                (plan.MonthlyPricePerSeat * seatCount * 12m)
                + plan.SetupFee;

            // Step 5: Calculate discounts
            var discountCalculator = new DiscountCalculator();

            var discountResult = discountCalculator.Calculate(
                customer,
                plan,
                seatCount,
                useLoyaltyPoints,
                baseAmount);

            decimal discountAmount = discountResult.Amount;
            string notes = discountResult.Notes;

            // Step 6: Apply minimum discounted subtotal
            decimal subtotalAfterDiscount =
                baseAmount - discountAmount;

            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            // Step 7: Calculate support fee
            var feeCalculator = new FeeCalculator();

            var supportResult = feeCalculator.CalculateSupportFee(
                normalizedPlanCode,
                includePremiumSupport);

            decimal supportFee = supportResult.Amount;
            notes += supportResult.Notes;

            // Step 8: Calculate payment fee
            var paymentResult = feeCalculator.CalculatePaymentFee(
                normalizedPaymentMethod,
                subtotalAfterDiscount,
                supportFee);

            decimal paymentFee = paymentResult.Amount;
            notes += paymentResult.Notes;

            // Step 9: Calculate tax
            var taxCalculator = new TaxCalculator();

            decimal taxBase =
                subtotalAfterDiscount + supportFee + paymentFee;

            decimal taxAmount = taxCalculator.CalculateTax(
                customer.Country,
                taxBase);

            decimal finalAmount = taxBase + taxAmount;

            // Step 10: Apply minimum invoice amount
            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                notes += "minimum invoice amount applied; ";
            }

            // Step 11: Create invoice using InvoiceFactory
            var invoiceFactory = new InvoiceFactory();

            RenewalInvoice invoice = invoiceFactory.Create(
                customerId,
                customer,
                normalizedPlanCode,
                normalizedPaymentMethod,
                seatCount,
                baseAmount,
                discountAmount,
                supportFee,
                paymentFee,
                taxAmount,
                finalAmount,
                notes);

            // Step 12: Save invoice
            LegacyBillingGateway.SaveInvoice(invoice);

            // Step 13: Send email
            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";

                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                LegacyBillingGateway.SendEmail(
                    customer.Email,
                    subject,
                    body);
            }

            return invoice;
        }
    }
}
