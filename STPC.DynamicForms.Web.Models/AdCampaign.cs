using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

[assembly: ContractNamespace("http://STPC.LiSim.Abc", ClrNamespace = "STPC.DynamicForms.Web.Models")]
namespace STPC.DynamicForms.Web.Models
{
  [Table("AdCampaign")]
  [DataContract(IsReference = true)]
  public class AdCampaign
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Uid { get; set; }

    [DataMember]
	 [Display(Name = "Nombre campaña")]
    public string Text { get; set; }

    [DataMember]
	 [Display(Name = "Imagen")]
    public string Image { get; set; }


    [ForeignKey("Hierarchy_id")]
    public virtual Hierarchy Hierarchy { get; set; }


    [DataMember]
    public int Hierarchy_id { get; set; }

    [DataMember]
	 [Display(Name = "Fecha inicio")]
    public DateTime BeginDate { get; set; }

    [DataMember]
	 [Display(Name = "Fecha fin")]
    public DateTime EndDate { get; set; }

    [DataMember]
    public string Url { get; set; }

    [DataMember]
	 [Display(Name = "Aplica a los hijos")]
    public bool ApplyToChilds { get; set; }

    public List<Hierarchy> HierarchyByCampaign { get; set; }


	 public virtual AplicationName AplicationName { get; set; }

	 public int? AplicationNameId { get; set; }

  }
}