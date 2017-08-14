using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace STPC.DynamicForms.Web.Models
{
	[DataContract()]
	public class Request
	{
		[DataMember]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int32 RequestId { get; set; }

		[DataMember]
		public DateTime Created { get; set; }

		[DataMember]
		public DateTime Updated { get; set; }

		[DataMember]
		public Guid FormId { get; set; }


		[DataMember]
		public Guid? WorkFlowState { get; set; }

		[MaxLength(255)]
		[DataMember]
		public string PageFlowState { get; set; }

		[DataMember]
		public Guid PageFlowId { get; set; }

		[MaxLength(255)]
		[DataMember]
		public string CreatedBy { get; set; }

		[MaxLength(255)]
		[DataMember]
		public string UpdatedBy { get; set; }

		[MaxLength(255)]
		[DataMember]
		public string AssignedTo { get; set; }

		//public virtual AplicationName AplicationName { get; set; }

		[DataMember]
		public int? AplicationNameId { get; set; }


	}
}