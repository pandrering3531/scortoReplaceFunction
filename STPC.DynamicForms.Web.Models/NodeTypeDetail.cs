using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STPC.DynamicForms.Web.Models
{
	public class NodeTypeDetail
	{
		[Key]
		public Guid NodeTypeDetailId { get; set; }

		[Required]
		public virtual HierarchyNodeType HierarchyNodeType { get; set; }
		[Required]
		public Guid PageFieldTypeId { get; set; }
		[ForeignKey("PageFieldTypeId")]
		public virtual PageFieldType FieldType { get; set; }

		[MaxLength(30)]
		public string FieldName { get; set; }

		[MaxLength(1500)]
		public string FieldPrompt { get; set; }

		public bool IsRequired { get; set; }
		[MaxLength(128)]
		public string OptionsCategoryName { get; set; }
		[MaxLength(16)]
		public string MaxSize { get; set; }
		[MaxLength(16)]
		public string MinSize { get; set; }
		public int SortOrder { get; set; }
		[MaxLength(64)]
		public string Style { get; set; }

		//public virtual HierarchyNodeType HierarchyNodeType { get; set; }
	}
}