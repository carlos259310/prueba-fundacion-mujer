using System.ComponentModel;

namespace ProductCatalog.Api.Application.DTOs;


public sealed record ClienteDto(
    [property: Description("ID único del cliente.")]
    int CliId,
    [property: Description("Nombre del cliente.")]
    string CliNombre,
    [property: Description("Apellido del cliente.")]
    string CliApellido,
    [property: Description("Correo electrónico (opcional).")]
    string? CliCorreo,
    [property: Description("Número de celular (opcional).")]
    string? CliCelular,
    [property: Description("Dirección (opcional).")]
    string? CliDireccion
);

public sealed record CreateClienteDto(
    [property: Description("Nombre. Requerido, máx. 100 caracteres.")]
    string CliNombre,
    [property: Description("Apellido. Requerido, máx. 100 caracteres.")]
    string CliApellido,
    [property: Description("Correo electrónico opcional. Máx. 150 caracteres.")]
    string? CliCorreo,
    [property: Description("Celular opcional. Máx. 20 caracteres.")]
    string? CliCelular,
    [property: Description("Dirección opcional. Máx. 200 caracteres.")]
    string? CliDireccion
);

public sealed record UpdateClienteDto(
    [property: Description("Nombre. Requerido, máx. 100 caracteres.")]
    string CliNombre,
    [property: Description("Apellido. Requerido, máx. 100 caracteres.")]
    string CliApellido,
    [property: Description("Correo electrónico opcional. Máx. 150 caracteres.")]
    string? CliCorreo,
    [property: Description("Celular opcional. Máx. 20 caracteres.")]
    string? CliCelular,
    [property: Description("Dirección opcional. Máx. 200 caracteres.")]
    string? CliDireccion
);

