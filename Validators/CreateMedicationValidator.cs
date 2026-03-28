using FluentValidation;
using PharmacyInventoryAPI.DTOs;

namespace PharmacyInventoryAPI.Validators;

public class CreateMedicationValidator : AbstractValidator<CreateMedicationDto>
{
    public CreateMedicationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Medication name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.GenericName)
            .MaximumLength(200).WithMessage("Generic name must not exceed 200 characters.");

        RuleFor(x => x.Manufacturer)
            .NotEmpty().WithMessage("Manufacturer is required.")
            .MaximumLength(200);

        RuleFor(x => x.DosageForm)
            .NotEmpty().WithMessage("Dosage form is required.")
            .MaximumLength(100);

        RuleFor(x => x.Strength)
            .NotEmpty().WithMessage("Strength is required.")
            .MaximumLength(100);

        RuleFor(x => x.Category)
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder level must be non-negative.");
    }
}

public class UpdateMedicationValidator : AbstractValidator<UpdateMedicationDto>
{
    public UpdateMedicationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Medication name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Manufacturer)
            .NotEmpty().WithMessage("Manufacturer is required.")
            .MaximumLength(200);

        RuleFor(x => x.DosageForm)
            .NotEmpty().WithMessage("Dosage form is required.")
            .MaximumLength(100);

        RuleFor(x => x.Strength)
            .NotEmpty().WithMessage("Strength is required.")
            .MaximumLength(100);

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder level must be non-negative.");
    }
}
