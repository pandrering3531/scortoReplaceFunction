using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace STPC.DynamicForms.Web.Models
{
	public class Role
	{
		[Key]
		[Column(Order = 0)]
		[MaxLength(255)]
		[DataMember]
		public string Rolename { get; set; }

		public int? AplicationNameId { get; set; }

		public virtual AplicationName AplicationName { get; set; }

		[DataMember]
		public virtual ICollection<User> UsersInRole { get; set; }

		[DataMember]
		public virtual ICollection<PageField> ViewFields { get; set; }

		[DataMember]
		public virtual ICollection<PageField> EditFields { get; set; }

		[DataMember]
		public virtual ICollection<PerformanceIndicator> PerformanceIndicator { get; set; }

		[DataMember]
		public virtual ICollection<ObjectPermissions> Roles { get; set; }
		
	}
}
