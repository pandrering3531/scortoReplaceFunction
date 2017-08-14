using System;
using System.Web.Security;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using STPC.DynamicForms.Web.Services.Entities;
using System.Configuration;
using STPC.DynamicForms.Web.Common;

namespace STPC.DynamicForms.Web.Controllers
{
    public class PerformanceIndicatorController : Controller
    {

        CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
        Services.Entities.STPC_FormsFormEntities _stpcForms = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

        //
        // GET: /PerformanceIndicator/

        [Authorize]
        public ActionResult List()
        {
            LoadData();
            List<PerformanceIndicator> performIndicatorList = _stpcForms.PerformanceIndicators.Expand("Role").ToList();
            return PartialView("Index", performIndicatorList);
        }



        [HttpPost]
        public ActionResult GetIndex(PerformanceIndicator model, FormCollection par)
        {
            List<PerformanceIndicator> performIndicator = _stpcForms.PerformanceIndicators.Expand("Role").ToList();
            return PartialView("_Index", performIndicator);
        }

     
        //
        // GET: /PerformanceIndicator/Create

        public ActionResult Create()
        {
            LoadData();
            ViewBag.roles = _stpcForms.Roles.ToList();
            PerformanceIndicator perIndicator = new PerformanceIndicator();
            perIndicator.LastModifiedBy = Guid.Parse(Membership.GetUser().ProviderUserKey.ToString());

            return PartialView(perIndicator);
        }

        //
        // POST: /PerformanceIndicator/Create

        [HttpPost]
        public ActionResult Create(PerformanceIndicator model, FormCollection collection)
        {
            _stpcForms.AddToPerformanceIndicators(model);
            model.Modified = DateTime.Now;
            if (collection["ApplyBy"] != null) selectAssignment(ref model, collection["ApplyBy"]);

            _stpcForms.SaveChanges();
            return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
        }

        private void selectAssignment(ref PerformanceIndicator model, string selectedAssignment)
        {
            if (selectedAssignment == "R")
            {
                model.Hierarchy = null;
                //model.user = null;
            };
            if (selectedAssignment == "U")
            {
                model.Hierarchy = null;
                model.Role = null;

            }
            if (selectedAssignment == "H")
            {
                model.Role = null;
                //model.user = null;      
            }
        }

        //
        // GET: /PerformanceIndicator/Edit/5

        public ActionResult Edit(int id)
        {
            PerformanceIndicator _performanceIndicator = _stpcForms.PerformanceIndicators.Expand("Role").Where(e => e.Id == id).FirstOrDefault();
            LoadData();

            //TODO: Optimize this
            foreach (Role item in ViewBag.roles)
            {
                PerformanceIndicator TempPerformanceIndicator = _stpcForms.PerformanceIndicators.Expand("Role").Where(e => e.Id == id && e.Role.Rolename == item.Rolename).FirstOrDefault();

                if (TempPerformanceIndicator != null)
                {
                    _performanceIndicator.Role = item;
                    break;
                }
            }

            foreach (Hierarchy item in ViewBag.Hierarchy)
            {
                PerformanceIndicator TempPerformanceIndicator = _stpcForms.PerformanceIndicators.Where(e => e.Id == id && e.Hierarchy.Id == item.Id).FirstOrDefault();

                if (TempPerformanceIndicator != null)
                {
                    _performanceIndicator.Hierarchy = item;
                    break;
                }
            }


            //TODO:allocation by user

            return PartialView(_performanceIndicator);
        }

        //
        // POST: /PerformanceIndicator/Edit/5

        [HttpPost]
        public ActionResult Edit(PerformanceIndicator model, FormCollection collection)
        {
            PerformanceIndicator _performanceIndicator = _stpcForms.PerformanceIndicators.Where(e => e.Id == model.Id).FirstOrDefault();

            if (collection["ApplyBy"] != null) selectAssignment(ref model, collection["ApplyBy"]);

            _performanceIndicator.IndicatorType = model.IndicatorType;
            _performanceIndicator.Modified = DateTime.Now;
            _performanceIndicator.Source = model.Source;
            _performanceIndicator.ViolationMaxvalue = model.ViolationMaxvalue;
            _performanceIndicator.ViolationMinvalue = model.ViolationMinvalue;
            _performanceIndicator.WarningMaxValue = model.WarningMaxValue;
            _performanceIndicator.WarningMinValue = model.WarningMinValue;
            _performanceIndicator.Hierarchy = model.Hierarchy;
            _performanceIndicator.Role = model.Role;
            _performanceIndicator.Enabled = model.Enabled;
            _performanceIndicator.LastModifiedBy = Guid.Parse(Membership.GetUser().ProviderUserKey.ToString());
            _stpcForms.UpdateObject(_performanceIndicator);

            _stpcForms.SaveChanges(System.Data.Services.Client.SaveChangesOptions.Batch);

            return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                PerformanceIndicator _performanceIndicator = _stpcForms.PerformanceIndicators.Where(e => e.Id == id).FirstOrDefault();

                if (_performanceIndicator != null)
                {
                    _stpcForms.DeleteObject(_performanceIndicator);
                    _stpcForms.SaveChanges();
                }
                return Json(new { Success = true });
            }
            catch
            {
                return Json(new { Success = true });
            }
        }

    
        private void LoadData()
        {
            ViewBag.roles = _stpcForms.Roles.ToList();
            ViewBag.Users = provider.GetAllUsers();
            ViewBag.Hierarchy = _stpcForms.Hierarchies.ToList();
        }
    }
}
