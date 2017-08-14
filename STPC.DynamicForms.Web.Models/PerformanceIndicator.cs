using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace STPC.DynamicForms.Web.Models
{
	public class PerformanceIndicator
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[DataMember]
		public int IndicatorType { get; set; }

		[DataMember]
		[MaxLength(128)]
		public string Source { get; set; }

		[DataMember]
		public bool Enabled { get; set; }

		[DataMember]
		public Role Role { get; set; }

		[DataMember]
		public Hierarchy Hierarchy { get; set; }

		[DataMember]
		public User User { get; set; }

		[DataMember]
		public Guid LastModifiedBy { get; set; }

		[DataMember]
		public DateTime? Modified { get; set; }

		[DataMember]
		public int WarningMinValue { get; set; }

		[DataMember]
		public int WarningMaxValue { get; set; }

		[DataMember]
		public int ViolationMinvalue { get; set; }

		[DataMember]
		public int ViolationMaxvalue { get; set; }

		public virtual AplicationName AplicationName { get; set; }

		public int? AplicationNameId { get; set; }

	}
}