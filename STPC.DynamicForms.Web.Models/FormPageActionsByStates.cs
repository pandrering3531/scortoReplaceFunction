using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace STPC.DynamicForms.Web.Models
{
	public class FormPageActionsByStates
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }

		[DataMember]
		public virtual FormPageActions FormPageActions { get; set; }

		[DataMember]
		public Guid FormPageActionsUid { get; set; }

		[DataMember]
		public Guid FormStatesUid { get; set; }

		public FormStates FormStates { get; set; }

	}
}