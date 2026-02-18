# ClaseControl - Guía de configuración

## Cambios realizados

La aplicación se adaptó de "Estudiantes" a **ClaseControl**, una app para gestión de clases particulares (Inglés e Informática) orientada a Uruguay.

### Backend (.NET)

- **Entidades nuevas**: `Alumno` y `Clase`
- **Controllers**: `AlumnosController`, `ClasesController`, `DashboardController`
- **Migración**: `AddAlumnoAndClase` (crea tablas Alumnos y Clases)

### Frontend (Angular)

- **Layout**: Sidebar fija + header minimalista
- **Páginas**:
  - Dashboard: métricas (clases hoy, este mes, ingresos $, alumnos activos), clases de hoy, próximas clases
  - Alumnos: cards con búsqueda y filtros (Todos/Activos/Inactivos), última clase, clases totales
  - Perfil alumno: tabs Información, Clases, Notas
  - Agenda: Hoy / Próximas / Pasadas, filtros por estado, resumen del día/semana/mes

### Diseño

- Sidebar azul oscuro (#1e293b)
- Cards con sombra suave y bordes redondeados
- Estados: verde (realizada), naranja (programada), rojo (cancelada)
- Moneda: pesos uruguayos ($)

## Pasos para ejecutar

### 1. Aplicar migración en el backend

```bash
cd Tracklass.API/Tracklass.API
dotnet ef database update
```

### 2. Ejecutar backend

```bash
dotnet run
```

(Backend en https://localhost:7294)

### 3. Ejecutar frontend

```bash
cd estudiantes-frontend
npm install
ng serve
```

(Frontend en http://localhost:4200)

## Materias soportadas

Solo **Inglés** e **Informática**.

## Configuración

- **Precio por clase**: Por defecto $300. Para cambiarlo, editar `appsettings.json`:
  ```json
  "ClaseControl": {"PrecioPorClase": 300}
  ```
- **Email**: Opcional. Puede dejarse vacío para alumnos que no usan correo.

## Rutas

- `/dashboard` - Dashboard principal
- `/alumnos` - Lista de alumnos en cards
- `/alumnos/:id` - Perfil del alumno (tabs: Información, Clases, Notas)
- `/agenda` - Agenda de clases (Hoy / Próximas / Pasadas)
