using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace STPC.DynamicForms.Web.Models
{
	[DataContract()]
	public class Report
	{
		[DataMember]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int32 Id { get; set; }

		[MaxLength(50)]
		[DataMember]
		public string Name { get; set; }

		[MaxLength(255)]
		[DataMember]
		public string ReportPath { get; set; }

		[MaxLength(255)]
		[DataMember]
		public string Parameters { get; set; }

		[DataMember]
		public Boolean IsDefaultView { get; set; }

		[Required]
		public virtual Form Form { get; set; }

		public virtual AplicationName AplicationName { get; set; }

		public int? AplicationNameId { get; set; }

	}
}