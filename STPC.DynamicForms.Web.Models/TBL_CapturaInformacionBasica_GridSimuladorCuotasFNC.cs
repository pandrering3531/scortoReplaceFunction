using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace STPC.DynamicForms.Web.Models
{
    public class TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC
    {
        [Required]
        [DataMember]
        public int F_RequestId {get; set;}

        [Required]
        [DataMember]
        public long F_monValorObligacionFnc { get; set; }
        
        [Required]
        [DataMember]
        public long F_monValorCuotaFnc { get; set; }

        [Required]
        [DataMember]
        [MaxLength(50)]
        public string F_varEntidadFnc { get; set; }

        [Required]
        [DataMember]
        [MaxLength(10)]
        public string F_varNumeroObligacionFnc {get; set;}

        [Key]
        [Required]
        [DataMember]
        public int F_Id { get; set; }

        [DataMember]
        public bool F_bitEliminado { get; set; }

        [DataMember]
        public bool F_bitSeleccionado { get; set; }

        [Required]
        [DataMember]
        [Display(Name = "Saldo")]
        public long F_monSaldo { get; set; }

        [Required]
        [DataMember]
        [Display(Name = "Plazo")]
        public string F_varPlazo { get; set; }

        [Required]
        [DataMember]
        [Display(Name = "Forma de Pago")]
        public string F_varFormapago { get; set; }

        [Required]
        [DataMember]
        [Display(Name = "Proximo Pago")]
        public string F_varProximoPago { get; set; }

        [Required]
        [DataMember]
        [Display(Name = "Tipo")]
        public string F_varTipo { get; set; }

        [Required]
        [DataMember]
        [Display(Name = "Producto")]
        public string F_varProducto { get; set; }

        public TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC()
        {

        }
    }
}
