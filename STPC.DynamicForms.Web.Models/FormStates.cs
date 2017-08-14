using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STPC.DynamicForms.Web.Models
{
	public class FormStates
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }

		[MaxLength(128)]
		public string StateName { get; set; }

		[MaxLength(128)]
		public string StateSymbol { get; set; }

		public List<FormPageActions> FormPageActionsList { set; get; }

		public List<FormPageActionsByStates> FormPageActionsByStatesList { set; get; }

		public List<FormPageByStates> FormPageByStates { set; get; }

		public FormStates()
		{
			FormPageActionsList = new List<FormPageActions>();
		}

	}
}