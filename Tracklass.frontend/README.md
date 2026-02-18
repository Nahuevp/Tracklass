# estudiantes-frontend

Frontend de la aplicación Estudiantes App, desarrollado en Angular 18 y Angular Material. Se conecta a la API `Estudiantes-API` para gestionar un listado de estudiantes con operaciones CRUD.

## Tecnologías utilizadas

- Angular 18
- Angular Material
- TypeScript
- HTML / CSS

##  Objetivo

Diseñar una interfaz moderna y funcional que consuma el backend de ASP.NET Core y permita interactuar con la lista de estudiantes.

##  Funcionalidades

- Listar estudiantes
- Ver detalles de un estudiante
- Agregar estudiante
- Editar estudiante existente
- Eliminar estudiante
- Spinner de carga y notificaciones con Toastr

##  Estructura principal

- `/components`: componentes reutilizables como Spinner, Navbar, ConfirmDialog y Card del estudiante.
- `/interfaces`: definición de las interfaces de los objetos que se manejan en la app (estudiantes, respuestas del backend).
- `/pages`: páginas principales: Home, Listado, Detalle, Upsert (crear/editar).
- `/services`: servicios para la conexión HTTP con la API (usando `HttpClient` y `RxJS`).
