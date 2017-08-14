using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using STPC.DynamicForms.Web.Models;

namespace STPC.DynamicForms.Web.Models
{
	public class LoginViewModel
	{
		[Required]
		[Display(Name = "Número de identificación")]
		public long Username { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Contraseña")]
		public string Password { get; set; }

		[Required]
		public string StringCode { get; set; }

		[Required]
		public string Idtype { get; set; }

		public List<SelectListItem> IDTypes { get; set; }

		public LoginViewModel()
		{
			this.IDTypes = new List<SelectListItem>();
			foreach (var item in Enum.GetNames(typeof(IDTypesEnumeration)))
			{
				this.IDTypes.Add(new SelectListItem { Text = item, Value = item });
			};
		}
	}
}