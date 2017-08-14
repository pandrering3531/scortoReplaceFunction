using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STPC.DynamicForms.Web.Models
{
	public class ForbiddenPassword
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Uid { get; set; }

		[MaxLength(255)]
		public string LastModifiedBy { get; set; }

		public DateTime LastModified { get; set; }

		[MaxLength(255)]
		public string ForbiddenText { get; set; }
	}
}