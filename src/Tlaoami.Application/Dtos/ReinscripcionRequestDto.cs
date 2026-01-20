using System;

namespace Tlaoami.Application.Dtos
{
    public class ReinscripcionRequestDto
    {
        public Guid AlumnoId { get; set; }
        public Guid GrupoId { get; set; }
    }
}
