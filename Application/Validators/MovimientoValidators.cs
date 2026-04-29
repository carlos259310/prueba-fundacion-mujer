using FluentValidation;
using ProductCatalog.Api.Application.DTOs;
using ProductCatalog.Api.Domain.Entities;

namespace ProductCatalog.Api.Application.Validators;

public sealed class CreateMovimientoValidator : AbstractValidator<CreateMovimientoDto>
{
    public CreateMovimientoValidator()
    {
        RuleFor(x => x.ProdId)
            .GreaterThan(0).WithMessage("El producto es requerido.");

        RuleFor(x => x.MovCantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");

        RuleFor(x => x.MovTipo)
            .IsInEnum().WithMessage("Tipo de movimiento no válido.");

        RuleFor(x => x.MovConcepto)
            .IsInEnum().WithMessage("Concepto de movimiento no válido.");

        RuleFor(x => x.MovBodegaFinal)
            .NotNull().WithMessage("La bodega destino es requerida para una entrada.")
            .When(x => x.MovTipo == TipoMovimiento.Entrada);

        RuleFor(x => x.MovBodegaInicial)
            .NotNull().WithMessage("La bodega origen es requerida para una salida.")
            .When(x => x.MovTipo == TipoMovimiento.Salida);

        RuleFor(x => x.MovBodegaInicial)
            .NotNull().WithMessage("La bodega origen es requerida para un traslado.")
            .When(x => x.MovTipo == TipoMovimiento.Traslado);

        RuleFor(x => x.MovBodegaFinal)
            .NotNull().WithMessage("La bodega destino es requerida para un traslado.")
            .When(x => x.MovTipo == TipoMovimiento.Traslado);
    }
}
