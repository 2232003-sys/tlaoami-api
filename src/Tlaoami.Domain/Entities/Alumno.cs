
using System;

namespace Tlaoami.Domain.Entities
{
    public class Alumno
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public DateTime FechaNacimiento { get; set; }
    }
}
