namespace Tlaoami.Domain.Entities;

public class MovimientoConciliacion
{
    public Guid Id { get; set; }
    
    public Guid MovimientoBancarioId { get; set; }
    public MovimientoBancario? MovimientoBancario { get; set; }
    
    public Guid? AlumnoId { get; set; }
    public Alumno? Alumno { get; set; }
    
    public Guid? FacturaId { get; set; }
    public Factura? Factura { get; set; }
    
    public DateTime FechaConciliacion { get; set; }
    
    public string? Comentario { get; set; }
}
