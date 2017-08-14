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
  [Table("StrategySettings")]
  [DataContract(IsReference = true)]
	public class StrategySettings
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Uid { get; set; }

    [DataMember]
	 [Display(Name = "StrategyID")]
	 public int StrategyID { get; set; }

	

	 [DataMember]
	 [Display(Name = "AttributeName")]
	 public string AttributeName { get; set; }

    [DataMember]
	 [Display(Name = "Value")]
    public string Value { get; set; }

	 [DataMember]
	 [Display(Name = "ValueType")]
	 public string ValueType { get; set; }
  }
}