using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace STPC.DynamicForms.Web.Models
{
	[Serializable]
	public class FormPage
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[DataMember]
		public Guid Uid { get; set; }

		[Required]
		[DataMember]
		public int SortOrder { get; set; }

		[DataMember]
		public int DisplayOrder { get; set; }

		[DataMember]
		[MaxLength(64)]
		public string Name { get; set; }
		
		[MaxLength(512)]
		[DataMember]
		public string Description { get; set; }
		
		[DataMember]
		[Required]
		public virtual Form Form { get; set; }

		[DataMember]
		public virtual Guid? ReadOnlyState { get; set; }
		
		[DataMember]
		public virtual ICollection<Panel> Panels { get; set; }

		[DataMember]
		public virtual ICollection<PageEvent> PageEvents { get; set; }

		[DataMember]
		public List<FormPageActions> PageActions { set; get; }

		[DataMember]
		public List<FormPageByStates> FormPageByStates { set; get; }

		[DataMember]
		[MaxLength(50)]
		public string ShortPath { get; set; }

		[DataMember]
		[ScaffoldColumn(false)]
		public DateTime Timestamp { get; set; }

		[DataMember]
		public int StrategyID { get; set; }

		public FormPage()
		{
			this.Panels = new List<Panel>();
			this.PageActions = new List<FormPageActions>();
			this.Timestamp = DateTime.Now;
		}
	}
}