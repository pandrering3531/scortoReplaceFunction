using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace STPC.DynamicForms.Web.Models
{
    public class TBL_Desembolso_FINCOMERCIOGrid
    {
        [Required]
        [DataMember]
        public int F_RequestId {get; set;}

        [DataMember]
        [MaxLength(50)]
        [Display(Name = "Tipo desembolso")]
        public string F_varTipoDesembolso { get; set; }

        [Required(ErrorMessage = "El campo 'Banco' es requerido")]
        [DataMember]
        [MaxLength(10)]
        [Display(Name = "Banco")]
        public string F_varBanco { get; set; }

        [Required(ErrorMessage = "El campo 'Número Cuenta' es requerido")]
        [DataMember]
        [MaxLength(10)]
        [Display(Name = "Número Cuenta")]
        public string F_varNumeroCuenta { get; set; }

        [Required(ErrorMessage = "El campo 'Tipo Cuenta' es requerido")]
        [DataMember]
        [MaxLength(10)]
        [Display(Name = "Tipo Cuenta")]
        public string F_varTipoCuenta { get; set; }

        [DataMember]
        public bool F_bitEliminado { get; set; }

        [DataMember]
        public bool F_bitSeleccionado { get; set; }

        [DataMember]
        [MaxLength(10)]
        [Display(Name = "Id Banco")]
        public string F_varIdBanco { get; set; }

        [Key]
        [Required]
        [DataMember]
        public int F_Id { get; set; }

        public TBL_Desembolso_FINCOMERCIOGrid()
        {

        }
    }
}
