using FluentValidation;
using ProductCatalog.Api.Application.DTOs;

namespace ProductCatalog.Api.Application.Validators;

public sealed class CreateBodegaValidator : AbstractValidator<CreateBodegaDto>
{
    public CreateBodegaValidator()
    {
        RuleFor(x => x.BodNombre)
            .NotEmpty().WithMessage("El nombre de la bodega es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");
    }
}

public sealed class UpdateBodegaValidator : AbstractValidator<UpdateBodegaDto>
{
    public UpdateBodegaValidator()
    {
        RuleFor(x => x.BodNombre)
            .NotEmpty().WithMessage("El nombre de la bodega es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");
    }
}
