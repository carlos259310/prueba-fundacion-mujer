using FluentValidation;
using ProductCatalog.Api.Application.DTOs;

namespace ProductCatalog.Api.Application.Validators;

public sealed class AjustarStockValidator : AbstractValidator<AjustarStockDto>
{
    public AjustarStockValidator()
    {
        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.")
            .LessThanOrEqualTo(999_999).WithMessage("La cantidad máxima es 999 999. Las cantidades son enteros sin decimales.");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de movimiento no válido.");

        RuleFor(x => x.Concepto)
            .IsInEnum().WithMessage("Concepto de movimiento no válido.");
    }
}
