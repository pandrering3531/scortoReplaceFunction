using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace STPC.DynamicForms.Web.Models
{
	//[MetadataType(typeof(FormMetaData))]
	public class AplicationName
	{

		[ScaffoldColumn(false)]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
	

		[MaxLength(64)]
		public string Name { get; set; }

		//public virtual ICollection<Category> Category { get; set; }
		[DataMember]
		public virtual ICollection<Category> Category { get; set; }

		public virtual ICollection<AdCampaign> AdCampaign { get; set; }

		public virtual ICollection<Hierarchy> Hierarchy { get; set; }

		public virtual ICollection<PerformanceIndicator> PerformanceIndicator { get; set; }

		public virtual ICollection<Request> Request{ get; set; }

		public virtual ICollection<Report> Report { get; set; }

		public virtual ICollection<MenuItem> MenuItem { get; set; }

		public virtual ICollection<Option> Option { get; set; }
		public AplicationName()
		{
			this.Category = new List<Category>();
			
			this.AdCampaign = new List<AdCampaign>();
			this.Hierarchy = new List<Hierarchy>();
			this.PerformanceIndicator = new List<PerformanceIndicator>();
			this.Request = new List<Request>();
			this.Report = new List<Report>();
			this.MenuItem = new List<MenuItem>();

		}

	}
}