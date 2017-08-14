using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.Web.Models
{
	public class MenuItemRole
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }

		[MaxLength(255)]
		public string RoleName { get; set; }
		public Guid MenuItemUid { get; set; }

	}
}