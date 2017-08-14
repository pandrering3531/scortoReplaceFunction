using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace STPC.DynamicForms.Web.RT.Models
{
    public class DetailRequestModel
    {
        public string Solicitante { get; set; }
        public string Identificacion { get; set; }
        public string Radicado { get; set; }
        public string Valor { get; set; }
        public string Plazo { get; set; }
        public string EstadoActualSolicitud { get; set; }
        public List<RegistroEventos> LogEventos { get; set; }
        public string ObservacionAnalista { get; set; }
        public string ObservacionCooordinador { get; set; }
        public string ObservacionComite { get; set; }
        public string ObservacionVisacion { get; set; }
        public string ObservacionCapacidadPago { get; set; }

    }

    public class RegistroEventos
    {
        public string Radicado { get; set; }
        public string Area { get; set; }
        public string Actividad { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public string Usuario { get; set; }

    }

    public class GuardarGestionView
    {
        [Required]
        public string Radicado { get; set; }
        public string EstadoNuevo { get; set; }
        public string EstadoActual { get; set; }
        [Required]
        public string Causal { get; set; }
        [Required]
        public string Observaciones { get; set; }
        public string Usuario { get; set; }

    }

}