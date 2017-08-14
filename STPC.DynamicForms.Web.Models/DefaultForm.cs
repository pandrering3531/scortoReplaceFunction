using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace STPC.DynamicForms.Web.Models
{
	public class DefaultForm
	{
		[Key]
		[DataMember]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Uid { get; set; }

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

		[Required]
		public virtual Form Form { get; set; }
	}
}