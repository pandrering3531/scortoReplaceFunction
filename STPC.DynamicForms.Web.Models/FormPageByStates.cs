using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace STPC.DynamicForms.Web.Models
{
	public class FormPageByStates
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }

		public Guid FormStatesUid { get; set; }

		public Guid FormPageUid { get; set; }

		[Required]
		public virtual FormStates FormStates { get; set; }

		public virtual FormPage FormPage { get; set; }
		

	}
}