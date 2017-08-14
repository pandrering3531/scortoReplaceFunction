using STPC.DynamicForms.Web.Common.Services.Users;
using STPC.DynamicForms.Web.RT.Models;
using STPC.DynamicForms.Web.RT.Services.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace STPC.DynamicForms.Web.RT.Helpers
{


	public class ObjectPermissionsProvider
	{
		STPC_FormsFormEntities db = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

		

		public ObjectPermissionsProvider() { }

		
		/// <summary>
		/// Permissions of the any object by Table
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="objectName"></param>
		/// <param name="Permissions"> String separated by commas</param>
		/// <param name="role"></param>
		/// <returns></returns>
		public string ValidatePermission(string tableName, string objectName, string userName)
		{
			string[] roles = Roles.GetRolesForUser(userName);
			string result=string.Empty;
			List<STPC.DynamicForms.Web.RT.Services.Entities.ObjectPermissions> _ObjectPermissions =
				db.ObjectPermissions.Expand("Role").Where(op => op.TableName == tableName && op.ObjectName == objectName).ToList();
			foreach (var item in _ObjectPermissions)
			{
				if (_ObjectPermissions != null)
				{
					if (roles.Contains(item.Role.Rolename))
					{
						return item.Permission;
					}
					else
						result= string.Empty;
				}
				else
				{
					result =string.Empty;
				}
			}

			return result;
		}

	}



	//public class ListWithEvents<T> : List<T>
	//{
	//	public event EventHandler ItemAdding;
	//	public event EventHandler ItemAdded;

	//	public new void Add(T item)
	//	{
	//		if (ItemAdding != null)
	//			ItemAdding(item, EventArgs.Empty);

	//		base.Add(item);

	//		if (ItemAdded != null)
	//			ItemAdded(item, EventArgs.Empty);

	//	}		

	//}

}


