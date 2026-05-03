using FluentValidation;
using ProductCatalog.Api.Application.DTOs;

namespace ProductCatalog.Api.Application.Validators;

public sealed class CreateClienteValidator : AbstractValidator<CreateClienteDto>
{
    public CreateClienteValidator()
    {
        RuleFor(x => x.CliNombre)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.CliApellido)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(100).WithMessage("El apellido no puede superar 100 caracteres.");

        RuleFor(x => x.CliCorreo)
            .MaximumLength(150).WithMessage("El correo no puede superar 150 caracteres.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.")
            .When(x => !string.IsNullOrEmpty(x.CliCorreo)); // solo valida si viene un valor

        RuleFor(x => x.CliCelular)
            .MaximumLength(20).WithMessage("El celular no puede superar 20 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.CliCelular));

        RuleFor(x => x.CliDireccion)
            .MaximumLength(200).WithMessage("La dirección no puede superar 200 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.CliDireccion));
    }
}

public sealed class UpdateClienteValidator : AbstractValidator<UpdateClienteDto>
{
    public UpdateClienteValidator()
    {
        RuleFor(x => x.CliNombre)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");

        RuleFor(x => x.CliApellido)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .MaximumLength(100).WithMessage("El apellido no puede superar 100 caracteres.");

        RuleFor(x => x.CliCorreo)
            .MaximumLength(150).WithMessage("El correo no puede superar 150 caracteres.")
            .EmailAddress().WithMessage("El correo no tiene un formato válido.")
            .When(x => !string.IsNullOrEmpty(x.CliCorreo));

        RuleFor(x => x.CliCelular)
            .MaximumLength(20).WithMessage("El celular no puede superar 20 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.CliCelular));

        RuleFor(x => x.CliDireccion)
            .MaximumLength(200).WithMessage("La dirección no puede superar 200 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.CliDireccion));
    }
}