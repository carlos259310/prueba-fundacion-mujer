# ProductCatalog — Sistema de Inventario por Bodegas

> API REST + interfaz web para gestión de catálogo de productos e inventario distribuido por bodegas.
> Desarrollada en **.NET 8 Minimal APIs** con arquitectura limpia, PostgreSQL (Supabase) y frontend Bootstrap 5.

---

## Índice

- [Vista rápida del sistema](#vista-rápida-del-sistema)
- [Stack tecnológico](#stack-tecnológico)
- [Arquitectura del proyecto](#arquitectura-del-proyecto)
- [Base de datos — Diseño y esquema](#base-de-datos--diseño-y-esquema)
- [Vistas del frontend](#vistas-del-frontend)
- [API — Endpoints disponibles](#api--endpoints-disponibles)
- [Reglas de negocio](#reglas-de-negocio)
- [Cómo clonar y correr localmente](#cómo-clonar-y-correr-localmente)
- [Correr con Docker](#correr-con-docker)
- [Variables de entorno](#variables-de-entorno)
- [Comandos útiles de la CLI](#comandos-útiles-de-la-cli)
- [Despliegue en Railway](#despliegue-en-railway)

---

## Vista rápida del sistema

El sistema expone una **interfaz web completa** servida desde el mismo servidor junto con la API REST. No necesita un frontend separado.

```
http://localhost:5080/             → Dashboard con estadísticas en tiempo real
http://localhost:5080/bodegas.html → Gestión de bodegas (CRUD completo)
http://localhost:5080/productos.html → Catálogo de productos (paginado)
http://localhost:5080/inventario.html → Stock por bodega con ajuste directo
http://localhost:5080/movimientos.html → Historial de movimientos y reportes
http://localhost:5080/about.html   → Documentación técnica del proyecto
http://localhost:5080/swagger      → Swagger UI interactivo (prueba todos los endpoints)
```

---

## Stack tecnológico

| Capa | Tecnología | Versión |
|---|---|---|
| **Runtime** | .NET | 8.0 |
| **API** | ASP.NET Core Minimal APIs | 8.0 |
| **ORM** | Entity Framework Core + Npgsql | 8.0.0 |
| **Base de datos** | PostgreSQL (Supabase) | 15+ |
| **Validaciones** | FluentValidation | 11.3.0 |
| **Documentación API** | Swashbuckle (Swagger) | 6.8.1 |
| **Frontend** | Bootstrap | 5.3.3 |
| **Íconos** | Bootstrap Icons | 1.11.3 |
| **JS** | Vanilla JS (ES2020+) | — |
| **Contenedor** | Docker (multi-stage) | — |
| **Nube** | Railway + Supabase | — |

---

## Arquitectura del proyecto

```
ProductCatalog.Api/
│
├── Domain/                   # Núcleo del dominio (sin dependencias externas)
│   ├── Entities/
│   │   ├── Bodega.cs          ← Entidad almacén
│   │   ├── Producto.cs        ← Entidad producto
│   │   ├── Inventario.cs      ← Entidad stock (PK compuesta: prod_id + bod_id)
│   │   ├── Movimiento.cs      ← Entidad movimiento de inventario
│   │   ├── TipoMovimiento.cs  ← Enum: Entrada | Salida | Traslado
│   │   └── ConceptoMovimiento.cs ← Enum: Compra | Venta | Ajuste | Traslado | Devolucion
│   └── Exceptions/
│       └── Exceptions.cs      ← NotFoundException, StockInsuficienteException, etc.
│
├── Application/              # Casos de uso y contratos
│   ├── DTOs/
│   │   ├── BodegaDtos.cs      ← CreateBodegaDto, UpdateBodegaDto, BodegaDto
│   │   ├── ProductoDtos.cs    ← CreateProductoDto, UpdateProductoDto, ProductoDto
│   │   ├── InventarioDtos.cs  ← InventarioDto, AjustarStockDto
│   │   ├── MovimientoDtos.cs  ← CreateMovimientoDto, MovimientoDto, ReporteMovimientosDto
│   │   └── PagedResult.cs     ← Wrapper de paginación genérico
│   ├── Interfaces/
│   │   ├── IBodegaRepository.cs / IBodegaService.cs
│   │   ├── IProductoRepository.cs / IProductoService.cs
│   │   ├── IInventarioRepository.cs / IInventarioService.cs
│   │   └── IMovimientoRepository.cs / IMovimientoService.cs
│   ├── Services/              # Lógica de negocio
│   │   ├── BodegaService.cs
│   │   ├── ProductoService.cs
│   │   ├── InventarioService.cs
│   │   └── MovimientoService.cs
│   └── Validators/            # FluentValidation
│       ├── BodegaValidators.cs
│       ├── ProductoValidators.cs
│       ├── InventarioValidators.cs
│       └── MovimientoValidators.cs
│
├── Infrastructure/            # Implementaciones de persistencia
│   ├── AppDbContext.cs        ← DbContext EF Core con configuración de tablas
│   ├── Repositories/
│   │   ├── BodegaRepository.cs
│   │   ├── ProductoRepository.cs
│   │   ├── InventarioRepository.cs
│   │   └── MovimientoRepository.cs
│   └── Seeders/
│       ├── InitialDataSeeder.cs  ← Datos iniciales (1 bodega + 5 productos)
│       └── DatabaseResetter.cs   ← Reset completo para desarrollo
│
├── Endpoints/                 # Handlers de la API y middleware
│   ├── BodegaEndpoints.cs
│   ├── ProductoEndpoints.cs
│   ├── InventarioEndpoints.cs
│   ├── MovimientoEndpoints.cs
│   ├── ExceptionMiddleware.cs ← Convierte excepciones de dominio a respuestas HTTP
│   ├── EnumSchemaFilter.cs    ← Enums como strings en Swagger
│   └── TagDescriptionFilter.cs
│
├── Migrations/                # Migraciones EF Core (auto-aplicadas al iniciar)
│
├── wwwroot/                   # Frontend estático servido por Kestrel
│   ├── index.html             ← Dashboard
│   ├── bodegas.html
│   ├── productos.html
│   ├── inventario.html
│   ├── movimientos.html
│   ├── about.html
│   ├── app.css                ← Tema visual (paleta morada corporativa)
│   └── js/
│       ├── bodegas.js
│       ├── productos.js
│       ├── inventario.js
│       └── movimientos.js
│
├── Program.cs                 ← Configuración, DI, middleware y startup
├── Dockerfile                 ← Build multi-stage (SDK → Runtime)
├── appsettings.json           ← Conexión a Supabase (incluida para facilitar evaluación)
└── railway.toml               ← Configuración de despliegue
```

### Flujo de una request

```
HTTP Request
    │
    ▼
ExceptionMiddleware       ← captura cualquier excepción y devuelve JSON limpio
    │
    ▼
CORS Middleware
    │
    ▼
Static Files (wwwroot/)   ← sirve el frontend si coincide con un archivo físico
    │
    ▼
Endpoint Routing
    │
    ├─ Bodega/Producto/Inventario/Movimiento Endpoint handler
    │       │
    │       ▼
    │   FluentValidation  ← valida DTOs antes de pasar al servicio
    │       │
    │       ▼
    │   Service           ← aplica reglas de negocio
    │       │
    │       ▼
    │   Repository        ← accede a PostgreSQL via EF Core
    │
    ▼
HTTP Response (JSON)
```

---

## Base de datos — Diseño y esquema

La base de datos se crea y actualiza **automáticamente al iniciar la API** mediante `MigrateAsync()`. No se necesita correr `dotnet ef` manualmente.

### Diagrama entidad-relación

```
┌─────────────────────┐          ┌──────────────────────────────────┐
│      bodegas        │          │            productos              │
├─────────────────────┤          ├──────────────────────────────────┤
│ bod_id         INT  │◄──┐      │ prod_id       INT  PK AUTO       │
│ bod_nombre    VC100 │   │      │ prod_nombre   VC200 NOT NULL      │
│ bod_principal  BOOL │   │      │ prod_codigo   VC50  UNIQUE        │
└─────────────────────┘   │      │ prod_descripcion TEXT NULL        │
          ▲               │      │ prod_marca    VC100 NULL          │
          │               │      └──────────────────────────────────┘
          │               │                     ▲
          │               │                     │
┌─────────────────────────────────┐             │
│           inventario            │             │
├─────────────────────────────────┤             │
│ prod_id    INT  FK → productos  │─────────────┘
│ bod_id     INT  FK → bodegas    │──────────────┘
│ inv_stock  INT  DEFAULT 0       │   PK compuesta:
│ inv_last_update  TIMESTAMPTZ    │   (prod_id, bod_id)
└─────────────────────────────────┘
          
┌─────────────────────────────────┐
│           movimientos           │
├─────────────────────────────────┤
│ mov_id            INT  PK AUTO  │
│ prod_id           INT  FK → productos
│ mov_bodega_inicial INT  NULL    │ ← id de bodega origen (nullable)
│ mov_bodega_final   INT  NULL    │ ← id de bodega destino (nullable)
│ mov_cantidad      INT  NOT NULL │
│ mov_tipo          TEXT NOT NULL │ ← Entrada | Salida | Traslado
│ mov_concepto      TEXT NOT NULL │ ← Compra | Venta | Ajuste | Traslado | Devolucion
│ mov_fecha         TIMESTAMPTZ  │
└─────────────────────────────────┘
```

### Detalle de tablas

#### `bodegas`
| Columna | Tipo PostgreSQL | Restricciones | Notas |
|---|---|---|---|
| `bod_id` | `integer` | PK, IDENTITY | Auto-incremental |
| `bod_nombre` | `varchar(100)` | NOT NULL | Nombre del almacén |
| `bod_principal` | `boolean` | NOT NULL, DEFAULT false | Marca la bodega primaria |

#### `productos`
| Columna | Tipo PostgreSQL | Restricciones | Notas |
|---|---|---|---|
| `prod_id` | `integer` | PK, IDENTITY | Auto-incremental |
| `prod_nombre` | `varchar(200)` | NOT NULL | Nombre del producto |
| `prod_codigo` | `varchar(50)` | NOT NULL, UNIQUE | Código único de referencia |
| `prod_descripcion` | `text` | NULL | Descripción libre |
| `prod_marca` | `varchar(100)` | NULL | Fabricante o marca |

#### `inventario`
| Columna | Tipo PostgreSQL | Restricciones | Notas |
|---|---|---|---|
| `prod_id` | `integer` | PK (1/2), FK → `productos` | Cascade DELETE |
| `bod_id` | `integer` | PK (2/2), FK → `bodegas` | Cascade DELETE |
| `inv_stock` | `integer` | NOT NULL, DEFAULT 0 | Nunca puede ser negativo |
| `inv_last_update` | `timestamp with time zone` | NOT NULL | Actualizado en cada movimiento |

> El stock se almacena **por combinación producto-bodega**, no existe stock global.
> Si un producto tiene stock en 3 bodegas, hay 3 registros en esta tabla.

#### `movimientos`
| Columna | Tipo PostgreSQL | Restricciones | Notas |
|---|---|---|---|
| `mov_id` | `integer` | PK, IDENTITY | Auto-incremental |
| `prod_id` | `integer` | NOT NULL, FK → `productos` | Cascade DELETE |
| `mov_bodega_inicial` | `integer` | NULL | Requerido para Salida y Traslado |
| `mov_bodega_final` | `integer` | NULL | Requerido para Entrada y Traslado |
| `mov_cantidad` | `integer` | NOT NULL | Entero positivo sin decimales (1–999 999) |
| `mov_tipo` | `text` | NOT NULL | `Entrada` / `Salida` / `Traslado` |
| `mov_concepto` | `text` | NOT NULL | `Compra` / `Venta` / `Ajuste` / `Traslado` / `Devolucion` |
| `mov_fecha` | `timestamp with time zone` | NOT NULL | UTC, se asigna en el servidor |

### Efecto de cada tipo de movimiento

| Tipo | `mov_bodega_inicial` | `mov_bodega_final` | Efecto en inventario |
|---|---|---|---|
| `Entrada` | — (null) | Requerida | Suma `mov_cantidad` al stock de bodega destino |
| `Salida` | Requerida | — (null) | Resta `mov_cantidad` del stock de bodega origen |
| `Traslado` | Requerida | Requerida | Resta de origen **y** suma a destino en una sola operación |

### Datos iniciales (seeder)

Al arrancar con la base de datos vacía, el sistema siembra automáticamente:

**1 Bodega:**
- `Bodega Principal` (bod_principal = true)

**5 Productos:**
| Nombre | Código | Marca |
|---|---|---|
| Laptop Dell XPS 15 | `LAP-001` | Dell |
| Mouse Logitech MX Master 3 | `MOU-001` | Logitech |
| Teclado Mecánico Keychron K2 | `TEC-001` | Keychron |
| Monitor LG UltraWide 34" | `MON-001` | LG |
| Audífonos Sony WH-1000XM5 | `AUD-001` | Sony |

---

## Vistas del frontend

El frontend está construido en **HTML + Vanilla JS + Bootstrap 5.3.3**, servido directamente por Kestrel desde `wwwroot/`. No hay framework SPA — cada página es un HTML independiente que consume la API con `fetch`.

### Paleta de colores (tema corporativo morado)

```css
--brand:        #6b21a8   /* Morado principal */
--brand-dark:   #4c1572   /* Navbar, footer */
--brand-hover:  #7e22ce   /* Estados hover */
--brand-light:  #f3e8ff   /* Fondos de tarjetas */
```

---

### `/` — Dashboard (`index.html`)

**Propósito:** Punto de entrada del sistema con métricas en tiempo real.

**Componentes:**
- **Hero section** con gradiente morado y logo del sistema
- **4 tarjetas de estadísticas** (se cargan con `fetch` al abrir la página):
  - Bodegas registradas → `GET /api/bodegas`
  - Total de productos → `GET /api/productos?pageSize=1` (usa el campo `total`)
  - Movimientos de hoy → `GET /api/movimientos/reporte?fechaDesde={hoy}&fechaHasta={hoy}`
  - Registros de stock → `GET /api/inventario`
- **4 tarjetas de módulos** con enlace a cada sección
- **Banner de API Docs** con enlace directo a Swagger UI

---

### `/bodegas.html` — Gestión de Bodegas

**Propósito:** CRUD completo para administrar almacenes o puntos de distribución.

**Funciones:**
- Listar todas las bodegas en tabla con badge de "Principal" / "Secundaria"
- **Crear** bodega → botón "Nueva bodega" → modal con validación
- **Editar** bodega → botón en la fila → modal pre-cargado con datos actuales
- **Eliminar** bodega → botón en la fila → confirmación en modal antes de borrar
- Indicador visual de bodega principal (badge morado)
- Toast de confirmación al crear / editar / eliminar

**Campos del formulario:**
| Campo | Tipo | Obligatorio | Validación |
|---|---|---|---|
| Nombre | Texto | Sí | 1–100 caracteres |
| Bodega principal | Checkbox | No | — |

**Endpoints que consume:**
```
GET    /api/bodegas           → cargar tabla al iniciar
POST   /api/bodegas           → crear
PUT    /api/bodegas/{id}      → editar
DELETE /api/bodegas/{id}      → eliminar
```

---

### `/productos.html` — Catálogo de Productos

**Propósito:** Gestión paginada del catálogo de productos.

**Funciones:**
- Tabla paginada (10 por página) con controles de navegación y contador
- **Crear** producto → modal con todos los campos
- **Editar** producto → modal pre-cargado con datos del producto
- **Eliminar** producto → confirmación antes de borrar
- Manejo de error 409 cuando se intenta usar un código ya existente
- Visualización de código, marca y descripción en la tabla

**Campos del formulario:**
| Campo | Tipo | Obligatorio | Validación |
|---|---|---|---|
| Nombre | Texto | Sí | 1–200 caracteres |
| Código | Texto | Sí | 1–50 caracteres, único |
| Marca | Texto | No | Máx. 100 caracteres |
| Descripción | Textarea | No | Texto libre |

**Endpoints que consume:**
```
GET    /api/productos?page=1&pageSize=10  → tabla paginada
POST   /api/productos                     → crear (maneja 409 Conflict)
PUT    /api/productos/{id}                → editar
DELETE /api/productos/{id}               → eliminar
```

---

### `/inventario.html` — Gestión de Stock

**Propósito:** Visualizar el stock actual de todos los productos por bodega y hacer ajustes directos.

**Funciones:**
- Muestra **todos** los registros de stock al cargar (sin necesidad de filtrar)
- Filtro opcional por producto (select con todos los productos cargados al inicio)
- Tabla con columnas: Producto, Bodega, Stock actual (en rojo si es 0), botón Ajustar
- **Ajuste directo de stock** → modal con:
  - Selección de tipo: `Entrada` (suma) o `Salida` (resta)
  - Selección de concepto: Compra, Venta, Ajuste, Devolución
  - Cantidad (número positivo)
- Previene stock negativo (el backend responde 400 y el frontend muestra el error)
- Toast de confirmación al ajustar correctamente

> **Diferencia con Movimientos:** el ajuste directo de stock en esta vista usa `PATCH` 
> sobre el inventario. Para registrar traslados entre bodegas, usar la vista de Movimientos.

**Endpoints que consume:**
```
GET   /api/inventario           → todos los registros de stock
GET   /api/inventario?prodId=X  → filtrado por producto
GET   /api/productos?pageSize=200  → poblar el select de filtro
PATCH /api/productos/{id}/stock/{bodId}  → ajuste de stock
```

---

### `/movimientos.html` — Historial y Reportes

**Propósito:** Registrar movimientos de inventario (entradas, salidas, traslados) y consultar historial y reportes por fecha.

La página tiene **dos pestañas**:

#### Pestaña: Historial

- Muestra **todos** los movimientos al cargar (sin filtro inicial)
- Filtro opcional por producto
- Tabla paginada (10 por página) con:
  - Fecha y hora del movimiento
  - Nombre del producto
  - Tipo (badge de color: verde=Entrada, rojo=Salida, morado=Traslado)
  - Concepto (Compra, Venta, Ajuste, etc.)
  - Bodega origen y bodega destino (resueltos desde el mapa de bodegas cargado al inicio)
  - Cantidad
- Controles de paginación con contador "Mostrando X–Y de Z movimientos"

#### Pestaña: Reporte

- Filtros: fecha desde, fecha hasta (obligatorias), producto (opcional)
- Botón "Generar" → llama al endpoint de reporte
- **4 tarjetas resumen:** Total movimientos, Entradas, Salidas, Traslados
- Tabla detalle agrupada por día con totales por tipo

#### Modal: Registrar movimiento

Botón "Registrar movimiento" (visible en ambas pestañas) abre un modal con:

| Campo | Tipo | Lógica condicional |
|---|---|---|
| Producto | Select | Siempre visible |
| Tipo | Select | Entrada / Salida / Traslado |
| Concepto | Select | Siempre visible |
| Cantidad | Número | Siempre visible |
| Bodega origen | Select | Visible solo para Salida y Traslado |
| Bodega destino | Select | Visible solo para Entrada y Traslado |

Los campos de bodega aparecen y desaparecen dinámicamente según el tipo seleccionado.

**Endpoints que consume:**
```
GET  /api/bodegas                       → mapa bodId→bodNombre para el historial
GET  /api/productos?pageSize=200        → poblar selects
GET  /api/movimientos                   → historial completo paginado
GET  /api/movimientos?prodId=X          → filtrado por producto
GET  /api/movimientos/reporte?fechaDesde=yyyy-MM-dd&fechaHasta=yyyy-MM-dd  → reporte por día
POST /api/movimientos                   → registrar nuevo movimiento
```

---

### `/about.html` — Documentación del Proyecto

**Propósito:** Página estática con documentación técnica del sistema.

**Secciones:**
- Enunciado y requisitos de la prueba técnica
- Decisiones técnicas tomadas (framework, arquitectura, base de datos, etc.)
- Checklist de funcionalidades implementadas
- Referencia de todos los endpoints agrupados por módulo
- Badges del stack tecnológico

---

### `/swagger` — Swagger UI

**Propósito:** Documentación interactiva de la API con posibilidad de probar todos los endpoints directamente desde el navegador.

Incluye:
- Descripción detallada de cada endpoint con códigos de respuesta (`200`, `201`, `204`, `400`, `404`, `409`)
- Esquemas de los DTOs con descripción de cada campo
- Formato explícito de fechas (`string($date)` = `yyyy-MM-dd`) en el endpoint de reporte
- Cantidades documentadas como enteros positivos sin decimales (mínimo: 1, máximo: 999 999)
- Tabla de reglas de negocio y requisitos de bodega por tipo de movimiento
- Enums documentados como strings (no como números)
- Estilo visual personalizado con la paleta corporativa

---

## API — Endpoints disponibles

### Bodegas

| Método | Ruta | Descripción | Respuesta exitosa |
|---|---|---|---|
| `GET` | `/api/bodegas` | Listar todas las bodegas | `200 [BodegaDto]` |
| `GET` | `/api/bodegas/{id}` | Obtener bodega por ID | `200 BodegaDto` |
| `POST` | `/api/bodegas` | Crear bodega | `201 BodegaDto` |
| `PUT` | `/api/bodegas/{id}` | Actualizar bodega | `200 BodegaDto` |
| `DELETE` | `/api/bodegas/{id}` | Eliminar bodega | `204 No Content` |

### Productos

| Método | Ruta | Query params | Descripción | Respuesta exitosa |
|---|---|---|---|---|
| `GET` | `/api/productos` | `page`, `pageSize` | Listar (paginado) | `200 PagedResult<ProductoDto>` |
| `GET` | `/api/productos/{id}` | — | Obtener por ID | `200 ProductoDto` |
| `POST` | `/api/productos` | — | Crear producto | `201 ProductoDto` |
| `PUT` | `/api/productos/{id}` | — | Actualizar producto | `200 ProductoDto` |
| `DELETE` | `/api/productos/{id}` | — | Eliminar producto | `204 No Content` |

### Inventario

| Método | Ruta | Query params | Descripción | Respuesta exitosa |
|---|---|---|---|---|
| `GET` | `/api/inventario` | `prodId` (opcional) | Todo el stock (filtrable) | `200 [InventarioDto]` |
| `GET` | `/api/productos/{id}/stock` | — | Stock de un producto en todas las bodegas | `200 [InventarioDto]` |
| `PATCH` | `/api/productos/{id}/stock/{bodId}` | — | Ajuste directo de stock | `200 InventarioDto` |

### Movimientos

| Método | Ruta | Query params | Descripción | Respuesta exitosa |
|---|---|---|---|---|
| `GET` | `/api/movimientos` | `prodId`, `page`, `pageSize` | Historial completo (paginado) | `200 PagedResult<MovimientoDto>` |
| `GET` | `/api/movimientos/{prodId}` | `page`, `pageSize` | Historial de un producto | `200 PagedResult<MovimientoDto>` |
| `GET` | `/api/movimientos/reporte` | `fechaDesde` (yyyy-MM-dd), `fechaHasta` (yyyy-MM-dd), `prodId` | Reporte agrupado por día | `200 ReporteMovimientosDto` |
| `POST` | `/api/movimientos` | — | Registrar movimiento | `201 MovimientoDto` |

### Códigos de error

| Código | Situación |
|---|---|
| `400` | Validación fallida, stock insuficiente, bodega inválida, cantidad fuera de rango (1–999 999), tipo Traslado en ajuste directo |
| `404` | Recurso no encontrado (producto o bodega inexistentes) |
| `409` | Código de producto duplicado |
| `500` | Error interno del servidor (responde siempre en JSON) |

### Ejemplos de body

**Crear bodega:**
```json
{
  "bodNombre": "Bodega Norte",
  "bodPrincipal": false
}
```

**Crear producto:**
```json
{
  "prodNombre": "Laptop Dell XPS 15",
  "prodCodigo": "LAP-001",
  "prodMarca": "Dell",
  "prodDescripcion": "Procesador i7, 16GB RAM, 512GB SSD"
}
```

**Registrar movimiento — Entrada:**
```json
{
  "prodId": 1,
  "movTipo": "Entrada",
  "movConcepto": "Compra",
  "movCantidad": 50,
  "movBodegaFinal": 1
}
```

**Registrar movimiento — Traslado:**
```json
{
  "prodId": 1,
  "movTipo": "Traslado",
  "movConcepto": "Traslado",
  "movCantidad": 10,
  "movBodegaInicial": 1,
  "movBodegaFinal": 2
}
```

**Ajuste directo de stock (PATCH):**
```json
{
  "cantidad": 25,
  "tipo": "Salida",
  "concepto": "Venta"
}
```

---

## Reglas de negocio

1. **Stock nunca negativo** — Si la cantidad solicitada supera el stock disponible, la API responde `400` con el mensaje de error exacto:
   ```json
   { "error": "Stock insuficiente. Stock actual: 10, cantidad solicitada: 15." }
   ```

2. **Código de producto único** — El campo `prod_codigo` tiene restricción UNIQUE. Un intento de duplicar devuelve `409 Conflict`.

3. **Bodegas requeridas según tipo de movimiento:**
   - `Entrada` → solo `movBodegaFinal` (dónde llega la mercancía)
   - `Salida` → solo `movBodegaInicial` (de dónde sale)
   - `Traslado` → ambas bodegas obligatorias (origen y destino)

4. **Traslado no permitido en ajuste directo** — El endpoint `PATCH /api/productos/{id}/stock/{bodId}` solo acepta `Entrada` o `Salida`. Para traslados, usar `POST /api/movimientos`.

5. **Registros de inventario son upsert** — Al registrar el primer movimiento de un producto en una bodega, el registro de inventario se crea automáticamente con stock 0 y luego se ajusta. No es necesario crear el registro previamente.

6. **Eliminación en cascada** — Al eliminar un producto, se eliminan automáticamente sus registros de inventario y movimientos asociados. Al eliminar una bodega, se elimina el inventario de esa bodega.

---

## Cómo clonar y correr localmente

### Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8) instalado
- Conexión a internet (la cadena de conexión a Supabase ya está incluida en `appsettings.json`)

> **No necesitas instalar PostgreSQL localmente.** La base de datos ya está en Supabase y la cadena de conexión está en el repositorio. Esto es intencional para facilitar la evaluación de la prueba técnica. En producción real, las credenciales irían en variables de entorno.

### Pasos

```bash
# 1. Clonar el repositorio
git clone https://github.com/carlos259310/prueba-fundacion-mujer.git
cd prueba-fundacion-mujer

# 2. Restaurar dependencias NuGet
dotnet restore

# 3. Correr la API
dotnet run
```

Eso es todo. Al iniciar, la API:
- Aplica las migraciones automáticamente (crea las tablas si no existen)
- Siembra datos iniciales si la base de datos está vacía
- Inicia el servidor en `http://localhost:5080`

### Verificar que funciona

Abrir en el navegador:
- `http://localhost:5080` → Dashboard del sistema
- `http://localhost:5080/swagger` → Swagger UI con todos los endpoints

O via `curl`:
```bash
curl http://localhost:5080/api/bodegas
```

---

## Correr con Docker

### Opción 1 — Solo con Docker (sin clonar)

```bash
docker build -t productcatalog https://github.com/carlos259310/prueba-fundacion-mujer.git
docker run -p 5080:8080 productcatalog
```

### Opción 2 — Con código local

```bash
# Clonar y construir imagen
git clone https://github.com/carlos259310/prueba-fundacion-mujer.git
cd prueba-fundacion-mujer
docker build -t productcatalog .

# Correr contenedor
docker run -p 5080:8080 productcatalog

# O con variable de entorno de conexión personalizada
docker run -p 5080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=...;Port=5432;..." \
  productcatalog
```

Acceder en `http://localhost:5080`.

### Dockerfile (referencia)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENTRYPOINT ["dotnet", "ProductCatalog.Api.dll"]
```

La variable `PORT` es asignada dinámicamente por Railway. Localmente usa el puerto `8080` del contenedor, mapeado al `5080` del host en el comando `docker run -p 5080:8080`.

---

## Variables de entorno

| Variable | Descripción | Valor por defecto |
|---|---|---|
| `PORT` | Puerto en que escucha la app | `5080` local, asignado por Railway en nube |
| `ConnectionStrings__DefaultConnection` | Cadena de conexión a PostgreSQL | Incluida en `appsettings.json` (Supabase) |

Para usar una base de datos propia, sobrescribir la cadena de conexión:

```bash
# Bash / Linux / macOS
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=productcatalog;Username=postgres;Password=tu_password"
dotnet run

# PowerShell (Windows)
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=productcatalog;Username=postgres;Password=tu_password"
dotnet run
```

Formato de la cadena de conexión para PostgreSQL:
```
Host=<host>;Port=5432;Database=<nombre_db>;Username=<usuario>;Password=<clave>;SSL Mode=Require;Trust Server Certificate=true
```

---

## Comandos útiles de la CLI

El proyecto incluye comandos especiales para gestionar la base de datos desde la terminal:

```bash
# Resetear la BD (elimina todos los datos)
dotnet run -- --reset

# Sembrar datos iniciales (solo si la BD está vacía)
dotnet run -- --seed

# Resetear Y resembrar en un solo paso
dotnet run -- --reset-seed
```

> Estos comandos son útiles durante el desarrollo para volver a un estado limpio.
> Tras ejecutar `--reset` o `--seed`, el proceso termina sin iniciar el servidor.

### Migraciones EF Core (solo si modificas el modelo)

```bash
# Instalar la herramienta EF (primera vez)
dotnet tool install --global dotnet-ef

# Crear una nueva migración tras cambiar entidades
dotnet ef migrations add NombreDeLaMigracion

# Aplicar migraciones manualmente (normalmente no es necesario)
dotnet ef database update
```

---

## Despliegue en Railway

La API está desplegada en **Railway** conectada a **Supabase (PostgreSQL)**.

### Pasos para desplegar tu propia instancia

1. Crear cuenta en [Railway](https://railway.app) y [Supabase](https://supabase.com)
2. En Supabase: crear un proyecto y copiar la connection string de PostgreSQL
3. En Railway: crear un nuevo proyecto → "Deploy from GitHub repo" → seleccionar el repositorio
4. En Railway: agregar variable de entorno:
   ```
   ConnectionStrings__DefaultConnection = <tu_connection_string_de_supabase>
   ```
5. Railway detecta el `Dockerfile` automáticamente y despliega

Railway asigna la variable `PORT` automáticamente. El `Dockerfile` y `Program.cs` ya están configurados para usarla:

```csharp
// Program.cs
var port = Environment.GetEnvironmentVariable("PORT") ?? "5080";
builder.WebHost.UseUrls($"http://+:{port}");
```

---

## Nota sobre credenciales

> La cadena de conexión a Supabase está visible en `appsettings.json` del repositorio.
> Esto es **intencional** para que la prueba técnica sea completamente replicable sin
> configuración adicional por parte del evaluador.
>
> En un proyecto productivo real, las credenciales jamás irían en el repositorio;
> se usarían variables de entorno, secretos de CI/CD o Azure Key Vault.

---

## Desarrollado por

**Ing. Carlos Rodriguez** — Prueba técnica para Fundación de la Mujer, 2026.
