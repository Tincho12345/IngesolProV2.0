using System;
using System.ComponentModel.DataAnnotations;
using WebIngesol.Models.Viaticos;

namespace WebIngesol.Models.Viatico
{
    public class ValorViatico : AuditableEntity
    {
        [Required]
        public DateTime FechaDesde { get; set; }

        [Required]
        public decimal Valor { get; set; }

        [Required]
        public Guid TipoViaticoId { get; set; }

        public TipoViatico TipoViatico { get; set; } = null!;

        public string TipoViaticoNombre { get; set; } = string.Empty;

        public string? TipoViaticoObservaciones { get; set; } // 👈 NUEVO
    }

    public class ValorViaticoDto : AuditableEntity
    {
        [Required]
        public DateTime FechaDesde { get; set; }

        [Required]
        public decimal Valor { get; set; }

        [Required]
        public Guid TipoViaticoId { get; set; }

        public string TipoViaticoNombre { get; private set; } = string.Empty;

        public string? TipoViaticoObservaciones { get; private set; } // 👈 NUEVO
    }
}
