using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

[assembly: ContractNamespace("http://STPC.LiSim.Abc", ClrNamespace = "STPC.DynamicForms.Web.Models")]
namespace STPC.DynamicForms.Web.Models
{
	[DataContract(IsReference = true)]
	public class User
	{
		[DataMember]
		public Guid Id { get; set; }

		public virtual AplicationName AplicationName { get; set; }

		[DataMember]
		public int? AplicationNameId { get; set; }

		[Required]
		[MaxLength(255)]
		[DataMember]
		public string Username { get; set; }

		[MaxLength(64)]
		[Display(Name = "Nombres")]
		[DataMember]
		public string GivenName { get; set; }

		[MaxLength(64)]
		[Display(Name = "Apellidos")]
		[DataMember]
		public string LastName { get; set; }

		[Required]
		[MaxLength(128)]
		[DataMember]
		public string Email { get; set; }

		[Required]
		[MaxLength(128)]
		[DataMember]
		public string Password { get; set; }

		[MaxLength(255)]
		[DataMember]
		public string PasswordQuestion { get; set; }

		[MaxLength(255)]
		[DataMember]
		public string PasswordAnswer { get; set; }

		[DataMember]
		public bool IsApproved { get; set; }

		[DataMember]
		public DateTime LastActivityDate { get; set; }

		[DataMember]
		public DateTime LastLoginDate { get; set; }

		[DataMember]
		public DateTime LastPasswordChangedDate { get; set; }

		[DataMember]
		public DateTime CreationDate { get; set; }

		[DataMember]
		public bool IsOnLine { get; set; }

		[DataMember]
		public bool IsLockedOut { get; set; }

		[DataMember]
		public DateTime LastLockedOutDate { get; set; }

		[DataMember]
		public int FailedPasswordAttemptCount { get; set; }

		[DataMember]
		public DateTime FailedPasswordAttemptWindowStart { get; set; }

		[DataMember]
		public int FailedPasswordAnswerAttemptCount { get; set; }

		[DataMember]
		public DateTime FailedPasswordAnswerAttemptWindowStart { get; set; }

		[MaxLength(64)]
		[DataMember]
		public string Phone_LandLine { get; set; }

		[MaxLength(64)]
		[DataMember]
		public string Phone_Mobile { get; set; }

		[DataMember]
		public DateTime? Vacations_Start { get; set; }

		[DataMember]
		public DateTime? Vacations_End { get; set; }

		[Required]
		[DataMember]
		public virtual Hierarchy Hierarchy { get; set; }

		[DataMember]
		public virtual ICollection<Role> Roles { get; set; }

		[DataMember]
		public string Token { get; set; }

		[Display(Name = "Dirección")]
		[DataMember]
		public string Address { get; set; }

		[DataMember]
		public bool IsResetPassword { get; set; }

		[MaxLength(256)]
		public string WorkStation { get; set; }
	}
}