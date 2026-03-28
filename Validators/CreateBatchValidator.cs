using FluentValidation;
using PharmacyInventoryAPI.DTOs;

namespace PharmacyInventoryAPI.Validators;

public class CreateBatchValidator : AbstractValidator<CreateBatchDto>
{
    public CreateBatchValidator()
    {
        RuleFor(x => x.BatchNumber)
            .NotEmpty().WithMessage("Batch number is required.")
            .MaximumLength(100);

        RuleFor(x => x.MedicationId)
            .GreaterThan(0).WithMessage("Valid medication ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than zero.");

        RuleFor(x => x.ManufactureDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Manufacture date cannot be in the future.");

        RuleFor(x => x.ExpiryDate)
            .GreaterThan(x => x.ManufactureDate).WithMessage("Expiry date must be after manufacture date.");

        RuleFor(x => x.Supplier)
            .NotEmpty().WithMessage("Supplier is required.")
            .MaximumLength(200);
    }
}

public class CreateStockTransactionValidator : AbstractValidator<CreateStockTransactionDto>
{
    public CreateStockTransactionValidator()
    {
        RuleFor(x => x.BatchId)
            .GreaterThan(0).WithMessage("Valid batch ID is required.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid transaction type.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.Reference)
            .MaximumLength(200);

        RuleFor(x => x.Notes)
            .MaximumLength(500);
    }
}
