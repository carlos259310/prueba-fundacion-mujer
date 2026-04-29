namespace ProductCatalog.Api.Domain.Exceptions;

public abstract class DomainException(string message) : Exception(message);

public sealed class NotFoundException(string entidad, object id)
    : DomainException($"{entidad} con id '{id}' no encontrado.");

public sealed class CodigoYaExisteException(string codigo)
    : DomainException($"Ya existe un producto con el código '{codigo}'.");

public sealed class StockInsuficienteException(int stockActual, int cantidad)
    : DomainException($"Stock insuficiente. Stock actual: {stockActual}, cantidad solicitada: {cantidad}.");

public sealed class StockNegativoException()
    : DomainException("El stock no puede ser negativo.");

public sealed class BodegaNoEncontradaException(int bodId)
    : DomainException($"La bodega con id '{bodId}' no existe.");
