# Tlaoami Blueprint

## Visión general

Este proyecto es una API web .NET para un servicio llamado "Tlaoami". Sigue una arquitectura limpia con una separación de preocupaciones en tres proyectos: `Tlaoami.API` (presentación), `Tlaoami.Domain` (lógica de negocio) y `Tlaoami.Infrastructure` (acceso a datos).

## Estructura del proyecto

*   **`Tlaoami.sln`**: El archivo de la solución.
*   **`src/Tlaoami.API`**: La capa de presentación, un proyecto de API web de ASP.NET Core.
*   **`src/Tlaoami.Domain`**: La capa de dominio, una biblioteca de clases para entidades y modelos de negocio.
*   **`src/Tlaoami.Infrastructure`**: La capa de infraestructura, una biblioteca de clases para el acceso a datos mediante Entity Framework Core con SQLite.

## Estado actual

*   Se ha creado la estructura del proyecto.
*   Se han añadido las referencias del proyecto (`API` -> `Infrastructure` -> `Domain`).
*   Entity Framework Core, el proveedor de SQLite y las herramientas de diseño se han instalado en el proyecto `Infrastructure`.

## Pasos siguientes

1.  Definir un `DbContext` en el proyecto `Infrastructure`.
2.  Configurar el `DbContext` en el `Program.cs` del proyecto `API`.
3.  Crear una migración inicial.
4.  Aplicar la migración a la base de datos.
5.  Crear una entidad "Producto" en el proyecto `Domain`.
6.  Crear un `DbSet` para la entidad "Producto" en el `DbContext`.
7.  Crear una nueva migración para añadir la tabla `Productos`.
8.  Aplicar la nueva migración.
9.  Crear un controlador de API básico en el proyecto `API` para realizar operaciones CRUD en los productos.
