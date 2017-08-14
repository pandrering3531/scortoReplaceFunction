using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace STPC.DynamicForms.Web.Models
{
	public class ResetQuestion
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[DataMember]
		[MaxLength(512)]
		public string Question { get; set; }

    [DataMember]
    public bool IsActive { get; set; }
	}
}