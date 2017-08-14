using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace STPC.DynamicForms.Web.Models
{
	public class FormPageActions
	{
		[ScaffoldColumn(false)]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid Uid { get; set; }

		[MaxLength(64)]
		public string Name { get; set; }
		[MaxLength(512)]
		public string Description { get; set; }
		[MaxLength(64)]
		public string Caption { get; set; }
		public bool IsAssociated { get; set; }
		public bool IsExecuteStrategy { get; set; }
		public bool IsPrivateResource { get; set; }
		public Guid PageId { get; set; }
		public int DisplayOrder { get; set; }
		public bool Save { get; set; }
		public Guid? GoToPageId { get; set; }
		public Guid? FormStatesUid { get; set; }
		public virtual ICollection<FormPageActionsRoles> FormPageActionsRolesList { set; get; }
		public bool ShowSuccessMessage { get; set; }
		public bool ShowFailureMessage { get; set; }
		public List<FormPageActionsByStates> FormPageActionsByStatesList { get; set; }
		public FormStates FormStates { get; set; }
		[MaxLength(128)]
		public string SuccessMessage { get; set; }
		[MaxLength(128)]
		public string FailureMessage { get; set; }
		public int StrategyID { get; set; }
		[MaxLength(1024)]
		public string Resource { get; set; }
		public bool SendUserParam { get; set; }
		public bool SendRequestIdParam { get; set; }
		public bool SendHierarchyIdParam { get; set; }

		public FormPageActions()
		{
			//FormPageActionsByStatesList = new List<FormPageActionsByStates>();

			//FormPageActionsRolesList = new List<FormPageActionsRoles>();
		}
	}
}