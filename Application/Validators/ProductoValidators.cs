using FluentValidation;
using ProductCatalog.Api.Application.DTOs;

namespace ProductCatalog.Api.Application.Validators;

public sealed class CreateProductoValidator : AbstractValidator<CreateProductoDto>
{
    public CreateProductoValidator()
    {
        RuleFor(x => x.ProdNombre)
            .NotEmpty().WithMessage("El nombre del producto es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.ProdCodigo)
            .NotEmpty().WithMessage("El código del producto es requerido.")
            .MaximumLength(50).WithMessage("El código no puede superar 50 caracteres.");

        RuleFor(x => x.ProdDescripcion)
            .MaximumLength(500).WithMessage("La descripción no puede superar 500 caracteres.")
            .When(x => x.ProdDescripcion is not null);

        RuleFor(x => x.ProdMarca)
            .MaximumLength(100).WithMessage("La marca no puede superar 100 caracteres.")
            .When(x => x.ProdMarca is not null);
    }
}

public sealed class UpdateProductoValidator : AbstractValidator<UpdateProductoDto>
{
    public UpdateProductoValidator()
    {
        RuleFor(x => x.ProdNombre)
            .NotEmpty().WithMessage("El nombre del producto es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(x => x.ProdCodigo)
            .NotEmpty().WithMessage("El código del producto es requerido.")
            .MaximumLength(50).WithMessage("El código no puede superar 50 caracteres.");

        RuleFor(x => x.ProdDescripcion)
            .MaximumLength(500).WithMessage("La descripción no puede superar 500 caracteres.")
            .When(x => x.ProdDescripcion is not null);

        RuleFor(x => x.ProdMarca)
            .MaximumLength(100).WithMessage("La marca no puede superar 100 caracteres.")
            .When(x => x.ProdMarca is not null);
    }
}
