using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STPC.DynamicForms.Web.Models
{
	public class PageMathOperation
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }

		[MaxLength(1050)]
		public string Expression { get; set; }

		[MaxLength(256)]
		public string Trigger{ get; set; }

		public Guid ResultField { get; set; }

		public virtual Panel Panel { get; set; }

		public Guid PanelUid { get; set; }

		
	}
}
