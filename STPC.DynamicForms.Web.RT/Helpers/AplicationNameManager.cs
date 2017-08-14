using STPC.DynamicForms.Web.RT.Controllers;
using STPC.DynamicForms.Web.RT.Services.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STPC.DynamicForms.Web.RT.Helpers
{
	public static class AplicationNameManager
	{
		public static IEnumerable<StructControl> LoadAplicationName(STPC_FormsFormEntities _stpcForms)
		{
			List<AplicationName> listAplicationName = _stpcForms.AplicationName.ToList();

			var modelData = listAplicationName.Select(m => new StructControl()
			{
				Text = m.Name,
				Value = m.Id.ToString()

			});
			return modelData;
		}
	}
}