using System;

namespace Tlaoami.Domain.Enums
{
    /// <summary>
    /// Estado de una orden de venta.
    /// </summary>
    public enum EstatusOrdenVenta
    {
        /// <summary>Orden en construcción, no genera cargo</summary>
        Borrador = 0,
        
        /// <summary>Orden confirmada, genera cargo financiero</summary>
        Confirmada = 1,
        
        /// <summary>Orden cancelada</summary>
        Cancelada = 2
    }
}
