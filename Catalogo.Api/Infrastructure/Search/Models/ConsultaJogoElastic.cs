namespace Catalogo.Api.Infrastructure.Search.Models
{
    public class ConsultaJogoElastic
    {
        public Guid JogoId { get; set; }
        public string TermoBuscado { get; set; }
        public DateTime DataConsulta { get; set; }
    }
}
