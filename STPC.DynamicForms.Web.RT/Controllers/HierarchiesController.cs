using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using STPC.DynamicForms.Core;
using STPC.DynamicForms.Web.RT.Helpers;
using STPC.DynamicForms.Web.RT.Services.Entities;
using STPC.DynamicForms.Web.RT.Controllers;
using System.Web.Security;
using STPC.DynamicForms.Web.Common;

namespace STPC.DynamicForms.Web.Controllers
{
    
    
	public class HierarchiesController : Controller
	{

		CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
		STPC.DynamicForms.Web.RT.Services.Entities.STPC_FormsFormEntities db = new STPC.DynamicForms.Web.RT.Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
		CustomRequestProvider _RequestServiceClient = new CustomRequestProvider();

		public static List<Hierarchy> hierarchyList;

		public HierarchiesController()
		{
			db.IgnoreResourceNotFoundException = true;


		}
        [Authorize(Roles = "Administrador, Parametrizador")]
		public ActionResult List()
		{

			return getList();
		}

		private ActionResult getList()
		{
			ViewBag.HierarchyNodeType = db.HierarchyNodeTypes.ToList();


			//var model = db.Hierarchies.Expand("Parent").Where(jer => jer.IsActive == true).ToList();
			ValidateSingleTenant();

			int IsSingleTenant = 0;
			int? aplicationNameIdUser;

			if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
			{
				IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

			}

			if (!Roles.GetRolesForUser(this.User.Identity.Name).Contains("Administrador") && IsSingleTenant == 1)
			{

				var theuser = provider.GetUser(this.User.Identity.Name);
				aplicationNameIdUser = theuser.AplicationNameId;
				hierarchyList = db.Hierarchies.Expand("Parent").Where(jer => jer.IsActive == true && jer.AplicationNameId == aplicationNameIdUser).OrderBy(e => e.Name).ToList();
				return View(hierarchyList);
			}
			else
			{
				hierarchyList = db.Hierarchies.Expand("Parent").Where(jer => jer.IsActive == true).OrderBy(e => e.Name).ToList();
				return View(hierarchyList);
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

		[HttpPost]
		public ActionResult GetIndex(Hierarchy model, FormCollection par)
		{

			int? aplicationNameIdUser;

			if (par.AllKeys.Contains("ddlAplicationName") == true)
			{
				if (par["ddlAplicationName"].ToString() == string.Empty)
					return PartialView("_List", db.Hierarchies.Expand("Parent").Where(jer => jer.IsActive == true).OrderBy(e => e.Name).ToList());
				else
					return PartialView("_List", db.Hierarchies.Expand("Parent").Where(e => e.AplicationNameId == Convert.ToInt32(par["ddlAplicationName"].ToString()) && e.IsActive == true).OrderBy(e => e.Name).ToList());


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
					return PartialView("_List", db.Hierarchies.Expand("Parent").Where(e => e.AplicationNameId == aplicationNameIdUser && e.IsActive == true).OrderBy(e => e.Name).ToList());
				}
				else
					return PartialView("_List", db.Hierarchies.Expand("Parent").Where(jer => jer.IsActive == true).OrderBy(e => e.Name).ToList());

			}
		}

		[HttpPost]
		public ActionResult getHierarchieByAplicationNameID(int aplicationNameId)
		{
			ValidateSingleTenant();
			List<Hierarchy> listHierarchie = new List<Hierarchy>();
			if (aplicationNameId > 0)
			{
				listHierarchie = db.Hierarchies.Expand("Parent").Where(e => e.AplicationNameId == aplicationNameId && e.IsActive == true).ToList();


			}
			else
				listHierarchie = db.Hierarchies.Expand("Parent").Where(jer => jer.IsActive == true).ToList();

			return PartialView("_List", listHierarchie.OrderBy(e => e.Name).ToList());

		}


		[AcceptVerbs(HttpVerbs.Get)]
		[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
		[Authorize(Roles = "Administrador, Parametrizador")]
		public JsonResult GetHierarchiesForLevel(string level)
		{
			var modelList = db.Hierarchies.Expand("Parent").Where(r => r.Level == level && r.IsActive == true).OrderBy(z => z.Name);
			var modelData = new List<SelectListItem>();
			foreach (var item in modelList)
			{
				string hierarchyname;
				if (item.Parent == null)
					hierarchyname = item.Name;
				else
					hierarchyname = item.Parent.Name + " -> " + item.Name;
				modelData.Add(new SelectListItem { Value = item.Id.ToString(), Text = hierarchyname });
			}
			return Json(modelData, JsonRequestBehavior.AllowGet);
		}

		[AcceptVerbs(HttpVerbs.Get)]
		[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        [Authorize(Roles = "Administrador, Parametrizador")]
		public JsonResult GetHierarchiesForLevelByAplicationName(string level, int aplicationNameId)
		{
			List<Hierarchy> modelList;

			if (aplicationNameId != 0)
				modelList = db.Hierarchies.Expand("Parent").Where(r => r.Level == level && r.IsActive == true && r.AplicationNameId == aplicationNameId).OrderBy(z => z.Name).ToList();
			else
				modelList = db.Hierarchies.Expand("Parent").Where(r => r.Level == level && r.IsActive == true).OrderBy(z => z.Name).ToList();
			var modelData = new List<SelectListItem>();
			foreach (var item in modelList)
			{
				string hierarchyname;
				if (item.Parent == null)
					hierarchyname = item.Name;
				else
					hierarchyname = item.Parent.Name + " -> " + item.Name;
				modelData.Add(new SelectListItem { Value = item.Id.ToString(), Text = hierarchyname });
			}
			return Json(modelData, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
        [Authorize(Roles = "Administrador, Parametrizador")]
		public JsonResult DeleteHierarchy(int id)
		{
			DeleteCascadeHierarchy(id);
			db.SaveChanges();
			return Json(new { Success = true });
		}

		[HttpPost]
        [Authorize(Roles = "Administrador, Parametrizador")]
		public JsonResult AddChild(string name, int parentId, string LevelName, int nodeType)
		{
			var theparent = db.Hierarchies.Where(i => i.Id == parentId).FirstOrDefault();
			if (theparent != null)
			{
				var thechild = new STPC.DynamicForms.Web.RT.Services.Entities.Hierarchy()
				{
					Name = name,
					Parent = theparent,
					NodeType = nodeType,
					Level = LevelName
				};
				db.AddRelatedObject(theparent, "Children", thechild);
				db.SaveChanges();
				return Json(new { Success = true });
			}
			return Json(new { Success = false });
		}

		public JsonResult GetchildrenCountAndLevelName(int id)
		{
			var TheHierarchy = db.Hierarchies.Expand("Children").Where(i => i.Id == id).FirstOrDefault();
			if (TheHierarchy == null) return null;
			if (TheHierarchy.Children.Count == 0) return Json(new { ChildCount = 0, ChildLevelName = string.Empty }, JsonRequestBehavior.AllowGet);
			string childlevelname = TheHierarchy.Children.First().Level;
			return Json(new { ChildCount = TheHierarchy.Children.Count, ChildLevelName = childlevelname }, JsonRequestBehavior.AllowGet);

		}

		public JsonResult EditNode(int id)
		{
			var TheHierarchy = db.Hierarchies.Where(i => i.Id == id).FirstOrDefault();
			if (TheHierarchy == null) return null;
			string childlevelname = TheHierarchy.Level;

			ValidateSingleTenant();

			return Json(new { NodeName = TheHierarchy.Name, ChildLevelName = childlevelname, NodeType = TheHierarchy.NodeType }, JsonRequestBehavior.AllowGet);

		}

		[HttpPost]
        [Authorize(Roles = "Administrador, Parametrizador")]
		public JsonResult EditNode(string name, int nodeId, string LevelName, int nodeType)
		{
			var theNode = db.Hierarchies.Where(i => i.Id == nodeId).FirstOrDefault();

			if (theNode != null)
			{
				theNode.Name = name;
				theNode.NodeType = nodeType;
				db.UpdateObject(theNode);
				db.SaveChanges();
			}
			return Json(new { Success = true });

			// return Json(new { Success = false });
		}


		public void DeleteCascadeHierarchy(int id)
		{
			db.MergeOption = System.Data.Services.Client.MergeOption.PreserveChanges;

			var TheHierarchy = db.Hierarchies.Expand("Children").Where(i => i.Id == id).FirstOrDefault();
			foreach (var child in TheHierarchy.Children)
			{
				DeleteCascadeHierarchy(child.Id);
			}
			TheHierarchy.IsActive = false;
			db.UpdateObject(TheHierarchy);
			db.SaveChanges();

			//db.DeleteObject(TheHierarchy);
		}

		public ActionResult Create(int parentId)
		{


			Hierarchy _Hierarchy = new Hierarchy();

			var TheHierarchy = db.Hierarchies.Expand("Children").Where(i => i.Id == parentId).FirstOrDefault();

			if (TheHierarchy != null)
			{
				ViewBag.HierarchyNodeType = db.HierarchyNodeTypes.Where(uid => uid.AplicationNameId == TheHierarchy.AplicationNameId).ToList();
			}
			else
			{
				ViewBag.HierarchyNodeType = db.HierarchyNodeTypes.ToList();
			}

			if (TheHierarchy == null)
			{
				_Hierarchy.Level = string.Empty;
			}

			else
			{
				if (TheHierarchy.Children.Count > 0)
				{
					_Hierarchy.Level = TheHierarchy.Children.First().Level;
				}
				else
					_Hierarchy.Level = TheHierarchy.Level;
			}

			var theparent = db.Hierarchies.Where(i => i.Id == parentId).FirstOrDefault();

			_Hierarchy.Parent = new Hierarchy();

			if (theparent != null)
			{
				_Hierarchy.Parent = theparent;
			}


			ValidateSingleTenant();
			return PartialView(_Hierarchy);
		}

		public ActionResult Edit(int NodeId)
		{


			ViewBag.HierarchyNodeType = db.HierarchyNodeTypes.ToList();

			var TheHierarchy = db.Hierarchies.Expand("Parent").Where(i => i.Id == NodeId).FirstOrDefault();

			if (TheHierarchy.Parent != null && TheHierarchy.Parent.Id > 0)
			{
				ViewBag.HierarchyNodeType = db.HierarchyNodeTypes.Where(uid => uid.AplicationNameId == TheHierarchy.AplicationNameId).ToList();
			}
			else
			{
				ViewBag.HierarchyNodeType = db.HierarchyNodeTypes.ToList();
			}

			if (TheHierarchy == null) return null;
			string childlevelname = TheHierarchy.Level;

			if (TheHierarchy.NodeType != 0)
			{
				HierarchyNodeType _HierarchyNodeType = db.HierarchyNodeTypes.Where(e => e.Id == TheHierarchy.NodeType).FirstOrDefault();

				if (_HierarchyNodeType != null)
				{

					List<NodeTypeDetail> _ListNodeTypeDetail = db.NodeTypeDetail.Expand("FieldType").Where(e => e.HierarchyNodeType.Id == _HierarchyNodeType.Id).OrderBy(e => e.SortOrder).ToList();
					List<NodeTypeDetail_Extended> _ListNodeTypeDetail_Extended = new List<NodeTypeDetail_Extended>();

					MyViewModel modelQuery = new MyViewModel();
					modelQuery = _RequestServiceClient.GetSchemaTable(_HierarchyNodeType.TableName, NodeId);

					string _valueField = string.Empty;

					foreach (NodeTypeDetail item in _ListNodeTypeDetail)
					{
						CellValueViewModel _RowViewModel = null;
						if (modelQuery.Rows.Count > 0)
						{
							_RowViewModel = modelQuery.Rows[0].Values.Where(e => e.ColumnName == item.FieldName).FirstOrDefault();
							_valueField = _RowViewModel.Value.Trim();
						}
						else
							_valueField = string.Empty;

						if (item.OptionsCategoryName != null)
						{
							var TheCategory = db.Categories.Where(n => n.Name == item.OptionsCategoryName && n.IsActive).Single();
							var TheOptions = db.Options.Where(c => c.Category_Uid == TheCategory.Uid && c.IsActive).OrderBy(e => e.Value).ToList();

							var modelData = TheOptions.Select(m => new SelectListItem()
							{
								Text = m.Value,
								Value = m.Uid.ToString(),
							});
							ViewData[item.FieldName] = modelData;
						}

						_ListNodeTypeDetail_Extended.Add(
							new NodeTypeDetail_Extended
							{
								FieldName = item.FieldName,
								FieldPrompt = item.FieldPrompt,
								FieldType = item.FieldType,
								HierarchyNodeType = item.HierarchyNodeType,
								IsRequired = item.IsRequired,
								MaxSize = item.MaxSize,
								MinSize = item.MinSize,
								NodeTypeDetailId = item.NodeTypeDetailId,
								OptionsCategoryName = item.OptionsCategoryName,
								PageFieldTypeId = item.PageFieldTypeId,
								SortOrder = item.SortOrder,
								Style = item.Style,
								ValueField = _valueField
							});
					}



					ValidateSingleTenant();
					ViewBag.modelQuery = _ListNodeTypeDetail_Extended;

					if (modelQuery.Rows.Count == 0)
						ViewBag.AddAttr = 1;
					else
						ViewBag.AddAttr = 0;
				}
			}
			return PartialView(TheHierarchy);
		}

		[HttpPost]
        [Authorize(Roles = "Administrador, Parametrizador")]
		public ActionResult Create(Hierarchy model, FormCollection par)
		{
			var theparent = db.Hierarchies.Where(i => i.Id == model.Parent.Id).FirstOrDefault();
			db.IgnoreResourceNotFoundException = true;
			//Valida multiempresa y role del usuario

			int? aplicationNameIdUser;


			if (theparent != null)
				model.AplicationNameId = theparent.AplicationNameId;
			else
			{
				if (par.AllKeys.Contains("AplicationNameId") == true)
				{
					if (par["AplicationNameId"].ToString() == string.Empty)
						aplicationNameIdUser = null;
					else
						aplicationNameIdUser = Convert.ToInt32(par["AplicationNameId"].ToString());

				}
				else
				{
					int IsSingleTenant = 0;

					if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
					{
						IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

					}
					bool isAdmon = false;
					if (Roles.GetRolesForUser(this.User.Identity.Name).Contains("Administrador"))
					{
						isAdmon = true;
					}

					if (isAdmon && IsSingleTenant == 1)
					{
						aplicationNameIdUser = null;
					}
					if (!isAdmon && IsSingleTenant == 1)
					{
						var theuser = provider.GetUser(this.User.Identity.Name);
						aplicationNameIdUser = theuser.AplicationNameId;
					}
					else
						aplicationNameIdUser = null;
				}
				model.AplicationNameId = aplicationNameIdUser;
			}
			//


			if (model.Parent != null && model.Parent.Id > 0)
				model.Level = par["LevelChild"];
			else

				if (par.AllKeys.Contains("Level"))
					model.Level = par["Level"];
				else
					model.Level = par["LevelChild"];

			model.IsActive = true;

			if (theparent != null)
				db.AddRelatedObject(theparent, "Children", model);
			else
			{
				model.Parent = null;
				db.AddToHierarchies(model);
			}


			// guardar cambios
			db.SaveChanges();

			Dictionary<string, string> dicfields = new Dictionary<string, string>();

			for (int i = 0; i < par.Count; i++)
			{
				string[] fieds = par.GetKey(i).Split('_');


				if (fieds.Length == 2)
				{


					if (fieds[1] == Constants.NAME_FIELD_ID_DEFAULT_ATTR_HIERARCHY)
					{
						dicfields.Add(fieds[1], model.Id.ToString());
					}
					else
						dicfields.Add(fieds[1], par[i]);
				}
			}
			HierarchyNodeType _HierarchyNodeType = db.HierarchyNodeTypes.Where(nt => nt.Id == model.NodeType).FirstOrDefault();
			dicfields.Add("NodeId", model.Id.ToString());
			dicfields.Add("Created", DateTime.Now.ToString("MM/dd/yyyy"));
			dicfields.Add("Updated", DateTime.Now.ToString("MM/dd/yyyy"));
			dicfields.Add("CreatedBy", this.User.Identity.Name);

			if (_HierarchyNodeType != null)
			{
				_RequestServiceClient.InsertAtributesHierarchy(dicfields, _HierarchyNodeType.TableName);
			}

			return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
        [Authorize(Roles = "Administrador, Parametrizador")]
		public ActionResult Edit(Hierarchy model, FormCollection par)
		{
			var theNode = db.Hierarchies.Where(i => i.Id == model.Id).FirstOrDefault();

			//Valida multiempresa y role del usuario


			theNode.Name = model.Name;
			theNode.NodeType = model.NodeType;
			theNode.AplicationNameId = model.AplicationNameId;
			db.UpdateObject(theNode);
			db.SaveChanges();
			//Si tiene el dropDown de empresa visible, edita los hijos con base de la empresa seleccionada
			//De lo contrario 
			if (par.AllKeys.Contains("AplicationNameId") == true)
			{
				updateChildHierarchy(theNode,model.AplicationNameId);
			}
		

			Dictionary<string, string> dicfields = new Dictionary<string, string>();

			for (int i = 0; i < par.Count; i++)
			{
				string[] fieds = par.GetKey(i).Split('_');

				if (fieds.Length == 2)
				{
					if (fieds[1] == Constants.NAME_FIELD_ID_DEFAULT_ATTR_HIERARCHY)
					{
						dicfields.Add(fieds[1], theNode.Id.ToString());
					}
					else
						if (par[i] == "")
							dicfields.Add(fieds[1], " ");
						else
							dicfields.Add(fieds[1], par[i]);
				}
			}
			HierarchyNodeType _HierarchyNodeType = db.HierarchyNodeTypes.Where(nt => nt.Id == model.NodeType).FirstOrDefault();
			dicfields.Add("NodeId", model.Id.ToString());
			dicfields.Add("Updated", DateTime.Now.ToString("MM/dd/yyyy"));
			dicfields.Add("UpdatedBy", this.User.Identity.Name);

			//Valida si el atributo nodeType contiene registros en la tabla asignada. puesto que pudo se modificada.
			int iCountReg = _RequestServiceClient.FindRecordIntoAtributeTable(_HierarchyNodeType.TableName, model.Id);


			if (_HierarchyNodeType != null)
			{
				if (iCountReg > 0)
					_RequestServiceClient.UpdateAtributesHierarchy(dicfields, _HierarchyNodeType.TableName);
				else
				{
					dicfields.Add("Created", DateTime.Now.ToString("MM/dd/yyyy"));
					dicfields.Add("CreatedBy", this.User.Identity.Name);
					_RequestServiceClient.InsertAtributesHierarchy(dicfields, _HierarchyNodeType.TableName);

				}
			}


			return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
		}

		private void updateChildHierarchy(Hierarchy Parent, int? aplicationName)
		{

			List<Hierarchy> _Hierarchy = hierarchyList.Where(e => e.Parent != null && e.Parent.Id == Parent.Id).ToList();

			foreach (var chield in _Hierarchy)
			{
				
				var theNode = db.Hierarchies.Where(i => i.Id == chield.Id).FirstOrDefault();

				//Valida multiempresa y role del usuario
				theNode.Name = chield.Name;
				theNode.AplicationNameId = aplicationName;
				theNode.NodeType = chield.NodeType;
				db.UpdateObject(theNode);
				db.SaveChanges();

				updateChildHierarchy(chield, aplicationName);
			}
		}

		[HttpPost]
		public ActionResult GetSchemaTable(int NodeType, int NodeId)
		{
			MyViewModel modelQuery = new MyViewModel();
			HierarchyNodeType _HierarchyNodeType = db.HierarchyNodeTypes.Where(e => e.Id == NodeType).FirstOrDefault();
			List<NodeTypeDetail> _ListNodeTypeDetail = db.NodeTypeDetail.Expand("FieldType").Where(e => e.HierarchyNodeType.Id == NodeType).OrderBy(e => e.SortOrder).ToList();

			List<NodeTypeDetail_Extended> _ListNodeTypeDetail_Extended = new List<NodeTypeDetail_Extended>();

			modelQuery = _RequestServiceClient.GetSchemaTable(_HierarchyNodeType.TableName, NodeId);

			string _valueField = string.Empty;

			foreach (NodeTypeDetail item in _ListNodeTypeDetail)
			{
				CellValueViewModel _RowViewModel = null;
				if (modelQuery.Rows.Count > 0)
				{
					_RowViewModel = modelQuery.Rows[0].Values.Where(e => e.ColumnName == item.FieldName).FirstOrDefault();
					_valueField = _RowViewModel.Value;
				}
				else
					_valueField = string.Empty;

				//Si tiene una categia, crea el select list con las opciones
				if (item.OptionsCategoryName != null)
				{
					var TheCategory = db.Categories.Where(n => n.Name == item.OptionsCategoryName && n.IsActive).Single();
					var TheOptions = db.Options.Where(c => c.Category_Uid == TheCategory.Uid && c.IsActive).OrderBy(e => e.Value).ToList();

					var modelData = TheOptions.Select(m => new SelectListItem()
					{
						Text = m.Value,
						Value = m.Uid.ToString(),
					});
					ViewData[item.FieldName] = modelData;
				}

				_ListNodeTypeDetail_Extended.Add(
					new NodeTypeDetail_Extended
					{
						FieldName = item.FieldName,
						FieldPrompt = item.FieldPrompt,
						FieldType = item.FieldType,
						HierarchyNodeType = item.HierarchyNodeType,
						IsRequired = item.IsRequired,
						MaxSize = item.MaxSize,
						MinSize = item.MinSize,
						NodeTypeDetailId = item.NodeTypeDetailId,
						OptionsCategoryName = item.OptionsCategoryName,
						PageFieldTypeId = item.PageFieldTypeId,
						SortOrder = item.SortOrder,
						Style = item.Style,
						ValueField = _valueField
					});
			}

			//if (_HierarchyNodeType != null)
			//  modelQuery = _RequestServiceClient.GetSchemaTable(_HierarchyNodeType.TableName, NodeId);

			return PartialView("_Atributes", _ListNodeTypeDetail_Extended);
		}


		[AcceptVerbs(HttpVerbs.Get)]
		public JsonResult GetNodeTypeByAplicationName(int AplicationNameid)
		{
			List<HierarchyNodeType> modelList;
			if (AplicationNameid != 0)
				modelList = db.HierarchyNodeTypes.Where(an => an.AplicationNameId == AplicationNameid).ToList();
			else
				modelList = db.HierarchyNodeTypes.ToList();

			var modelData = modelList.Select(m => new SelectListItem()
			{
				Text = m.NodeType,
				Value = m.Id.ToString(),

			});

			return Json(modelData, JsonRequestBehavior.AllowGet);
		}
	}
}