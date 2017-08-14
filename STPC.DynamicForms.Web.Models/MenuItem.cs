using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace STPC.DynamicForms.Web.Models
{
	public class MenuItem
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }
		[MaxLength(64)]
		public string Controller { get; set; }
		[MaxLength(64)]
		public string Action { get; set; }
		[MaxLength(128)]
		public string Message { get; set; }
		public int DisplayOrder { get; set; }
		public Guid? ParentMenuItemUid { get; set; }
		public Guid? FormUid { get; set; }
		public Guid? FormState { get; set; }
		[MaxLength(1024)]
		public string Parameters { get; set; }
		public List<MenuItem> Childs { get; set; }		

		public MenuItem() { }

		public virtual AplicationName AplicationName { get; set; }

		public int? AplicationNameId { get; set; }
	}
}