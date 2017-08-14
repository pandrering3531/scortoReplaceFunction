using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using STPC.DynamicForms.Web;

namespace STPC.DynamicForms.Web.Models
{

	/// <summary>
	/// Clase para guardar la data del formulario y luego convertirlo a XML
	/// </summary>
	public class FormDataToXml
	{

		public string NameForm { get; set; }

		public Guid IdForm { get; set; }

		public string NameFormPage { get; set; }

		public Guid IdFormPage { get; set; }

		public string NamePanel { get; set; }

		public Guid Idpanel { get; set; }

		public Dictionary<string, string> FieldsByPage { get; set; }

	}
	public class FormViewModel
	{
		//Properties
		public Services.Entities.Panel panel { get; set; }
		public List<FormFieldViewModel> formfields { get; set; }

		//Constructor
		public FormViewModel()
		{
			formfields = new List<FormFieldViewModel>();
		}
	}

	public class FormPageViewModel
	{
		public Services.Entities.FormPage Page { get; set; }
		public List<Services.Entities.Panel> Panels { get; set; }
		public List<Services.Entities.PageTemplate> Templates { get; set; }

		//Constructor
		public FormPageViewModel()
		{
			Page = new Services.Entities.FormPage();
			Panels = new List<Services.Entities.Panel>();
			Templates = new List<Services.Entities.PageTemplate>();
		}


	}

	//public class FormResponseViewModel
	//{
	//    //Properties
	//    public Services.Entities.Form Form { get; set; }
	//    public List<Services.Entities.FormField> FormFields { get; set; }
	//    public List<Services.Entities.FormFieldType> FormFieldTypes { get; set; }

	//    //Constructor
	//    public FormResponseViewModel()
	//    {
	//        Form = new Services.Entities.Form();
	//        FormFields = new List<Services.Entities.FormField>();
	//        FormFieldTypes = new List<Services.Entities.FormFieldType>();
	//    }
	//}

	public class FormFieldViewModel
	{
		//Set int properties to int? nullable to avoid issues across field types

		//Primary key
		public Guid Uid { get; set; }

		//Control type
		public string ControlType { get; set; }

		// control type name
		public string ControlTypeName { get; set; }

		//Settings
		public bool ShowDelete { get; set; }
		public bool IsRequired { get; set; }

		//Name
		[Required(ErrorMessage = "* Required field")]
		public string FormFieldName { get; set; }

		//Prompt (description)
		public string FormFieldPrompt { get; set; }

		//Form field type (list & value)
		public List<SelectListItem> FormFieldTypes { get; set; }
		[Required(ErrorMessage = "* Select a field type")]
		public string SelectedFormFieldType { get; set; }

		//List options, row delimited
		public string Options { get; set; }

		//List layout, horizontal or vertical
		[Required(ErrorMessage = "* Please select orientation")]
		public string Orientation { get; set; }

		//Select list options
		public bool IsMultipleSelect { get; set; }
		[Range(1, 20, ErrorMessage = "* Must be between 1-20")]
		public int? ListSize { get; set; }
		public bool IsEmptyOption { get; set; }
		public string EmptyOption { get; set; }

		//Text area options
		[Range(1, 50, ErrorMessage = "* Must be between 1-50")]
		public int? Rows { get; set; }
		[Range(1, 100, ErrorMessage = "* Must be between 1-100")]
		public int? Cols { get; set; }

		//File upload options
		[RegularExpression(@"^(\*\.([a-zA-Z]{1,5})?,?){1,10}$", ErrorMessage = "Please enter valid file extension(s)")]
		public string ValidExtensions { get; set; }
		public string ErrorExtensions { get; set; }
		//[Range(1, 10000000, ErrorMessage = "* Must be between 1-10000000 bytes")]
		// se modifica la propiedad MaxSizeBytes por MaxSize
		// para que represente diferentes valores de acuerdo al tipo de control
		[Required(ErrorMessage = "* Please enter a max size")]
		public string MaxSize { get; set; }

		public string MinSize { get; set; }

		public string MaxSizeBD { get; set; }

		//Literal options
		[Required(ErrorMessage = "* Please enter display text")]
		public string LiteralText { get; set; }

		public string OptionsMode { get; set; }

		public string OptionsCategoryName { get; set; }

		public string OptionsWebServiceUrl { get; set; }

		public string[] AvailableRoles { get; set; }

		public string ValidationStrategyId { get; set; }

		public string ToolTip { get; set; }
		public Guid PanelUid { set; get; }

		public string ViewRoles { get; set; }
		public string EditRoles { get; set; }
		public List<string> SelectedViewRoles { get; set; }
		public List<string> SelectedEditRoles { get; set; }

		public string Style { get; set; }
		public bool Queryable { get; set; }

	}

	//-----------------------------------------------------------------------------------------------------

	//-----------------------------------------------------------------------------------------------------
	//Effectively, this is the admin ViewModel
	public class UserProfileViewModel
	{
		[DisplayName("First Name")]
		[DataType(DataType.Text)]
		public string FirstName { get; set; }

		[DisplayName("Last Name")]
		[DataType(DataType.Text)]
		public string LastName { get; set; }
	}

	//TODO: May wish to reconsider managing this data for a fuller per-user admin ViewModel?
	public class UserAllocationsViewModel
	{
		[DisplayName("User ID")]
		public Guid UserId { get; set; }

		[DisplayName("User Name")]
		public string UserName { get; set; }

		[DisplayName("Available Submissions")]
		[DataType(DataType.Text)]
		[RegularExpression(@"\d+", ErrorMessage = "Please enter a valid number.")]
		[Required(ErrorMessage = "Please enter a value for remaining, available submissions.")]
		[Range(0, 99999, ErrorMessage = "Please enter a number between 0 - 99,999")]
		public int AvailableSubmissions { get; set; }

		[DisplayName("Last Submission Date")]
		[DataType(DataType.DateTime, ErrorMessage = "Please enter a valid date-time format (e.g. 1/1/1900 12:00:00AM).")]
		public DateTime LastSubmission { get; set; }
	}
	//-----------------------------------------------------------------------------------------------------
}