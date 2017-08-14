using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace STPC.DynamicForms.Web.Models
{
	public class UserResetQuestion
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }

		[DataMember]
		public virtual User User { get; set; }

		[DataMember]
		public virtual ResetQuestion Question { get; set; }

		[DataMember]
		[MaxLength(512)]
		public string Answer { get; set; }

	}
}