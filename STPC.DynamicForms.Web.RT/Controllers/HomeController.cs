using System.Web.Mvc;
using STPC.DynamicForms.Web.RT.Models;
using System.Linq;
using System;
using System.Globalization;
using STPC.DynamicForms.Web.RT.Helpers;
using System.Configuration;
using System.Web.Security;
using System.Collections.Generic;
using STPC.DynamicForms.Web.RT.Services.Entities;
using STPC.DynamicForms.Core;
using STPC.DynamicForms.Web.Common;
using System.Data;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.Common.Messages;
using System.Net;
using StackExchange.Redis;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;

namespace STPC.DynamicForms.Web.RT.Controllers
{
  public class Indicator
  {
    public string StoreProcedureSourse { get; set; }
    public int IndicarorType { get; set; }
  }
  [HandleError]
    
   
  public class HomeController : Controller
  {
    public AbcRedisCacheManager _AbcRedisCacheManager = new AbcRedisCacheManager();
    Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
    List<Hierarchy> listHierarchy = null;
    List<Hierarchy> listHierarchylistTemp = null;
    public List<MenuItemRole> listMenuItemRole = null;
    public List<MenuItem> listMenuItem = null;
    CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
    CustomRequestProvider requestProvider = new CustomRequestProvider();

    private Common.Services.Users.Role[] roles;


    public HomeController()
    {
      _stpcForms.IgnoreResourceNotFoundException = true;

    }
    [Authorize]
    public ActionResult Index()
    {
      try
      {
      
        Common.Services.Users.User currentUser = provider.GetUser(this.User.Identity.Name);
        if (!currentUser.IsOnLine || Session["Logged"] == null)
          return RedirectToAction("LogOff", "Account");

        if (this.User.Identity.IsAuthenticated)
        {
          GetHierarchyByCampaign();
          return View("Menu");
        }
        return RedirectToAction("LogOn", "Account");
      }

      catch (Exception ex)
      {
        return DefaultActionErrorHandling(ex, "Index");
      }
    }


    public ActionResult Menu()
    {
      try
      {
        Common.Services.Users.User currentUser = provider.GetUser(this.User.Identity.Name);
        if (currentUser != null && !currentUser.IsOnLine || Session["Logged"] == null)
          return RedirectToAction("LogOffIsOnline", "Account");


        GetHierarchyByCampaign();
        return View();
      }

      catch (Exception ex)
      {
        return DefaultActionErrorHandling(ex, "Menu");
      }
    }

    /// <summary>
    /// Menus the authorization.
    /// </summary>
    /// <param name="userModel">The user model.</param>
    /// <returns></returns>
    [Authorize]
    public ActionResult MenuAuth(LoginViewModel userModel)
    {
      try
      {
        Common.Services.Users.User currentUser = provider.GetUser(this.User.Identity.Name);

        if (!currentUser.IsOnLine || Session["Logged"] == null)
          return RedirectToAction("LogOff", "Account");
        Session["GivenName"] = currentUser.GivenName + " " + currentUser.LastName;
        Session["LastLoginDate"] = currentUser.LastLoginDate;
        GetHierarchyByCampaign();
        ViewBag.logoffUrl = ViewBag.logoffUrl;
        //ViewBag.userName = currentUser.GivenName + " " + currentUser.LastName;
        //ViewBag.LastLoginDate = currentUser.LastLoginDate;
        #region validar el rol para pre-cargar una opcion

        string ROLE_NAME_ESTUDIANTE = System.Configuration.ConfigurationManager.AppSettings["RolEstudiante"];
        string ROLE_NAME_PSICOLOGA = System.Configuration.ConfigurationManager.AppSettings["RolAuxiliar"];
        string ROLE_NAME_GESTOR = System.Configuration.ConfigurationManager.AppSettings["RolGestor"];
        string ROLE_NAME_ADMIN = System.Configuration.ConfigurationManager.AppSettings["RolAdmin"];


        ViewData["IsLoadFormDefault"] = "";//ViewData bandera que indica si debe cargar por defecto el formulario o no

        //Validate if exist cache data

        try
        {
          //List<string> listAplicationName = new List<string>();
          //listAplicationName.Add("SufiDev");
          //listAplicationName.Add("ExitoDev");
          //listAplicationName.Add("TestDev");
          //_AbcRedisCacheManager.SetCacheAplicationName(listAplicationName, "listAplicationName");
          ////_AbcRedisCacheManager.ClearCacheByKey("listPageEvent");

          //_AbcRedisCacheManager.ClearCacheByKey("listOptions_XpressDev");
          //_AbcRedisCacheManager.ClearCacheByKey("listOptionsXpressDev");
          //_AbcRedisCacheManager.Clear();
          //_AbcRedisCacheManager.updateCacheOptioneByInstance();
          //_AbcRedisCacheManager.updateCachePageEventsByInstance();


          //_AbcRedisCacheManager.ClearCacheByKey("listFields");
        }
        catch (Exception)
        {


        }

        /*if (_AbcRedisCacheManager["listPageEvent"].Equals(""))
{
  var serializedPageEvents = JsonConvert.SerializeObject(_stpcForms.PageEvent.ToList());
  _AbcRedisCacheManager["listPageEvent"] = serializedPageEvents;
}*/

        if (currentUser.Roles.Count() == 1)
        {
          #region ESTUDIANTE

          if (currentUser.Roles.Where(e => e.Rolename == ROLE_NAME_ESTUDIANTE).FirstOrDefault() != null)
          {
            List<Services.Entities.Request> _request = _stpcForms.Request.Where(e => e.CreatedBy == this.User.Identity.Name).ToList().OrderByDescending(e => e.Updated).ToList();

            if (_request.Count > 0)
            {

              ViewData["IsLoadFormDefault"] = "S";
              _request = _request.OrderByDescending(e => e.Updated).ToList();
              ViewData["PageId"] = _request[0].PageFlowId;
              ViewData["formid"] = _request[0].FormId;
              ViewData["RequestId"] = _request[0].RequestId;
              //return RedirectToAction("FormPageRespondView", "Form", new { FormId = _request.FirstOrDefault().FormId, requestId = _request.FirstOrDefault().RequestId });
              return View(userModel);
            }
            else
            {
              ViewBag.userModel = userModel;
              return View(userModel);
            }
          }

          #endregion ESTUDIANTE

          #region PSICOLOGA

          /**********************************

		  else if (theuser.Roles.Where(e => e.Rolename == ROLE_NAME_PSICOLOGA).FirstOrDefault() != null)
		  {
			 // obtiene el estado para la consulta
			 Guid WORK_FLOW_STATE = Guid.Parse(System.Configuration.ConfigurationManager.AppSettings["EstadoAuxiliar"]);

			 List<Services.Entities.Request> _request =
				_stpcForms.Request.Where(e => e.WorkFlowState.Equals(WORK_FLOW_STATE)).ToList().OrderByDescending(e => e.Updated).ToList();

			 if (_request.Count > 0)
			 {
				_request = _request.OrderByDescending(e => e.Updated).ToList();
				return RedirectToAction("MyRequest", "Request", new { FormId = _request.FirstOrDefault().FormId, requestId = _request.FirstOrDefault().RequestId });
			 }
			 else
			 {
				ViewBag.userModel = userModel;
				return View(userModel);
			 }
		  }

		  **********************************/

          #endregion PSICOLOGA

          #region GESTOR

          /**********************************

		  else if (theuser.Roles.Where(e => e.Rolename == ROLE_NAME_GESTOR).FirstOrDefault() != null)
		  {
			 // obtiene el estado para la consulta
			 Guid WORK_FLOW_STATE = Guid.Parse(System.Configuration.ConfigurationManager.AppSettings["EstadoGestor"]);

			 List<Services.Entities.Request> _request =
				_stpcForms.Request.Where(e => e.WorkFlowState.Equals(WORK_FLOW_STATE)).ToList().OrderByDescending(e => e.Updated).ToList();

			 if (_request.Count > 0)
			 {
				_request = _request.OrderByDescending(e => e.Updated).ToList();
				return RedirectToAction("FormPageRespondView", "Form", new { FormId = _request.FirstOrDefault().FormId, requestId = _request.FirstOrDefault().RequestId });
			 }
			 else
			 {
				ViewBag.userModel = userModel;
				return View(userModel);
			 }
		  }
			 **********************************/

          #endregion GESTOR

          else
          {
            ViewBag.userModel = userModel;
            return View(userModel);
          }
        }

        #endregion validar el rol para pre-cargar una opcion


        else
        {
          ViewBag.userModel = userModel;
          return View(userModel);
        }



        //Valida si es necesario alertar al usuario que está pronta la expiración de su contraseña

      }
      catch (Exception ex)
      {
        DefaultErrorHandling(ex, "MenuAuth");
        ViewBag.userModel = userModel;
        return View(userModel);
      }
    }

    public ActionResult FormsMenu()
    {
      ViewBag.ViewLinkMenu = System.Configuration.ConfigurationManager.AppSettings["ViewLinkMenu"];

      var allForms = from items in _stpcForms.Forms.Expand("Pages")
                     select items;
      return PartialView("FormsMenu", allForms);
    }

    public List<MenuItem> getMenuItems(MenuItem root, STPC.DynamicForms.Web.Common.Services.Users.Role[] roleList = null, List<MenuItemRole> menuItemRoleList = null)
    {

      List<MenuItem> itemList = new List<MenuItem>();
      if (listMenuItemRole == null) listMenuItemRole = _stpcForms.MenuItemRole.ToList();

      if (listMenuItem == null) listMenuItem = _stpcForms.MenuItem.ToList();

      Guid? rootUid = null;
      if (root != null) rootUid = root.Uid;


      if (roleList == null)
      {
        Common.Services.Users.User currentUser = provider.GetUser(User.Identity.Name);
        roleList = currentUser.Roles;

      }

      if (menuItemRoleList == null)
      {
        menuItemRoleList = new List<MenuItemRole>();

        foreach (var rol in roleList)
        {

          var itemRoleList = listMenuItemRole.Where(itemRole => itemRole.RoleName.Equals(rol.Rolename)).ToList();

          foreach (var itemRole in itemRoleList)
          {
            if (menuItemRoleList.Find(mir => mir.Uid == itemRole.Uid) == null) menuItemRoleList.Add(itemRole);
          }
        }
      }



      var menuList = listMenuItem.Where(mi => mi.ParentMenuItemUid == rootUid).ToList();

      foreach (MenuItem item in menuList)
      {
        List<MenuItem> childItemList = getMenuItems(item, roleList, menuItemRoleList);

        item.Childs = new System.Collections.ObjectModel.Collection<MenuItem>();
        foreach (MenuItem child in childItemList)
        {

          item.Childs.Add(child);
        }


        //if the item was added before
        MenuItem eItem = itemList.Find(i => i.Uid == item.Uid);
        if (eItem != null)
        {
          foreach (MenuItem child in childItemList)
          {
            if (eItem.Childs.FirstOrDefault(i => i.Uid == item.Uid) == null) eItem.Childs.Add(child);

          }
        }
        else
        {
          if (menuItemRoleList.Find(mir => mir.MenuItemUid == item.Uid) != null) itemList.Add(item);
        }

      }

      return itemList;
    }

    public ActionResult FormsMenuAuth(LoginViewModel userModel)
    {
      ViewBag.userModel = userModel;
      // variable que configura los items como link
      ViewBag.ViewLinkMenu = System.Configuration.ConfigurationManager.AppSettings["ViewLinkMenu"];
      List<MenuItem> itemList = getMenuItems(null);

      return PartialView(itemList.OrderBy(item => item.DisplayOrder));
    }

    [HandleError(View = "_ErrorDetail")]
    public ActionResult ThrowException()
    {
      throw new ApplicationException();
    }

    public void GetHierarchyByCampaign()
    {

      STPC.DynamicForms.Web.Common.Services.Users.User currentUser = provider.GetUser(User.Identity.Name);

      int HierarchiesId = currentUser.Hierarchy.Id;
      listHierarchy = _stpcForms.Hierarchies.Expand("Children").ToList();
      Hierarchy userHierarchy = listHierarchy.Where(e => e.Id == currentUser.Hierarchy.Id).FirstOrDefault();



      List<AdCampaign> list_AdCampaign = GetCampaign(currentUser.Hierarchy.Id);
      List<AdCampaign> list_AdCampaignTemp = new List<AdCampaign>();

      list_AdCampaignTemp = list_AdCampaign.Where(e => e.HierarchyByCampaign.Contains(userHierarchy)).ToList();
      ViewBag.listCampanas = list_AdCampaignTemp;

      Session["AdCampaign"] = list_AdCampaignTemp;

      if (list_AdCampaignTemp != null)
        Session["counListCampaign"] = list_AdCampaignTemp.Count();
      else
        Session["counListCampaign"] = 0;

    }


    public JsonResult GetItemOfChildSelect(string ChildControl, string Value)
    {

      try
      {
        string[] arrayGuid = Value.Split(',');

        ChildControl = ChildControl.ToString().Replace("STPC_DFi_", string.Empty);

        PageField _pageField = _stpcForms.PageFields.Expand("FormFieldType").Where(e => e.Uid == Guid.Parse(ChildControl)).FirstOrDefault();

        var TheOptions = from x in _stpcForms.Options.ToList() where arrayGuid.Contains(x.Option_Uid_Parent.ToString()) && x.Category_Uid == Convert.ToInt32(_pageField.OptionsCategoryName) select x;

        ViewBag.typeControl = _pageField.FormFieldType.FieldType == "RadioList" ? "Radio" : "Checked";

        var modelData = TheOptions.Select(m => new StructControl()
        {
          Text = m.Value,
          Value = m.Uid.ToString(),
          Type = (_pageField.FormFieldType.FieldType == "RadioList" ? "Radio" : "checkbox")

        });
        return Json(modelData, JsonRequestBehavior.AllowGet);
      }
      catch
      {
        return this.Json(string.Empty);
      }
    }

    private List<AdCampaign> GetCampaign(int id)
    {
      List<AdCampaign> _ListCampaign = _stpcForms.AdCampaign.Expand("Hierarchy").Where(e => e.BeginDate <= DateTime.Now && e.EndDate >= DateTime.Now).ToList();
      listHierarchylistTemp = new List<Hierarchy>();
      foreach (var item in _ListCampaign)
      {
        item.HierarchyByCampaign.Add(item.Hierarchy);

        if (item.ApplyToChilds)
          if (item.Hierarchy != null)
            GetHierarchyChild(item.Hierarchy.Id, item);
      }
      return _ListCampaign;
    }

    private void GetHierarchyChild(int idHierarchy, AdCampaign Campaign)
    {
      Hierarchy childHierarchy = new Hierarchy();


      childHierarchy = listHierarchy.Where(e => e.Id == idHierarchy).FirstOrDefault();

      foreach (var item in childHierarchy.Children)
      {
        listHierarchylistTemp.Add(item);
        Campaign.HierarchyByCampaign.Add(item);
        GetHierarchyChild(item.Id, Campaign);
      }

    }



    public JsonResult GetDaysToExpirePassword()
    {

      try
      {
        STPC.DynamicForms.Web.Common.Services.Users.User currentUser = provider.GetUser(User.Identity.Name);
        return this.Json(provider.EsxpirarDaysPassword(currentUser));
      }
      catch
      {
        return this.Json(string.Empty);
      }
    }


    [HttpPost]
    [ValidateAntiForgeryTokenAttribute]
    public JsonResult GetChartData(string sp)
    {

      MyViewModel _DataTable = new MyViewModel();
      List<object> data = new List<object>();
      CustomRequestProvider _CustomRequestProvider = new CustomRequestProvider();
      string[] title = null;
      string[] cols = null;
      string[] rows = null;
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("@UserName", this.User.Identity.Name);
      _DataTable = _CustomRequestProvider.GetIndicatorByProcedure(sp, parameters);


      if (_DataTable != null)
      {
        if (_DataTable.Rows.Count > 0)
        {
          //La primera fila si existe, es el título de la gráfica
          title = new string[_DataTable.Rows[0].Values.Count];
          int countValues = 0;
          foreach (var item in _DataTable.Rows[0].Values)
          {
            title[countValues] = item.Value;
            countValues++;
          }
        }
        if (_DataTable.Rows.Count > 1)
        {
          //Crea array con las columnas de la gráfica
          cols = new string[_DataTable.Rows[1].Values.Count];

          int countColumns = 0;
          foreach (var item in _DataTable.Rows[1].Values)
          {
            cols[countColumns] = item.Value;
            countColumns++;
          }
        }
        data.Add(title);
        data.Add(cols);

        if (_DataTable.Rows.Count > 2)
        {
          //Crea array con las columnas de la gráfica
          for (int x = 2; x < _DataTable.Rows.Count; x++)
          {
            rows = new string[_DataTable.Rows[x].Values.Count];

            int countRows = 0;
            foreach (var item in _DataTable.Rows[x].Values)
            {
              rows[countRows] = item.Value;
              countRows++;
            }
            data.Add(rows);
          }

        }
      }



      //data.Add(new[] { "Task", "Hours per Day" });
      //data.Add(new[] { "Rechazado", "4","20" });
      //data.Add(new[] { "Aprobadas", "75", "15" });
      //data.Add(new[] { "Inconsistentes", "24", "30" });
      JsonResult jsonResult = Json(data);
      return Json(data);


      //List<object> data = new List<object>();
      //data.Add(new[] { "Year", "Sales", "Expenses","Other","cualquier" });
      //data.Add(new[] { "2004", "1000", "400","250","1950" });
      //data.Add(new[] { "2005", "1170", "460", "550", "900" });
      //data.Add(new[] { "2006", "660", "1120", "450", "1400" });
      //data.Add(new[] { "2007", "1030", "540", "350", "1500" });

      //return Json(data);



    }


    [HttpPost]
    [ValidateAntiForgeryTokenAttribute]
    public JsonResult GetIndicators()
    {
      //TODO: Review performance and optimization
      List<PerformanceIndicator> listPerformanceIndicatorbyRol = new List<PerformanceIndicator>();
      try
      {
        STPC.DynamicForms.Web.Common.Services.Users.User currentUser = provider.GetUser(User.Identity.Name);
        roles = currentUser.Roles;//Roles.GetRolesForUser(this.User.Identity.Name).ToList();

        for (int i = 0; i < roles.Length; i++)
        {

          List<PerformanceIndicator> listPerformanceIndicator = _stpcForms.PerformanceIndicators.Where(e => e.Enabled == true && e.Role.Rolename == roles[i].Rolename.ToString()).OrderBy(e => e.Id).ToList();
          listPerformanceIndicatorbyRol.AddRange(listPerformanceIndicator);
        }

        return Json(listPerformanceIndicatorbyRol);
      }
      catch (Exception ex)
      {
        DefaultErrorHandling(ex, "GetIndicators");
        return Json(listPerformanceIndicatorbyRol);
      }

    }


    private void DefaultErrorHandling(Exception ex, string triggerAction)
    {
      bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
      Guid correlationID = Guid.NewGuid();

      ILogging eventWriter = LoggingFactory.GetInstance();
      string errorMessage = string.Format(CustomMessages.E0007, "HomeController", triggerAction, correlationID, ex.Message);
      System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
      eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

    }

    private ActionResult DefaultActionErrorHandling(Exception ex, string triggerAction)
    {
      bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
      Guid correlationID = Guid.NewGuid();

      ILogging eventWriter = LoggingFactory.GetInstance();
      string errorMessage = string.Format(CustomMessages.E0007, "HomeController", triggerAction, correlationID, ex.Message);
      System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
      eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
      if (ShowErrorDetail)
      {
        return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(errorMessage), "HomeController", triggerAction));
      }
      else
      {
        return PartialView("_ErrorGeneral", string.Format(CustomMessages.E0001, correlationID.ToString()));
      }
    }


  }
  

}
