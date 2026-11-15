
# LegacyRenewalApp — Refactoring Project

## Project Overview

This project demonstrates the refactoring of an existing
C# subscription renewal application using SOLID principles,
DRY, high cohesion, low coupling, and dependency injection.

The objective is to improve code quality without changing
the original business logic or observable application behavior.

The project targets .NET 8.

## Project Structure

### LegacyRenewalApp

Contains the subscription renewal business logic and
the refactored classes.

### LegacyRenewalAppConsumer

The original client application.

This project has not been modified.

### LegacyRenewalApp.Tests

Contains automated unit tests written using xUnit.

### GitHub Actions

The workflow in `.github/workflows/build.yml`
automatically builds the solution, executes unit tests,
and verifies the consumer application's output.

## Refactoring Changes

### 1. RenewalValidator

Responsible for validating renewal input parameters.

Validates:
- Customer ID
- Plan code
- Seat count
- Payment method

This separates input validation from business processing.

Principle: Single Responsibility Principle (SRP).

### 2. DiscountCalculator

Handles discount calculations based on:
- Customer segment
- Customer loyalty duration
- Team size
- Loyalty points

Existing discount rates and calculation order are preserved.

Principles: SRP and high cohesion.

### 3. FeeCalculator

Handles premium support fees and payment fees.

Premium support fees:
- START: 250
- PRO: 400
- ENTERPRISE: 700

Payment fees:
- CARD: 2%
- BANK_TRANSFER: 1%
- PAYPAL: 3.5%
- INVOICE: 0%

Principles: SRP and high cohesion.

### 4. TaxCalculator

Calculates taxes based on customer country.

Existing rates:
- Poland: 23%
- Germany: 19%
- Czech Republic: 21%
- Norway: 25%
- Other countries: 20%

Principle: SRP.

### 5. InvoiceFactory

Creates RenewalInvoice objects using calculated values.

Responsible for:
- Invoice number generation
- Invoice field assignment
- Monetary rounding
- Invoice notes

Principle: SRP.

### 6. Billing Gateway Adapter

An IBillingGateway interface was introduced.

LegacyBillingGatewayAdapter implements this interface
and delegates operations to the original static
LegacyBillingGateway class.

The original LegacyBillingGateway was not modified.

This reduces direct coupling between the renewal
service and the legacy static dependency.

Principles: Dependency Inversion Principle (DIP)
and Adapter Pattern.

### 7. Dependency Injection and IoC

SubscriptionRenewalService receives its dependencies
through constructor injection.

RenewalDependencies provides default dependency
configuration using a simple manual IoC approach.

The original parameterless constructor remains available:

```csharp
var service = new SubscriptionRenewalService();
```

This preserves compatibility with the existing consumer.

### 8. Strategy Pattern

Payment method calculations use the Strategy Pattern.

The IPaymentFeeStrategy interface is implemented by:

- CardPaymentStrategy
- BankTransferPaymentStrategy
- PayPalPaymentStrategy
- InvoicePaymentStrategy

FeeCalculator selects the appropriate strategy
without using a large payment-method if-else chain.

New payment methods can be introduced by implementing
IPaymentFeeStrategy and registering the implementation.

Principle: Open/Closed Principle (OCP).

## SOLID and Design Principles

### Single Responsibility Principle

Validation, discount calculation, fee calculation,
tax calculation, and invoice creation are separated
into dedicated classes.

### Open/Closed Principle

Payment strategies support extension without changing
the existing payment calculation algorithm.

### Dependency Inversion Principle

The renewal service communicates with the billing
gateway through the IBillingGateway abstraction.

Other dependencies are managed using constructor
injection and a simple composition mechanism.

### DRY

Business rules are organized into reusable components,
reducing duplicated responsibilities.

### High Cohesion

Each extracted calculator or service focuses on
a clearly defined responsibility.

### Low Coupling

The main renewal service delegates calculations
and gateway operations to specialized components.

## Preserved Constraints

The following original components were not modified:

- LegacyRenewalAppConsumer/Program.cs
- LegacyRenewalAppConsumer/LegacyRenewalAppConsumer.csproj
- LegacyRenewalApp/LegacyBillingGateway.cs

Their SHA-256 hashes were compared with the original
project files and confirmed to match.

The public CreateRenewalInvoice method and the
parameterless SubscriptionRenewalService constructor
remain available to the consumer.

## Testing

The project uses xUnit for automated unit testing.

Tested components:

- RenewalValidator
- DiscountCalculator
- FeeCalculator
- TaxCalculator
- InvoiceFactory

The tests cover:

- Valid and invalid input
- Customer segment discounts
- Education discount eligibility
- Loyalty discounts
- Team size discounts
- Loyalty point limits
- Premium support fees
- Payment method fees
- Unsupported payment methods
- Country tax rates
- Invoice rounding and notes

Final local test result:

- Total: 35
- Passed: 35
- Failed: 0
- Skipped: 0

GitHub Actions also performs automated output
verification of the consumer application.

## How to Build

Requirements:

- .NET 8 SDK

From the solution directory, run:

```bash
dotnet restore RefactoringRenewalExample.sln
dotnet build RefactoringRenewalExample.sln
```

## How to Run

```bash
dotnet run --project ./LegacyRenewalAppConsumer
```

The reference consumer scenario produces:

```text
Invoice created successfully
Final amount: 17671.67
```

The invoice number includes the current UTC date,
so its date component may vary between executions.

## How to Run Unit Tests

```bash
dotnet test RefactoringRenewalExample.sln
```

All 35 unit tests should pass.

## Continuous Integration

GitHub Actions is configured to:

1. Restore .NET dependencies.
2. Build the solution in Release configuration.
3. Execute xUnit tests.
4. Run the original consumer application.
5. Verify expected invoice output.

Workflow file:

`.github/workflows/build.yml`

## Refactoring Outcome

The original SubscriptionRenewalService contained
multiple responsibilities in one long method.

After refactoring, calculations and supporting operations
are organized into dedicated components.

The tested consumer scenario produces the same final
amount and invoice notes as the original application.

The refactoring improves code organization,
maintainability, testability, cohesion, and coupling
while preserving the existing public consumer contract.

## Conclusion

This project demonstrates safe refactoring of legacy
C# code using SOLID, DRY, dependency injection,
a simple IoC composition mechanism, Adapter Pattern,
and Strategy Pattern.

The original consumer and legacy billing gateway
remain unchanged.
