
using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly RenewalValidator _validator;
        private readonly CustomerRepository _customerRepository;
        private readonly SubscriptionPlanRepository _planRepository;
        private readonly DiscountCalculator _discountCalculator;
        private readonly FeeCalculator _feeCalculator;
        private readonly TaxCalculator _taxCalculator;
        private readonly InvoiceFactory _invoiceFactory;
        private readonly IBillingGateway _billingGateway;

        public SubscriptionRenewalService()
            : this(RenewalDependencies.CreateDefault())
        {
        }

        private SubscriptionRenewalService(
            RenewalDependencies dependencies)
            : this(
                dependencies.Validator,
                dependencies.CustomerRepository,
                dependencies.PlanRepository,
                dependencies.DiscountCalculator,
                dependencies.FeeCalculator,
                dependencies.TaxCalculator,
                dependencies.InvoiceFactory,
                dependencies.BillingGateway)
        {
        }

        public SubscriptionRenewalService(
            RenewalValidator validator,
            CustomerRepository customerRepository,
            SubscriptionPlanRepository planRepository,
            DiscountCalculator discountCalculator,
            FeeCalculator feeCalculator,
            TaxCalculator taxCalculator,
            InvoiceFactory invoiceFactory,
            IBillingGateway billingGateway)
        {
            _validator = validator
                ?? throw new ArgumentNullException(nameof(validator));

            _customerRepository = customerRepository
                ?? throw new ArgumentNullException(nameof(customerRepository));

            _planRepository = planRepository
                ?? throw new ArgumentNullException(nameof(planRepository));

            _discountCalculator = discountCalculator
                ?? throw new ArgumentNullException(nameof(discountCalculator));

            _feeCalculator = feeCalculator
                ?? throw new ArgumentNullException(nameof(feeCalculator));

            _taxCalculator = taxCalculator
                ?? throw new ArgumentNullException(nameof(taxCalculator));

            _invoiceFactory = invoiceFactory
                ?? throw new ArgumentNullException(nameof(invoiceFactory));

            _billingGateway = billingGateway
                ?? throw new ArgumentNullException(nameof(billingGateway));
        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            _validator.Validate(
                customerId,
                planCode,
                seatCount,
                paymentMethod);

            string normalizedPlanCode =
                planCode.Trim().ToUpperInvariant();

            string normalizedPaymentMethod =
                paymentMethod.Trim().ToUpperInvariant();

            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException(
                    "Inactive customers cannot renew subscriptions");
            }

            decimal baseAmount =
                (plan.MonthlyPricePerSeat * seatCount * 12m)
                + plan.SetupFee;

            var discountResult = _discountCalculator.Calculate(
                customer,
                plan,
                seatCount,
                useLoyaltyPoints,
                baseAmount);

            decimal discountAmount = discountResult.Amount;
            string notes = discountResult.Notes;

            decimal subtotalAfterDiscount =
                baseAmount - discountAmount;

            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            var supportResult = _feeCalculator.CalculateSupportFee(
                normalizedPlanCode,
                includePremiumSupport);

            decimal supportFee = supportResult.Amount;
            notes += supportResult.Notes;

            var paymentResult = _feeCalculator.CalculatePaymentFee(
                normalizedPaymentMethod,
                subtotalAfterDiscount,
                supportFee);

            decimal paymentFee = paymentResult.Amount;
            notes += paymentResult.Notes;

            decimal taxBase =
                subtotalAfterDiscount + supportFee + paymentFee;

            decimal taxAmount = _taxCalculator.CalculateTax(
                customer.Country,
                taxBase);

            decimal finalAmount = taxBase + taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                notes += "minimum invoice amount applied; ";
            }

            RenewalInvoice invoice = _invoiceFactory.Create(
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

            _billingGateway.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";

                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                _billingGateway.SendEmail(
                    customer.Email,
                    subject,
                    body);
            }

            return invoice;
        }
    }
}
