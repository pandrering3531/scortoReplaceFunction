using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace STPC.DynamicForms.Web.Models
{
    public class TBL_CapturaInformacionBasicaAnalista_GridGrafologia
    {
        [Required]
        [DataMember]
        public int F_RequestId {get; set;}

        [Required]
        [DataMember]
        public int F_NroRegistro { get; set; }

        [Required(ErrorMessage = "El campo 'Nombre Soporte' es requerido")]
        [DataMember]
        [MaxLength(50)]
        [Display(Name = "Nombre Soporte")]
        public string F_varNombreSoporte { get; set; }

        [Required(ErrorMessage = "El campo 'Soporte' es requerido")]
        [DataMember]
        [Display(Name = "Soporte")]
        public string F_varSoporte { get; set; }

        [Required(ErrorMessage = "El campo 'Resultado' es requerido")]
        [DataMember]
        [MaxLength(50)]
        [Display(Name = "Resultado")]
        public string F_varResultado { get; set; }

        [Required(ErrorMessage = "El campo 'Observacion' es requerido")]
        [DataMember]
        [Display(Name = "Observación")]
        public string F_varObservacion { get; set; }

        [Key]
        [Required]
        [DataMember]
        public int F_Id { get; set; }

        [DataMember]
        public bool F_bitEliminado { get; set; }

        public TBL_CapturaInformacionBasicaAnalista_GridGrafologia()
        {

        }
    }
}
