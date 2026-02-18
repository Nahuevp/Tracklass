# Deploy: Render (backend) + Vercel (frontend)

## ¿Cómo funciona la persistencia de datos?

1. **Backend (Render)**: Ejecuta tu API .NET. Los datos se guardan en una **base de datos**.
2. **Frontend (Vercel)**: Solo sirve archivos estáticos (HTML, JS, CSS). No guarda datos.
3. **Flujo**: Usuario → Frontend (Vercel) → llama a API → Backend (Render) → Base de datos.

**Sí, los datos persisten** mientras la base de datos esté activa. El backend es quien escribe y lee en la DB. Render puede apagar el backend si no hay tráfico (plan free), pero al recibir una request lo vuelve a levantar; la DB sigue guardando todo.

---

## Base de datos: opciones para Render

Render **no ofrece SQL Server gratis**. Tenés que usar una de estas opciones:

### Opción A: PostgreSQL en Render (gratis)
- Crear un **PostgreSQL** en Render (plan Free).
- Modificar el backend para usar PostgreSQL en vez de SQL Server (cambio de proveedor EF Core).
- Connection string te lo da Render al crear el recurso.

### Opción B: Base de datos externa
- **Supabase** (PostgreSQL gratis)
- **Railway** (PostgreSQL gratis con límites)
- **ElephantSQL** (PostgreSQL gratis)

Conectás el backend a esa DB con el connection string.

### Opción C: SQL Server en la nube (de pago)
- Azure SQL, AWS RDS, etc. Para un proyecto personal suele ser excesivo.

---

## Pasos para deployar

### 1. Backend en Render

1. Subí el código a GitHub.
2. En Render: **New → Web Service**.
3. Conectá el repo.
4. Configuración:
   - **Build Command**: `dotnet publish -c Release -o out`
   - **Start Command**: `dotnet out/Tracklass.API.dll`
   - **Root Directory**: `Tracklass.API` (ruta al proyecto .NET)

5. **Variables de entorno**:
   - `ConnectionStrings__SqlServer` = tu connection string (si usás SQL Server)
   - O `ConnectionStrings__DefaultConnection` (si migrás a PostgreSQL)
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `ASPNETCORE_URLS` = `http://0.0.0.0:$PORT` (Render define `PORT`)

6. Render te da una URL tipo: `https://tu-api.onrender.com`

---

### 2. Frontend en Vercel

1. **Variables de entorno**:
   - `API_URL` o similar con la URL del backend (ej: `https://tu-api.onrender.com`)

2. Configurar el Angular para usar esa URL:
   - En `environment.production.ts` o un archivo de config:
     ```ts
     export const environment = {
       production: true,
       apiUrl: 'https://tu-api.onrender.com'
     };
     ```
   - En los services, usar `environment.apiUrl` en lugar de `https://localhost:7294`.

3. Vercel: **Import Project** → elegir el repo.
4. **Root Directory**: `estudiantes-frontend`
5. **Build Command**: `npm run build`
6. **Output Directory**: `dist/estudiantes-frontend/browser` (o el que genere `ng build`)

---

### 3. CORS en el backend

En `Program.cs`, la política CORS debe permitir el dominio de Vercel:

```csharp
builder.Services.AddCors(op =>
{
    op.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();  // O especificar: .WithOrigins("https://tu-app.vercel.app")
    });
});
```

`AllowAnyOrigin()` sirve para desarrollo; para producción conviene restringir al dominio de Vercel.

---

## Resumen

| Componente | Dónde | Persistencia |
|------------|-------|--------------|
| Frontend   | Vercel | No guarda datos |
| Backend    | Render | Procesa requests y escribe en DB |
| Base de datos | Render / Supabase / Railway | **Aquí persisten todos los datos** |

Los datos no se pierden al apagar el navegador ni al redeployar. Solo se perderían si se borrara la base de datos o el servicio donde está hosteada.
