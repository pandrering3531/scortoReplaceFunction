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
	[Table("Options")]
	[DataContract(IsReference = true)]
	public class Option
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[DataMember]
		public int Uid { get; set; }


		[DataMember]
		[MaxLength(256)]
		public string Key { get; set; }

		[DataMember]
		[MaxLength(256)]
		public string Value { get; set; }

		[DataMember]
		public int Category_Uid { get; set; }

		public int Option_Uid_Parent { get; set; }

		[ForeignKey("Category_Uid")]
		[DataMember]
		public virtual Category Category { get; set; }

		[DataMember]
		public bool IsActive { get; set; }

		public virtual AplicationName AplicationName { get; set; }

		[DataMember]
		public int? AplicationNameId { get; set; }

		

	}
}
