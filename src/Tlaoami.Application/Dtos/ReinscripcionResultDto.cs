using System;

namespace Tlaoami.Application.Dtos
{
    public class ReinscripcionResultDto
    {
        public Guid AlumnoId { get; set; }
        public Guid GrupoId { get; set; }
        public Guid CicloId { get; set; }
        public string Status { get; set; } = "REINSCRITO";
    }
}
