namespace ProductCatalog.Api.Domain.Entities;

public sealed class Cliente
{
    public int CliId { get; set; }
    public string CliNombre { get; set; } = string.Empty;
    public string CliApellido { get; set; } = string.Empty;
    public string? CliCorreo { get; set; }
    public string? CliCelular { get; set; }
    public string? CliDireccion { get; set; }
}
