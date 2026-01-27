using System;

namespace Tlaoami.Domain.Enums
{
    /// <summary>
    /// Tipo de origen de una factura/cargo.
    /// Permite rastrear si proviene de colegiatura, orden de venta, etc.
    /// </summary>
    public enum OrigenFactura
    {
        /// <summary>Colegiatura generada automáticamente por reglas</summary>
        Colegiatura = 0,
        
        /// <summary>Orden de venta confirmada (productos/servicios)</summary>
        OrdenVenta = 1,
        
        /// <summary>Cargo manual creado por administrador</summary>
        Manual = 2,
        
        /// <summary>Recargo por mora</summary>
        Recargo = 3,
        
        /// <summary>Otros cargos no categorizados</summary>
        Otro = 99
    }
}
