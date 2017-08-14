using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STPC.DynamicForms.Web.Models
{
	public class AuthenticationAudit
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }

		[Required]
		[MaxLength(255)]
		public string UserName { get; set; }

		public DateTime EventTime { get; set; }

		[Required]
		[MaxLength(256)]
		public string WorkStation { get; set; }
	}
}