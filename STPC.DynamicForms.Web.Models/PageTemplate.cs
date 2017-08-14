using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;
namespace STPC.DynamicForms.Web.Models
{
	public class PageTemplate
	{
		[Key]
		public Guid Uid { get; set; }
		[MaxLength(256)]
		public string Name { get; set; }
		[MaxLength(512)]
		public string Description { get; set; }
		public virtual ICollection<PanelTemplate> PanelTemplates { get; set; }

		[ScaffoldColumn(false)]
		public DateTime Timestamp { get; set; }
	}
}