using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

[assembly: ContractNamespace("http://STPC.LiSim.Abc", ClrNamespace = "STPC.DynamicForms.Web.Models")]
namespace STPC.DynamicForms.Web.Models
{

	[Table("Hierarchy")]
	[DataContract(IsReference = true)]
	public class Hierarchy
	{
		[DataMember]
		public int Id { get; set; }
		[DataMember]
		[MaxLength(256)]
		public string Name { get; set; }
		[DataMember]
		[MaxLength(128)]
		public string Level { get; set; }

		[DataMember(IsRequired = false)]
		public Hierarchy Parent { get; set; }


		[DataMember]
		public ICollection<Hierarchy> Children { get; set; }

		[DataMember]
		public int NodeType { get; set; }

		public bool IsActive { get; set; }

		public virtual AplicationName AplicationName { get; set; }

		public int? AplicationNameId { get; set; }

		public Hierarchy()
		{
			Children = new HashSet<Hierarchy>();
		}
	}
}