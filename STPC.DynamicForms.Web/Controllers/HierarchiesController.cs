using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using STPC.DynamicForms.Core;
using STPC.DynamicForms.Web.Helpers;
using STPC.DynamicForms.Web.Services.Entities;

namespace STPC.DynamicForms.Web.Controllers
{
	public class HierarchiesController : Controller
	{
		Services.Entities.STPC_FormsFormEntities db = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
		CustomRequestProvider _RequestServiceClient = new CustomRequestProvider();

		[Authorize(Roles = "Administrador, Co-Administrador")]
		public ActionResult List()
		{
			ViewBag.HierarchyNodeType = db.HierarchyNodeTypes.ToList();

			var model = db.Hierarchies.Expand("Parent").ToList();
			return View(model);
		}

		[HttpPost]
		public ActionResult GetIndex()
		{
			ViewBag.HierarchyNodeType = db.HierarchyNodeTypes.ToList();

			var model = db.Hierarchies.Expand("Parent").ToList();
			return PartialView("_List", model);
		}


		[AcceptVerbs(HttpVerbs.Get)]
		[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
		[Authorize(Roles = "Administrador, Co-Administrador")]
		public JsonResult GetHierarchiesForLevel(string level)
		{
			var modelList = db.Hierarchies.Expand("Parent").Where(r => r.Level == level).OrderBy(z => z.Name);
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
		[Authorize(Roles = "Administrador, Co-Administrador")]
		public JsonResult DeleteHierarchy(int id)
		{
			DeleteCascadeHierarchy(id);
			db.SaveChanges();
			return Json(new { Success = true });
		}

		[HttpPost]
		[Authorize(Roles = "Administrador, Co-Administrador")]
		public JsonResult AddChild(string name, int parentId, string LevelName, int nodeType)
		{
			var theparent = db.Hierarchies.Where(i => i.Id == parentId).FirstOrDefault();
			if (theparent != null)
			{
				var thechild = new STPC.DynamicForms.Web.Services.Entities.Hierarchy()
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



			return Json(new { NodeName = TheHierarchy.Name, ChildLevelName = childlevelname, NodeType = TheHierarchy.NodeType }, JsonRequestBehavior.AllowGet);

		}

		[HttpPost]
		[Authorize(Roles = "Administrador, Co-Administrador")]
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
			var TheHierarchy = db.Hierarchies.Expand("Children").Where(i => i.Id == id).FirstOrDefault();
			foreach (var child in TheHierarchy.Children)
			{
				DeleteCascadeHierarchy(child.Id);
			}
			db.DeleteObject(TheHierarchy);
		}

		public ActionResult Create(int parentId)
		{

			ViewBag.HierarchyNodeType = db.HierarchyNodeTypes.ToList();
			Hierarchy _Hierarchy = new Hierarchy();

			var TheHierarchy = db.Hierarchies.Expand("Children").Where(i => i.Id == parentId).FirstOrDefault();
			if (TheHierarchy == null) return null;

			if (TheHierarchy.Children.Count > 0)
			{
				_Hierarchy.Level = TheHierarchy.Children.First().Level;
			}
			else
				_Hierarchy.Level = TheHierarchy.Level;


			var theparent = db.Hierarchies.Where(i => i.Id == parentId).FirstOrDefault();
			if (theparent != null)
			{
				_Hierarchy.Parent = new Hierarchy();
				_Hierarchy.Parent = theparent;
			}

			return PartialView(_Hierarchy);
		}

		public ActionResult Edit(int NodeId)
		{
			ViewBag.HierarchyNodeType = db.HierarchyNodeTypes.ToList();
			var TheHierarchy = db.Hierarchies.Where(i => i.Id == NodeId).FirstOrDefault();

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
							var TheCategory = db.Categories.Where(n => n.Name == item.OptionsCategoryName).Single();
							var TheOptions = db.Options.Where(c => c.Category_Uid == TheCategory.Uid).OrderBy(e => e.Value).ToList();

							var modelData = TheOptions.Select(m => new SelectListItem()
							{
								Text = m.Value,
								Value = m.Key,
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
		[Authorize(Roles = "Administrador, Co-Administrador")]
		public ActionResult Create(Hierarchy model, FormCollection par)
		{
			var theparent = db.Hierarchies.Where(i => i.Id == model.Parent.Id).FirstOrDefault();


			db.AddRelatedObject(theparent, "Children", model);
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
		[Authorize(Roles = "Administrador, Co-Administrador")]
		public ActionResult Edit(Hierarchy model, FormCollection par)
		{
			var theNode = db.Hierarchies.Where(i => i.Id == model.Id).FirstOrDefault();
			theNode.Name = model.Name;
			theNode.NodeType = model.NodeType;
			db.UpdateObject(theNode);
			db.SaveChanges();

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

		[HttpPost]
		public ActionResult GetSchemaTable(int NodeType, int NodeId)
		{
			MyViewModel modelQuery = new MyViewModel();
			HierarchyNodeType _HierarchyNodeType = db.HierarchyNodeTypes.Where(e => e.Id == NodeType).FirstOrDefault();
			List<NodeTypeDetail> _ListNodeTypeDetail = db.NodeTypeDetail.Expand("FieldType").Where(e => e.HierarchyNodeType.Id == NodeType).OrderBy(e=>e.SortOrder).ToList();

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
					var TheCategory = db.Categories.Where(n => n.Name == item.OptionsCategoryName).Single();
					var TheOptions = db.Options.Where(c => c.Category_Uid == TheCategory.Uid).OrderBy(e => e.Value).ToList();

					var modelData = TheOptions.Select(m => new SelectListItem()
					{
						Text = m.Value,
						Value = m.Key,
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
	}
}