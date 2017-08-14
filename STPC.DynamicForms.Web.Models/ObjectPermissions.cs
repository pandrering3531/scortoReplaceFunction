using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace STPC.DynamicForms.Web.Models
{

	
	public class ObjectPermissions
	{
		[ScaffoldColumn(false)]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }


		[DataMember]
		[MaxLength(256)]
		public string TableName { get; set; }


		[DataMember]
		[MaxLength(256)]
		public string ObjectName { get; set; }

		[DataMember]
		[MaxLength(10)]
		public string Permission { get; set; }

		[DataMember]
		public Role Role { get; set; }
		
	}
}