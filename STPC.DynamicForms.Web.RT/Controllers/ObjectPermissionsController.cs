using STPC.DynamicForms.Web.RT.Helpers;
using STPC.DynamicForms.Web.RT.Services.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace STPC.DynamicForms.Web.RT.Controllers
{
    
    
	public class ObjectPermissionsController : Controller
	{
		STPC.DynamicForms.Web.RT.Services.Entities.STPC_FormsFormEntities db = new STPC.DynamicForms.Web.RT.Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));


		public ObjectPermissionsController()
		{
			db.IgnoreResourceNotFoundException = true;
		}

		//
		// GET: /ObjectPermissions/
		public ActionResult Index()
		{
			return View();
		}

		#region AngularJs


		/// <summary>
		/// Carga lista completa de la tabla ObjectPermissions
		/// </summary>
		/// <returns>JSON ObjectPermissions</returns>
		public JsonResult GetObjectConfiguration()
		{

			var _ObjectPermissions = db.ObjectPermissions.Expand("Role").ToList();
			return new JsonResult { Data = _ObjectPermissions, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

		}
		/// <summary>
		/// Carga la lista de registros de la tabla ObjectPermissions
		/// </summary>
		/// <param name="tableName">Nombre de la tabla </param>
		/// <returns>JSON ObjectPermissions</returns>
		public JsonResult GetObjectConfigurationByTableName(string tableName)
		{

			var _ObjectPermissions = db.ObjectPermissions.Expand("Role").Where(e => e.TableName == tableName).OrderBy(o => o.ObjectName).ToList();
			return new JsonResult { Data = _ObjectPermissions, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

		}

		/// <summary>
		/// Carga la lista de roles, menos el que tiene seleccionado el registro que se está editando
		/// </summary>
		/// <param name="roleNameRecord"></param>
		/// <returns></returns>
		public JsonResult GetListRoles(string objectName, string tableName)
		{
			//Carga el objeto que se está editando para sacar los roles que tiene asignados
			List<ObjectPermissions> _ObjectPermissions = db.ObjectPermissions.Expand("Role").Where(e => e.ObjectName == objectName && e.TableName == tableName).OrderBy(o => o.ObjectName).ToList();
			
			var _Roles = db.Roles.ToList();// .Where(joineds => !joined.Contains(joineds.Rolename)).ToList();

			

			return new JsonResult { Data = _Roles, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

		}

		[HttpPost]
		public JsonResult UpdateObjectPermmision(string table, string objectName, string permission, string roleName)
		{
			try
			{

				ObjectPermissions _ObjectPermissionstoUpdate = db.ObjectPermissions.Expand("Role").
					Where(e => e.ObjectName == objectName && e.TableName == table && e.Role.Rolename == roleName).FirstOrDefault();
				ObjectPermissions _ObjectPermissions = null;

				if (_ObjectPermissionstoUpdate == null)
				{
					if (!string.IsNullOrEmpty(permission))
					{
						_ObjectPermissions = new ObjectPermissions();
						_ObjectPermissions.TableName = table;
						_ObjectPermissions.ObjectName = objectName;
						_ObjectPermissions.Permission = permission;
						_ObjectPermissions.Role = new Services.Entities.Role();
						STPC.DynamicForms.Web.RT.Services.Entities.Role _role = db.Roles.Expand("Roles").Where(e => e.Rolename == roleName).FirstOrDefault();

						db.AddRelatedObject(_role, "Roles", _ObjectPermissions);
						db.SaveChanges();
					}
					else
					{
						return new JsonResult { Data = "Seleccione al menos un rol", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
					}
				}
				else
				{
					//Si viene al menos un permiso lo edita
					if (!string.IsNullOrEmpty(permission))
					{
						_ObjectPermissionstoUpdate.Permission = permission;
						db.UpdateObject(_ObjectPermissionstoUpdate);
						
					}
					else
					//De lo contrario lo borra
					{
						db.DeleteObject(_ObjectPermissionstoUpdate);
					}
					db.SaveChanges();
				}
				return new JsonResult { Data = "Success", JsonRequestBehavior = JsonRequestBehavior.AllowGet };
			}
			catch (Exception ex)
			{

				return new JsonResult { Data = ex.Message, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
			}

		}
		#endregion

	}
}
