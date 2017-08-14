using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace STPC.DynamicForms.Web.Models
{
	

	public class HierarchyNodeType
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[DataMember]
		[MaxLength(128)]
		public string NodeType { get; set; }

		[DataMember]
		[MaxLength(256)]
		public string TableName { get; set; }

		public virtual AplicationName AplicationName { get; set; }

		public int? AplicationNameId { get; set; }
		//public virtual NodeTypeDetail NodeTypeDetail { get; set; }
	}
}