namespace Tlaoami.Application.Contracts;

public class ImportacionResultadoDto
{
    public int MovimientosImportados { get; set; }
    public int Depositos { get; set; }
    public int Retiros { get; set; }
    public int Omitidos { get; set; }
}
