namespace Tlaoami.Application.Dtos;

public class KpiConciliacionDto
{
    public int TotalPagosReportados { get; set; }
    public int PagosAutoConciliados { get; set; }
    public int PagosConciliadosManualmente { get; set; }
    public int PagosPendientes { get; set; }
    public decimal TasaAutoConciliacion { get; set; }
    public decimal MontoTotalConciliado { get; set; }
    public decimal MontoAutoConciliado { get; set; }
    public decimal MontoManualConciliado { get; set; }
}
