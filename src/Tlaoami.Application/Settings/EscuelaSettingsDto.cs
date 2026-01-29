using System;

namespace Tlaoami.Application.Settings
{
    public class EscuelaSettingsDto
    {
        public Guid EscuelaId { get; set; }

        // ===== INFORMACIÓN INSTITUCIONAL =====
        public string Nombre { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string TextoRecibos { get; set; } = string.Empty;

        // ===== CONFIGURACIÓN OPERATIVA =====
        public string Moneda { get; set; } = "MXN";
        public string ZonaHoraria { get; set; } = "America/Mexico_City";
        public int DiaCorteColegiatura { get; set; } = 10;
        public bool BloquearReinscripcionConSaldo { get; set; } = false;

        // ===== AUDITORÍA =====
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
