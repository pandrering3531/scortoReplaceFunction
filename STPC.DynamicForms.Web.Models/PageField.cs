using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;


namespace STPC.DynamicForms.Web.Models
{

	
	public class PageField
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public Guid Uid { get; set; }

		[Required]
		public virtual Panel Panel { get; set; }

		[Required]
		public Guid FormFieldType_Uid { get; set; }
		
		[ForeignKey("FormFieldType_Uid")]
		public virtual PageFieldType FormFieldType { get; set; }
		
		[MaxLength(30)]
		public string FormFieldName { get; set; }
		
		[MaxLength(1500)]
		public string FormFieldPrompt { get; set; }
		
		[MaxLength(1500)]
		public string LiteralText { get; set; }

		public bool IsRequired { get; set; }

		public bool IsHidden { get; set; }
		
		public bool IsMultipleSelect { get; set; }
		
		public bool IsEmptyOption { get; set; }

		[MaxLength(255)]		
		public string EmptyOption { get; set; }
		
		[MaxLength(255)]		
		public string ValidExtensions { get; set; }
		
		[MaxLength(255)]		
		public string ErrorExtensions { get; set; }
		
		[MaxLength(30)]		
		public string Orientation { get; set; }
		
		[MaxLength(5)]		
		public string OptionsMode { get; set; }
		
		public string Options { get; set; }
		
		public string OptionsCategoryName { get; set; }
		
		[MaxLength(256)]
		public string OptionsWebServiceUrl { get; set; }
		
		public int? ListSize { get; set; }
		
		public int? Rows { get; set; }
		
		public int? Cols { get; set; }
		
		public string MaxSize { get; set; } // 22-03-2013: esta propiedad almacena un valor maximo ingresado por el usuario
		
		public string MaxSizeBD { get; set; } // 22-03-2013: se renombra esta propiedad para que almacene un valor maximo de acuerdo al tipo de control
		
		public string MinSize { get; set; }
		
		public int SortOrder { get; set; }
		
		public int PanelColumn { get; set; }
		
		public int PanelColumnSortOrder { get; set; }
		
		public Guid PanelUid { get; set; }
		
		public bool ShowDelete { set; get; }
		
		public int? ValidationStrategyID { get; set; }
		
		public DateTime Timestamp { get; set; }
		
		public string Style { get; set; }

		public virtual ICollection<Role> ViewR { get; set; }

		public virtual ICollection<Role> EditR { get; set; }
		
		public bool Queryable { get; set; }
		
		public string ToolTip { get; set; }


		public PageField()
		{
			this.Timestamp = DateTime.Now;
		}
		public string ViewRoles { get; set; }
		public string EditRoles { get; set; }

	}
}