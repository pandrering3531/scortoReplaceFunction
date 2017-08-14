using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STPC.DynamicForms.Web.Models
{
	public class PasswordHistory
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }

		[Required]
		[MaxLength(128)]
		public string LastModifiedBy { get; set; }

		public DateTime LastModified { get; set; }

		[Required]
		[MaxLength(128)]
		public string Password { get; set; }
	}
}