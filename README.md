# ProductCatalog.Api

API REST para gestión de catálogo de productos e inventario por bodegas.
Desarrollada en **.NET 8** con arquitectura limpia (Clean Architecture).

---

## Tecnologías

- .NET 8 — Minimal APIs
- Entity Framework Core 8 + Npgsql
- PostgreSQL (Supabase)
- FluentValidation
- Swagger / Swashbuckle

---

## Arquitectura

```
Domain/          → entidades puras y excepciones de negocio
Application/     → interfaces, DTOs, servicios y validadores
Infrastructure/  → DbContext y repositorios (EF Core)
Endpoints/       → handlers de la API y middleware de excepciones
```

---

## Base de datos

El proyecto usa **Supabase (PostgreSQL)**. La cadena de conexión está incluida
en `appsettings.json` directamente.

> ⚠️ **Nota:** La cadena de conexión es visible en el repositorio únicamente
> porque este proyecto es una **prueba técnica** y se requiere que sea
> completamente replicable por el evaluador sin configuración adicional.
> En un proyecto productivo real, las credenciales irían en variables de
> entorno y nunca en el repositorio.

### Tablas creadas automáticamente vía migraciones EF Core

| Tabla | Descripción |
|---|---|
| `bodegas` | Almacenes o puntos de inventario |
| `productos` | Catálogo de productos |
| `inventario` | Stock por producto por bodega |
| `movimientos` | Historial de entradas, salidas y traslados |

---

## Cómo correr el proyecto localmente

### Requisitos
- .NET 8 SDK
- (Opcional) Docker — solo si quieres una instancia local de Postgres

### Pasos

```bash
# 1. Clonar el repositorio
git clone https://github.com/carlos259310/prueba-fundacion-mujer.git
cd prueba-fundacion-mujer

# 2. Restaurar dependencias
dotnet restore

# 3. Aplicar migraciones (crea las tablas en la BD)
dotnet ef database update

# 4. Correr la API
dotnet run
```

### Acceder a Swagger

```
http://localhost:5080/swagger
```

---

## Endpoints disponibles

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/health` | Estado de la API |
| GET | `/api/bodegas` | Listar bodegas |
| POST | `/api/bodegas` | Crear bodega |
| PUT | `/api/bodegas/{id}` | Actualizar bodega |
| DELETE | `/api/bodegas/{id}` | Eliminar bodega |
| GET | `/api/productos` | Listar productos (paginado) |
| POST | `/api/productos` | Crear producto |
| GET | `/api/productos/{id}` | Obtener producto |
| PUT | `/api/productos/{id}` | Actualizar producto |
| DELETE | `/api/productos/{id}` | Eliminar producto |
| GET | `/api/productos/{id}/stock` | Ver stock por bodega |
| PATCH | `/api/productos/{id}/stock/{bodId}` | Ajustar stock |
| GET | `/api/movimientos/{prodId}` | Historial de movimientos |
| POST | `/api/movimientos` | Registrar movimiento |

---

## Despliegue

La API está desplegada en **Railway** con la base de datos en **Supabase**.

URL pública: _(se agrega al finalizar el deploy)_
