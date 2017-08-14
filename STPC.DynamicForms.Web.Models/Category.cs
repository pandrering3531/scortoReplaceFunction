using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

[assembly: ContractNamespace("http://STPC.LiSim.Abc", ClrNamespace = "STPC.DynamicForms.Web.Models")]
namespace STPC.DynamicForms.Web.Models
{


	[Table("Categories")]
	[DataContract(IsReference = true)]
	public partial class Category
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Uid { get; set; }

		[MaxLength(128)]
		public string Name { get; set; }

		public int Dependency { get; set; }

		public bool IsActive { get; set; }

		[DataMember]
		public virtual ICollection<Option> Options { get; set; }

		public Category() { }

		public Category(string Name)
		{
			var category = new Category();

		}
		[NotMapped]
		public bool IsEditable { get; set; }
		[NotMapped]
		public bool IsUpgradable { get; set; }
		[NotMapped]
		public bool IsRemovable { get; set; }

		public virtual AplicationName AplicationName { get; set; }


		public int? AplicationNameId { get; set; }
		

	}

}