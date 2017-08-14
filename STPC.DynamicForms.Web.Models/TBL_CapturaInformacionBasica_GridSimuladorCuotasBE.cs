using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace STPC.DynamicForms.Web.Models
{
    public class TBL_CapturaInformacionBasica_GridSimuladorCuotasBE
    {
        [Required]
        [DataMember]
        public int F_RequestId {get; set;}

        [Required]
        [DataMember]
        [Display(Name = "Valor Obligacion")]
        public long F_monValorObligacionBe { get; set; }
        
        [Required]
        [DataMember]
        [Display(Name = "Valor Cuota")]
        public long F_monValorCuotaBe { get; set; }

        [Required(ErrorMessage = "El campo 'Entidad' es requerido")]
        [DataMember]
        [MaxLength(50)]
        [Display(Name = "Entidad")]
        public string F_varEntidadBE { get; set; }

        [Required(ErrorMessage = "El campo 'Numero Obligacion' es requerido")]
        [DataMember]
        [MaxLength(10)]
        [Display(Name = "Numero Obligacion")]
        public string F_varNumeroObligacionBe { get; set; }

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

        public TBL_CapturaInformacionBasica_GridSimuladorCuotasBE()
        {

        }
    }
}
