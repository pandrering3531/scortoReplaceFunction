using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STPC.DynamicForms.Web.Models
{
	public class PageEvent
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }
		[MaxLength(256)]
		public string FieldValue { get; set; }
		[MaxLength(30)]
		public string ListenerField { get; set; }

		public virtual FormPage FormPage { get; set; }



		//public virtual PageField PageField { get; set; }

		//public virtual PageField PageFieldListener { get; set; }

		public Guid FormPageUid { get; set; }

		[Required(ErrorMessage = "*")]
		public Guid ListenerFieldId { get; set; }

		[Required(ErrorMessage = "*")]
		public Guid PageFieldUid { get; set; }
		[MaxLength(30)]
		public string SourceField { get; set; }
		[MaxLength(30)]
		public string EventType { get; set; }

	}
}