using System;

namespace Tlaoami.Application.Settings
{
    public class EscuelaSettingsDto
    {
        public Guid EscuelaId { get; set; }
        public int DiaCorteColegiatura { get; set; }
        public bool BloquearReinscripcionConSaldo { get; set; }
        public string ZonaHoraria { get; set; } = string.Empty;
        public string Moneda { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
