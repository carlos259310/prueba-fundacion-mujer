# Guía: Desplegar una nueva instancia del sistema para otro cliente

Esta guía te lleva paso a paso a crear una copia completamente independiente del sistema
(nueva rama Git + nueva base de datos + nuevo deploy) para un cliente diferente.
Cada cliente queda **100% aislado**: sus datos no se mezclan con los de otros.

---

## Lo que vas a necesitar

- Cuenta en **GitHub** (ya la tienes)
- Cuenta en **Supabase** — https://supabase.com (gratis)
- Cuenta en **Railway** — https://railway.app (gratis)
- Terminal con `git` instalado

---

## PASO 1 — Crear la rama del nuevo cliente

Abre la terminal en la carpeta del proyecto y corre:

```bash
# Asegúrate de estar en main y tener todo al día
git checkout main
git pull origin main

# Crea la nueva rama con el nombre del cliente
git checkout -b cliente-nombre-empresa

# Súbela a GitHub
git push origin cliente-nombre-empresa
```

> Reemplaza `nombre-empresa` con el nombre real, por ejemplo: `cliente-bancolombia`

**¿Qué lograste?**
GitHub ahora tiene una rama separada con exactamente el mismo código de `main`.
Los cambios que hagas en esta rama no afectan a `main` ni a otros clientes.

---

## PASO 2 — Crear la base de datos en Supabase

1. Entra a https://supabase.com → **New project**
2. Ponle un nombre claro, por ejemplo: `inventario-nombre-empresa`
3. Elige una contraseña segura para la BD (guárdala)
4. Selecciona la región más cercana (ej: `South America (São Paulo)`)
5. Espera ~2 minutos a que cree el proyecto

**Obtener la connection string:**
1. En el proyecto Supabase → menú lateral **Settings** → **Database**
2. Busca la sección **Connection string** → pestaña **URI**
3. Copia la cadena, se ve así:
   ```
   postgresql://postgres:[TU-PASSWORD]@db.xxxxxxxxxxxx.supabase.co:5432/postgres
   ```
4. Reemplaza `[TU-PASSWORD]` con la contraseña que pusiste en el paso 3

> Las tablas se crean solas al iniciar la API — no necesitas hacer nada más en Supabase.

---

## PASO 3 — Crear el deploy en Railway

1. Entra a https://railway.app → **New project**
2. Elige **Deploy from GitHub repo**
3. Selecciona el repositorio `prueba-fundacion-mujer`
4. En **Branch**, selecciona la rama del cliente: `cliente-nombre-empresa`
5. Railway detecta el `Dockerfile` automáticamente → haz clic en **Deploy**

**Agregar la variable de entorno (conexión a Supabase):**
1. En Railway → tu proyecto → pestaña **Variables**
2. Agrega esta variable:
   ```
   ConnectionStrings__DefaultConnection = postgresql://postgres:[PASSWORD]@db.xxx.supabase.co:5432/postgres
   ```
   (la connection string que copiaste en el Paso 2)
3. Railway reinicia automáticamente con la nueva variable

**Obtener la URL pública:**
1. Railway → tu proyecto → pestaña **Settings** → sección **Domains**
2. Haz clic en **Generate Domain**
3. Copia la URL, por ejemplo: `inventario-cliente-abc.up.railway.app`

---

## PASO 4 — Verificar que todo funciona

Abre estas URLs en el navegador (reemplaza con tu URL de Railway):

| Qué probar | URL |
|---|---|
| Página principal | `https://tu-url.up.railway.app` |
| Swagger (API docs) | `https://tu-url.up.railway.app/swagger` |
| Bodegas (endpoint) | `https://tu-url.up.railway.app/api/bodegas` |

Si la página carga y el Swagger muestra los endpoints → **todo está funcionando**.

La base de datos ya tiene los datos iniciales (seeder automático):
- 1 Bodega Principal
- 5 Productos de ejemplo
- Stock inicial en la bodega

---

## PASO 5 — Hacer cambios específicos para el cliente (opcional)

Si necesitas personalizar algo para ese cliente (nombre de la empresa en el footer,
productos diferentes en el seeder, etc.):

```bash
# Asegúrate de estar en la rama del cliente
git checkout cliente-nombre-empresa

# Haz tus cambios en los archivos...

# Guarda y sube
git add .
git commit -m "Personalizar sistema para nombre-empresa"
git push origin cliente-nombre-empresa
```

Railway detecta el push y hace el redeploy automáticamente en ~2 minutos.

---

## Resumen visual

```
GitHub
├── main                        ← código base (tu proyecto original)
├── cliente-bancolombia         ← copia para Bancolombia
└── cliente-empresa-abc         ← copia para Empresa ABC

Railway
├── Deploy #1 → rama main       → BD Supabase proyecto original
├── Deploy #2 → rama bancolombia → BD Supabase proyecto bancolombia
└── Deploy #3 → rama empresa-abc → BD Supabase proyecto empresa-abc
```

Cada deploy tiene su propia URL y sus propios datos. Ninguno interfiere con el otro.

---

## Si actualizas el código base y quieres pasarlo al cliente

```bash
# Traer los cambios de main a la rama del cliente
git checkout cliente-nombre-empresa
git merge main
git push origin cliente-nombre-empresa
```

Railway hace el redeploy automático.

---

## Comandos de referencia rápida

```bash
# Ver todas las ramas
git branch -a

# Cambiar entre ramas
git checkout nombre-rama

# Crear nueva rama desde main
git checkout main && git checkout -b nueva-rama

# Subir rama nueva a GitHub
git push origin nueva-rama

# Traer cambios de main a la rama actual
git merge main
```
