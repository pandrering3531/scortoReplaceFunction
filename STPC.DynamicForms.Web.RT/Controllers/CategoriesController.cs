using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using STPC.DynamicForms.Web.RT.Services.Entities;
using System.Configuration;

using System.Web.Security;
using STPC.DynamicForms.Web.RT.Helpers;
using STPC.DynamicForms.Web.RT.Models;
using STPC.DynamicForms.Web.Common.Services.Users;
using Newtonsoft.Json;
using STPC.DynamicForms.Web.Common;


namespace STPC.DynamicForms.Web.RT.Controllers
{
    
    
	public class CategoriesController : Controller
	{
		STPC_FormsFormEntities db = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
		ObjectPermissionsProvider _ObjectPermissionsProvider;
		CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
		public static List<STPC.DynamicForms.Web.RT.Services.Entities.Category> listCategory;
		public AbcRedisCacheManager _AbcRedisCacheManager = new AbcRedisCacheManager();

		public CategoriesController()
		{
			db.IgnoreResourceNotFoundException = true;
		}

		[Authorize(Roles = "Administrador, Co-Administrador")]
		public ActionResult List()
		{
			_ObjectPermissionsProvider = new ObjectPermissionsProvider();
			var model = new STPC.DynamicForms.Web.RT.Models.CategoriesViewModel();

			model.Categories = db.Categories.Where(cat => cat.IsActive == true).OrderBy(e => e.Name).ToList();
			listCategory = model.Categories;

			getObjectPermisionsByCategory(model);
			ValidateSingleTenant();
			return View(model);
		}

		private void getObjectPermisionsByCategory(CategoriesViewModel model)
		{


			ViewBag.Category = model.Categories;

			string permission = string.Empty;

			for (int i = 0; i < model.Categories.Count; i++)
			{
				permission = _ObjectPermissionsProvider.ValidatePermission("Categories", model.Categories[i].Name, this.User.Identity.Name);

				if (permission.IndexOf("R") == -1)
				{
					model.Categories.Remove(model.Categories[i]);
					i--;

				}

			}
		}

		private void ValidateSingleTenant()
		{
			//Consulta si se maneja multiEmpresa
			int IsSingleTenant = 0;
			bool isAdmon = false;

			if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
			{
				IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

			}

			//Valida si el usuario Logeado es administrador
			if (Roles.GetRolesForUser(this.User.Identity.Name).Contains("Administrador"))
			{
				isAdmon = true;
			}


			if (isAdmon && IsSingleTenant == 1)
			{
				ViewBag.IsSingleTenant = 1;
			}
			else
			{
				ViewBag.IsSingleTenant = 0;
			}

			ViewBag.listAplication = AplicationNameManager.LoadAplicationName(db);
		}

		public ActionResult Options(string categoryname = "")
		{
			_ObjectPermissionsProvider = new ObjectPermissionsProvider();
			db.IgnoreResourceNotFoundException = true;
			System.Data.Services.Client.DataServiceQuery<STPC.DynamicForms.Web.RT.Services.Entities.Category> query =
				  db.Categories.Expand("Options");
			STPC.DynamicForms.Web.RT.Services.Entities.Category category = query.Where(n => n.Name == categoryname).FirstOrDefault();

			string permission;
			permission = _ObjectPermissionsProvider.ValidatePermission("Categories", category.Name, this.User.Identity.Name);

			if (permission.IndexOf("U") >= 0)
			{
				ViewData["Editable"] = 1;

			}
			else
			{
				ViewData["Editable"] = 0;
			}

			if (permission.IndexOf("D") >= 0)
			{
				ViewData["Removeable"] = 1;

			}
			else
			{
				ViewData["Removeable"] = 0;
			}

			ValidateSingleTenant();
			ViewBag.categoryname = categoryname;

			ICollection<STPC.DynamicForms.Web.RT.Services.Entities.Option> listOptions = db.Options.Where(e => e.Category.Name == categoryname && e.IsActive == true).ToList();

			int IsSingleTenant = 0;

			if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
			{
				IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

			}
			int? aplicationNameIdUser;
			if (!Roles.GetRolesForUser(this.User.Identity.Name).Contains("Administrador") && IsSingleTenant == 1)
			{

				var theuser = provider.GetUser(this.User.Identity.Name);
				aplicationNameIdUser = theuser.AplicationNameId;
				ViewBag.listOpciones = listOptions.Where(e => e.AplicationNameId == aplicationNameIdUser);
			}
			else
				ViewBag.listOpciones = listOptions;

			return PartialView("_Options", category);

		}

		[HttpPost]
		public ActionResult OptionsByAplicationName(int aplicationNameId, string categoryname = "")
		{
			_ObjectPermissionsProvider = new ObjectPermissionsProvider();
			db.IgnoreResourceNotFoundException = true;
			//System.Data.Services.Client.DataServiceQuery<STPC.DynamicForms.Web.RT.Services.Entities.Category> query =
			//	  db.Categories.Expand("Options");
			STPC.DynamicForms.Web.RT.Services.Entities.Category category = db.Categories.Expand("Options").Where(n => n.Name == categoryname).FirstOrDefault();


			string permission;
			permission = _ObjectPermissionsProvider.ValidatePermission("Categories", category.Name, this.User.Identity.Name);

			if (permission.IndexOf("U") >= 0)
			{
				ViewData["Editable"] = 1;

			}
			else
			{
				ViewData["Editable"] = 0;
			}

			if (permission.IndexOf("D") >= 0)
			{
				ViewData["Removeable"] = 1;

			}
			else
			{
				ViewData["Removeable"] = 0;
			}

			ValidateSingleTenant();
			ViewBag.categoryname = categoryname;

			if (aplicationNameId == 0)

				ViewBag.aplicationName = null;
			else
				ViewBag.aplicationName = aplicationNameId;

			ICollection<STPC.DynamicForms.Web.RT.Services.Entities.Option> listOptions = new List<Services.Entities.Option>();

			if (aplicationNameId != 0)
				listOptions = db.Options.Where(e => e.AplicationNameId == aplicationNameId && e.Category.Name == categoryname && e.IsActive == true).ToList();
			else
				listOptions = db.Options.Where(e => e.Category.Name == categoryname && e.IsActive == true).ToList();
			ViewBag.listOpciones = listOptions;

			//category.Options = (System.Collections.ObjectModel.Collection<STPC.DynamicForms.Web.RT.Services.Entities.Option>)listOptions;

			//_AbcRedisCacheManager.UpdateCache(category.Options.ToList(), "listOptions");
			return PartialView("_Options", category);

		}

		[HttpPost]
		[Authorize(Roles = "Administrador, Co-Administrador")]
		public ActionResult AddCategory(string Name, string Dependency, string aplicationName)
		{

			if (listCategory.Where(e => e.Name.ToUpper() == Name.ToUpper()).ToList().Count > 0)
			{
				return Json(new { Success = false, name = "Ya existe categoría con el nombre " + Name });
			}


			if (string.IsNullOrEmpty(Dependency))
				Dependency = "0";

			var NewCategory = new STPC.DynamicForms.Web.RT.Services.Entities.Category { Name = Name, Dependency = Convert.ToInt32(Dependency), IsActive = true };

			ViewBag.Category = db.Categories.ToList();

			if (!string.IsNullOrEmpty(aplicationName))
			{
				db.AddRelatedObject(db.AplicationName.Where(e => e.Id == Convert.ToInt32(aplicationName)).FirstOrDefault(), "Category", NewCategory);
			}
			else
			{
				db.AddToCategories(NewCategory);
			}

			db.SaveChanges();
			CreatePermissions(NewCategory);

			//op.Role.Rolename=this.User.Identity.

			return Json(new { Success = true, name = NewCategory.Name });
		}

		private void CreatePermissions(STPC.DynamicForms.Web.RT.Services.Entities.Category NewCategory)
		{
			STPC.DynamicForms.Web.RT.Services.Entities.ObjectPermissions op;

			STPC.DynamicForms.Web.Common.Services.Users.Role[] roles;
			//Crea los permisos al usuario actual para la categoria que está creando
			using (UserServiceClient srv = new UserServiceClient())
			{
				roles = srv.GetUser(this.User.Identity.Name, false).Roles;
			}

			foreach (var item in roles)
			{
				op = new STPC.DynamicForms.Web.RT.Services.Entities.ObjectPermissions();
				op.ObjectName = NewCategory.Name;
				op.TableName = "Categories";
				op.Permission = "R,D,U";
				op.Role = new Services.Entities.Role();
				STPC.DynamicForms.Web.RT.Services.Entities.Role _role = db.Roles.Expand("Roles").Where(e => e.Rolename == item.Rolename).FirstOrDefault();

				db.AddRelatedObject(_role, "Roles", op);
				db.SaveChanges();

			}
		}

		[HttpPost]
		[Authorize(Roles = "Administrador, Co-Administrador")]
		public ActionResult DeleteCategory(string Name)
		{
			var TheCategory = db.Categories.Where(cn => cn.Name == Name).FirstOrDefault();
			if (TheCategory != null)
			{
				TheCategory.IsActive = false;
				db.UpdateObject(TheCategory);
				db.SaveChanges();
				return Json(new { Success = true });
			}
			return Json(new { Success = false });
		}


		[HttpPost]
		[Authorize(Roles = "Administrador, Co-Administrador")]
		public ActionResult AddOption(string CategoryName, string OptionKey, string OptionValue, int aplicationNameId, int idOptionParent = -1)
		{
			var TheCategory = db.Categories.Where(n => n.Name == CategoryName).Single();
			var TheNewOption = new STPC.DynamicForms.Web.RT.Services.Entities.Option { Key = OptionKey, Value = OptionValue, Category_Uid = TheCategory.Uid, Option_Uid_Parent = idOptionParent };
			TheNewOption.IsActive = true;
			TheNewOption.Category = TheCategory;

			if (aplicationNameId != 0)
			{
				TheNewOption.AplicationNameId = aplicationNameId;
			}
			else
			{
				int IsSingleTenant = 0;

				if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
				{
					IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

				}
				//Valida si el usuario Logeado es administrador
				bool isAdmon = false;
				if (Roles.GetRolesForUser(this.User.Identity.Name).Contains("Administrador"))
				{
					isAdmon = true;
				}


				if (isAdmon && IsSingleTenant == 1)
				{
					TheNewOption.AplicationNameId = null;
				}
				if (!isAdmon && IsSingleTenant == 1)
				{
					var theuser = provider.GetUser(this.User.Identity.Name);
					TheNewOption.AplicationNameId = theuser.AplicationNameId;
				}
				else
					TheNewOption.AplicationNameId = null;
			}

			db.AddRelatedObject(TheCategory, "Options", TheNewOption);
			db.SaveChanges();

			_AbcRedisCacheManager.Create(TheNewOption, false, false, "listOptions");

			return Json(new { Success = true, id = CategoryName, key = TheNewOption.Key, value = TheNewOption.Value });
		}

		//[HttpPost]
		//[Authorize(Roles = "Administrador, Co-Administrador")]
		//public ActionResult RemoveOption(string OptionKey, string CategoryName)
		//{
		//	var TheCategory = db.Categories.Where(n => n.Name == CategoryName).Single();

		//	var TheOption = db.Options.Where(cn => cn.Category_Uid == TheCategory.Uid).Where(k => k.Key == OptionKey).FirstOrDefault();
		//	if (TheOption != null)
		//	{
		//		db.DeleteObject(TheOption);
		//		db.SaveChanges();
		//		return Json(new { Success = true });
		//	}
		//	return Json(new { Success = false });
		//}

		[HttpPost]
		[Authorize(Roles = "Administrador, Co-Administrador")]
		public ActionResult RemoveOption(int OptionKey, string CategoryName)
		{
			var TheCategory = db.Categories.Where(n => n.Name == CategoryName).Single();

			var TheOption = db.Options.Where(cn => cn.Category_Uid == TheCategory.Uid).Where(k => k.Uid == OptionKey).FirstOrDefault();
			if (TheOption != null)
			{
				TheOption.IsActive = false;
				db.UpdateObject(TheOption);
				db.SaveChanges();

				_AbcRedisCacheManager.Create(TheOption, false, true, "listOptions");

				return Json(new { Success = true });
			}
			return Json(new { Success = false });
		}

		[AcceptVerbs(HttpVerbs.Get)]
		[Authorize(Roles = "Administrador, Co-Administrador")]
		public JsonResult LoadCategories()
		{
			var modelList = db.Categories.Where(e => e.IsActive == true).OrderBy(or => or.Name).ToList();


			_ObjectPermissionsProvider = new ObjectPermissionsProvider();

			ValidatePermisionsCategories(modelList);

			var modelData = modelList.Select(m => new SelectListItem()
			{
				Text = m.Name,
				Value = m.Uid.ToString(),

			});

			ViewBag.listAplication = AplicationNameManager.LoadAplicationName(db);
			return Json(modelData, JsonRequestBehavior.AllowGet);
		}

		[AcceptVerbs(HttpVerbs.Get)]
		[Authorize(Roles = "Administrador, Co-Administrador")]
		public JsonResult LoadOptionsByIdOption(int IdCategoria)
		{
			List<STPC.DynamicForms.Web.RT.Services.Entities.Option> modelList = db.Options.Where(e => e.Category_Uid == IdCategoria && e.IsActive == true).ToList();

			var modelData = modelList.Select(m => new SelectListItem()
			{
				Text = m.Value,
				Value = m.Uid.ToString(),

			});


			return Json(modelData, JsonRequestBehavior.AllowGet);
		}

		[Authorize(Roles = "Administrador, Co-Administrador")]
		public ActionResult Edit(int id)
		{
			STPC.DynamicForms.Web.RT.Services.Entities.Option _EditOption = db.Options.Where(e => e.Uid == id).FirstOrDefault();
			ValidateSingleTenant();
			return PartialView(_EditOption);
		}

		[HttpPost]
		public ActionResult Edit(STPC.DynamicForms.Web.RT.Services.Entities.Option model, FormCollection par)
		{
			STPC.DynamicForms.Web.RT.Services.Entities.Option _Option = db.Options.Where(e => e.Uid == model.Uid).FirstOrDefault();

			//Valida si llega el AplicationName del usuario
			int? aplicationNameIdUser;

			if (par.AllKeys.Contains("ddlAplicationNameOptionsEdit") == true)
			{
				if (par["ddlAplicationNameOptionsEdit"].ToString() == string.Empty)
					aplicationNameIdUser = null;
				else
					aplicationNameIdUser = Convert.ToInt32(par["ddlAplicationNameOptionsEdit"].ToString());

			}
			else
			{
				int IsSingleTenant = 0;

				if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
				{
					IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

				}
				if (IsSingleTenant == 1)
				{
					var theuser = provider.GetUser(this.User.Identity.Name);
					aplicationNameIdUser = theuser.AplicationNameId;
				}
				else
					aplicationNameIdUser = null;
			}

			_Option.AplicationNameId = aplicationNameIdUser;
			_Option.Key = model.Key;
			_Option.Value = model.Value;

			if (!string.IsNullOrEmpty(par["ddlDependencyOptionEdit"]))
			{
				_Option.Option_Uid_Parent = Convert.ToInt32(par["ddlDependencyOptionEdit"]);
			}

			db.UpdateObject(_Option);
			db.SaveChanges();

			_AbcRedisCacheManager.Create(_Option, true, false, "listOptions");

			return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);

		}


		[Authorize(Roles = "Administrador, Co-Administrador")]
		public ActionResult Edit_category(string id)
		{
			//STPC.DynamicForms.Web.RT.Services.Entities.Category _EditOption = db.Categories.Expand("AplicationName").Where(e => e.Name == id && e.IsActive == true).FirstOrDefault();

			List<STPC.DynamicForms.Web.RT.Services.Entities.Category> listCat = db.Categories.Expand("AplicationName").Where(e => e.IsActive == true).OrderBy(or => or.Name).ToList();
			STPC.DynamicForms.Web.RT.Services.Entities.Category _EditOption = listCat.Where(e => e.Name == id && e.IsActive == true).FirstOrDefault();


			_ObjectPermissionsProvider = new ObjectPermissionsProvider();

			ValidatePermisionsCategories(listCat);

			ViewBag.Category = listCat;
			ViewBag.listAplication = AplicationNameManager.LoadAplicationName(db);
			return PartialView(_EditOption);
		}

		private void ValidatePermisionsCategories(List<STPC.DynamicForms.Web.RT.Services.Entities.Category> listCat)
		{
			string permission = string.Empty;

			for (int i = 0; i < listCat.Count; i++)
			{
				permission = _ObjectPermissionsProvider.ValidatePermission("Categories", listCat[i].Name, this.User.Identity.Name);

				if (permission.IndexOf("R") == -1)
				{
					listCat.Remove(listCat[i]);
					i--;

				}

			}
		}



		[HttpPost]
		public ActionResult Edit_category(string Name, string Dependency, int Uid, string aplicationName)
		{


			if (listCategory.Where(e => e.Name == Name && e.Uid != Uid).ToList().Count > 0)
			{
				return Json(new { Success = false, name = "Ya existe categoría con el nombre " + Name });
			}
			if (string.IsNullOrEmpty(Dependency))
				Dependency = "0";

			STPC.DynamicForms.Web.RT.Services.Entities.Category _Category = db.Categories.Expand("AplicationName").Where(e => e.Uid == Uid).FirstOrDefault();
			string lastName = _Category.Name;

			_Category.Dependency = Convert.ToInt32(Dependency);
			_Category.Name = Name;

			if (aplicationName == string.Empty)
				_Category.AplicationNameId = null;
			else
				_Category.AplicationNameId = Convert.ToInt32(aplicationName);

			if (Name != lastName)
				CreatePermissions(_Category);

			db.UpdateObject(_Category);
			db.SaveChanges();

			UpdateAplicationNameToCategorie(_Category.Uid, _Category.AplicationNameId);
			return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);

		}

		private void UpdateAplicationNameToCategorie(int uid, int? aplicationNameId)
		{
			List<STPC.DynamicForms.Web.RT.Services.Entities.Category> _ListCategory = db.Categories.Expand("AplicationName").Where(e => e.Dependency == uid).ToList();


			if (_ListCategory.Count > 0)
			{
				foreach (var item in _ListCategory)
				{
					item.AplicationNameId = aplicationNameId;

					db.UpdateObject(item);
					db.SaveChanges();

					if (item.Dependency > 0)
						UpdateAplicationNameToCategorie(item.Uid, item.AplicationNameId);
				}

			}

		}

		[AcceptVerbs(HttpVerbs.Get)]
		[Authorize(Roles = "Administrador, Co-Administrador")]
		public JsonResult LoadDependenciaByCategorie(int IdCategoria)
		{
			List<STPC.DynamicForms.Web.RT.Services.Entities.Category> _Category = new List<Services.Entities.Category>();
			_Category = db.Categories.Expand("AplicationName").Where(cat => cat.Uid == IdCategoria).ToList();

			IEnumerable<SelectListItem> modelData;

			if (_Category[0].AplicationName != null)
			{
				modelData = _Category.Select(m => new SelectListItem()
				{
					Text = m.AplicationName.Name,
					Value = m.AplicationNameId.ToString(),

				});
			}
			else
			{
				modelData = _Category.Select(m => new SelectListItem()
				{
					Text = "---Seleccione el nombre de la aplicación---",
					Value = "",
					Selected = true

				});
			}
			return Json(modelData, JsonRequestBehavior.AllowGet);
		}


	}
}