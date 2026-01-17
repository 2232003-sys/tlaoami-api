# Blueprint de la Aplicación Tlaoami

## Visión General

Tlaoami es una aplicación web de pila completa creada con .NET que sirve como un sistema de gestión de estudiantes. La aplicación permite a los usuarios ver una lista de estudiantes, buscar un estudiante específico por su ID y ver los detalles del estudiante, incluyendo sus facturas y pagos.

## Pila Tecnológica

*   **Backend:** ASP.NET Core Web API
*   **Frontend:** HTML simple con JavaScript para la interactividad del lado del cliente
*   **Base de datos:** SQLite
*   **ORM:** Entity Framework Core

## Estructura del Proyecto

El proyecto sigue una arquitectura limpia con la siguiente estructura:

*   **Tlaoami.API:** El proyecto de API de ASP.NET Core que contiene los controladores y el punto de entrada de la aplicación.
*   **Tlaoami.Application:** La capa de aplicación que contiene la lógica de negocio, los servicios y los Data Transfer Objects (DTOs).
*   **Tlaoami.Domain:** La capa de dominio que contiene las entidades principales y las interfaces de repositorio.
*   **Tlaoami.Infrastructure:** La capa de infraestructura que gestiona el acceso a los datos con Entity Framework Core.

## Lógica de Pagos

1.  **DTO de Creación de Pagos:** Se creó un `PagoCreateDto` para estandarizar la entrada de datos al registrar un nuevo pago. Contiene `FacturaId`, `Monto` y `FechaPago`.
2.  **Servicio de Pagos:** Se implementó el `PagoService` con un método `RegistrarPagoAsync`. Este servicio se encarga de:
    *   Crear una nueva entidad `Pago`.
    *   Asociar el pago a la factura correspondiente.
    *   Actualizar el estado de la factura a "Pagada" si el monto total ha sido cubierto.
3.  **Endpoint de la API:** Se expuso un único endpoint `POST /api/pagos` en `PagosController` para registrar nuevos pagos, manteniendo el controlador delgado y delegando la lógica de negocio al servicio de pagos.
4.  **Estado de Cuenta Automático:** El estado de cuenta del alumno, accesible a través de la API, se actualiza automáticamente después de registrar un pago, ya que la lógica de cálculo del estado de cuenta se basa en los datos actualizados de facturas y pagos.

## Flujo de Solicitud Actual

1.  **DTO de Estado de Cuenta:** Se creó un `EstadoCuentaDto` para encapsular los detalles del estado de cuenta del alumno, incluyendo el total facturado, el total pagado, el saldo pendiente y las listas de facturas pagadas y pendientes.
2.  **Lógica de Negocio del Servicio:** La lógica para calcular el estado de cuenta se implementó en `AlumnoService`, asegurando que el controlador permanezca delgado.
3.  **Endpoint de la API:** Se expuso un nuevo endpoint `GET /api/v1/alumnos/{id}/estado-cuenta` en `AlumnosController` para devolver el `EstadoCuentaDto`.
4.  **Versionado de la API:** Las rutas de todos los controladores (`AlumnosController`, `FacturasController`, `PagosController`) se actualizaron a `api/v1` para mantener la consistencia en el versionado de la API.
