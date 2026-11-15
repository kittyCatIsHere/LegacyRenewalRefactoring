
using System;
using System.Globalization;
using LegacyRenewalApp;
using Xunit;

namespace LegacyRenewalApp.Tests
{
    public class RenewalValidatorTests
    {
        private readonly RenewalValidator _validator = new();

        [Fact]
        public void ValidInput_DoesNotThrow()
        {
            _validator.Validate(1, "PRO", 10, "CARD");
        }

        [Theory]
        [InlineData(0, "PRO", 10, "CARD")]
        [InlineData(1, "", 10, "CARD")]
        [InlineData(1, "PRO", 0, "CARD")]
        [InlineData(1, "PRO", 10, "")]
        public void InvalidInput_ThrowsArgumentException(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod)
        {
            Assert.Throws<ArgumentException>(() =>
                _validator.Validate(
                    customerId,
                    planCode,
                    seatCount,
                    paymentMethod));
        }
    }

    public class DiscountCalculatorTests
    {
        private readonly DiscountCalculator _calculator = new();

        private static Customer CreateCustomer(
            string segment = "Standard",
            int years = 0,
            int points = 0)
        {
            return new Customer
            {
                Id = 1,
                FullName = "Test Customer",
                Segment = segment,
                YearsWithCompany = years,
                LoyaltyPoints = points,
                IsActive = true
            };
        }

        private static SubscriptionPlan CreatePlan(
            bool educationEligible = true)
        {
            return new SubscriptionPlan
            {
                Code = "PRO",
                MonthlyPricePerSeat = 100m,
                SetupFee = 0m,
                IsEducationEligible = educationEligible
            };
        }

        [Theory]
        [InlineData("Silver", "50", "silver discount; ")]
        [InlineData("Gold", "100", "gold discount; ")]
        [InlineData("Platinum", "150", "platinum discount; ")]
        [InlineData("Education", "200", "education discount; ")]
        [InlineData("Standard", "0", "")]
        public void SegmentDiscount_IsCorrect(
            string segment,
            string expectedAmount,
            string expectedNotes)
        {
            var customer = CreateCustomer(segment);

            var result = _calculator.Calculate(
                customer,
                CreatePlan(),
                1,
                false,
                1000m);

            decimal expected = decimal.Parse(
                expectedAmount,
                CultureInfo.InvariantCulture);

            Assert.Equal(expected, result.Amount);
            Assert.Equal(expectedNotes, result.Notes);
        }

        [Fact]
        public void EducationDiscount_RequiresEligiblePlan()
        {
            var customer = CreateCustomer("Education");

            var result = _calculator.Calculate(
                customer,
                CreatePlan(false),
                1,
                false,
                1000m);

            Assert.Equal(0m, result.Amount);
            Assert.Equal("", result.Notes);
        }

        [Theory]
        [InlineData(1, "0", "")]
        [InlineData(2, "30", "basic loyalty discount; ")]
        [InlineData(5, "70", "long-term loyalty discount; ")]
        public void LoyaltyDiscount_IsCorrect(
            int years,
            string expectedAmount,
            string expectedNotes)
        {
            var result = _calculator.Calculate(
                CreateCustomer(years: years),
                CreatePlan(),
                1,
                false,
                1000m);

            Assert.Equal(
                decimal.Parse(
                    expectedAmount,
                    CultureInfo.InvariantCulture),
                result.Amount);

            Assert.Equal(expectedNotes, result.Notes);
        }

        [Theory]
        [InlineData(9, "0", "")]
        [InlineData(10, "40", "small team discount; ")]
        [InlineData(20, "80", "medium team discount; ")]
        [InlineData(50, "120", "large team discount; ")]
        public void TeamDiscount_IsCorrect(
            int seats,
            string expectedAmount,
            string expectedNotes)
        {
            var result = _calculator.Calculate(
                CreateCustomer(),
                CreatePlan(),
                seats,
                false,
                1000m);

            Assert.Equal(
                decimal.Parse(
                    expectedAmount,
                    CultureInfo.InvariantCulture),
                result.Amount);

            Assert.Equal(expectedNotes, result.Notes);
        }

        [Fact]
        public void LoyaltyPoints_AreCappedAt200()
        {
            var customer = CreateCustomer(points: 260);

            var result = _calculator.Calculate(
                customer,
                CreatePlan(),
                1,
                true,
                1000m);

            Assert.Equal(200m, result.Amount);
            Assert.Equal(
                "loyalty points used: 200; ",
                result.Notes);
        }

        [Fact]
        public void DisabledLoyaltyPoints_AreNotUsed()
        {
            var customer = CreateCustomer(points: 260);

            var result = _calculator.Calculate(
                customer,
                CreatePlan(),
                1,
                false,
                1000m);

            Assert.Equal(0m, result.Amount);
            Assert.Equal("", result.Notes);
        }
    }

    public class FeeCalculatorTests
    {
        private readonly FeeCalculator _calculator = new();

        [Theory]
        [InlineData("START", 250)]
        [InlineData("PRO", 400)]
        [InlineData("ENTERPRISE", 700)]
        public void PremiumSupportFee_IsCorrect(
            string planCode,
            int expectedFee)
        {
            var result = _calculator.CalculateSupportFee(
                planCode,
                true);

            Assert.Equal((decimal)expectedFee, result.Amount);
            Assert.Equal(
                "premium support included; ",
                result.Notes);
        }

        [Fact]
        public void DisabledPremiumSupport_HasNoFee()
        {
            var result = _calculator.CalculateSupportFee(
                "PRO",
                false);

            Assert.Equal(0m, result.Amount);
            Assert.Equal("", result.Notes);
        }

        [Theory]
        [InlineData("CARD", "20", "card payment fee; ")]
        [InlineData("BANK_TRANSFER", "10", "bank transfer fee; ")]
        [InlineData("PAYPAL", "35", "paypal fee; ")]
        [InlineData("INVOICE", "0", "invoice payment; ")]
        public void PaymentMethodFee_IsCorrect(
            string method,
            string expectedFee,
            string expectedNotes)
        {
            var result = _calculator.CalculatePaymentFee(
                method,
                900m,
                100m);

            Assert.Equal(
                decimal.Parse(
                    expectedFee,
                    CultureInfo.InvariantCulture),
                result.Amount);

            Assert.Equal(expectedNotes, result.Notes);
        }

        [Fact]
        public void UnsupportedPaymentMethod_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                _calculator.CalculatePaymentFee(
                    "CRYPTO",
                    1000m,
                    0m));

            Assert.Equal(
                "Unsupported payment method",
                exception.Message);
        }
    }

    public class TaxCalculatorTests
    {
        private readonly TaxCalculator _calculator = new();

        [Theory]
        [InlineData("Poland", "230")]
        [InlineData("Germany", "190")]
        [InlineData("Czech Republic", "210")]
        [InlineData("Norway", "250")]
        [InlineData("France", "200")]
        public void CountryTax_IsCorrect(
            string country,
            string expectedTax)
        {
            decimal result = _calculator.CalculateTax(
                country,
                1000m);

            Assert.Equal(
                decimal.Parse(
                    expectedTax,
                    CultureInfo.InvariantCulture),
                result);
        }
    }

    public class InvoiceFactoryTests
    {
        [Fact]
        public void Create_RoundsAmountsAndTrimsNotes()
        {
            var factory = new InvoiceFactory();

            var customer = new Customer
            {
                Id = 1,
                FullName = "Test Customer",
                IsActive = true
            };

            var invoice = factory.Create(
                1,
                customer,
                "PRO",
                "CARD",
                10,
                1000.005m,
                100.005m,
                400m,
                20m,
                250m,
                1570.005m,
                "test note; ");

            Assert.Equal("Test Customer", invoice.CustomerName);
            Assert.Equal("PRO", invoice.PlanCode);
            Assert.Equal("CARD", invoice.PaymentMethod);
            Assert.Equal(10, invoice.SeatCount);

            Assert.Equal(1000.01m, invoice.BaseAmount);
            Assert.Equal(100.01m, invoice.DiscountAmount);
            Assert.Equal(1570.01m, invoice.FinalAmount);

            Assert.Equal("test note;", invoice.Notes);
        }
    }
}
