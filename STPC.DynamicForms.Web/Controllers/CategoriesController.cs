using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using STPC.DynamicForms.Web.Services.Entities;
using System.Configuration;

namespace STPC.DynamicForms.Web.Controllers
{
    public class CategoriesController : Controller
    {
        STPC_FormsFormEntities db = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

        [Authorize(Roles = "Administrador, Co-Administrador")]
        public ActionResult List()
        {
            var model = new Models.CategoriesViewModel();
            model.Categories = db.Categories.ToList();
            ViewBag.Category = model.Categories;
            return View(model);
        }

        public ActionResult Options(string categoryname = "")
        {
            db.IgnoreResourceNotFoundException = true;
            System.Data.Services.Client.DataServiceQuery<Category> query =
                 db.Categories.Expand("Options");
            var category = query.Where(n => n.Name == categoryname);
            return PartialView("_Options", category.FirstOrDefault());

        }
        [HttpPost]
        [Authorize(Roles = "Administrador, Co-Administrador")]
        public ActionResult AddCategory(string Name, string Dependency)
        {
            if (string.IsNullOrEmpty(Dependency))
                Dependency = "0";

            var NewCategory = new Category { Name = Name, Dependency = Convert.ToInt32(Dependency) };

            ViewBag.Category = db.Categories.ToList();

            db.AddToCategories(NewCategory);
            db.SaveChanges();
            return Json(new { Success = true, name = NewCategory.Name });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Co-Administrador")]
        public ActionResult DeleteCategory(string Name)
        {
            var TheCategory = db.Categories.Where(cn => cn.Name == Name).FirstOrDefault();
            if (TheCategory != null)
            {
                db.DeleteObject(TheCategory);
                db.SaveChanges();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Co-Administrador")]
        public ActionResult AddOption(string CategoryName, string OptionKey, string OptionValue, int idOptionParent = -1)
        {
            var TheCategory = db.Categories.Where(n => n.Name == CategoryName).Single();
            var TheNewOption = new Option { Key = OptionKey, Value = OptionValue, Category_Uid = TheCategory.Uid, Option_Uid_Parent = idOptionParent };
            db.AddRelatedObject(TheCategory, "Options", TheNewOption);
            db.SaveChanges();
            return Json(new { Success = true, id = CategoryName, key = TheNewOption.Key, value = TheNewOption.Value });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Co-Administrador")]
        public ActionResult RemoveOption(string OptionKey, string CategoryName)
        {
            var TheCategory = db.Categories.Where(n => n.Name == CategoryName).Single();

            var TheOption = db.Options.Where(cn => cn.Category_Uid == TheCategory.Uid).Where(k => k.Key == OptionKey).FirstOrDefault();
            if (TheOption != null)
            {
                db.DeleteObject(TheOption);
                db.SaveChanges();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [Authorize(Roles = "Administrador, Co-Administrador")]
        public JsonResult LoadCategories()
        {
            var modelList = db.Categories.ToList();

            var modelData = modelList.Select(m => new SelectListItem()
            {
                Text = m.Name,
                Value = m.Uid.ToString(),

            });

            return Json(modelData, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [Authorize(Roles = "Administrador, Co-Administrador")]
        public JsonResult LoadOptionsByIdOption(int IdCategoria)
        {
            var modelList = db.Options.Where(e => e.Category_Uid == IdCategoria).ToList();

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
            Option _EditOption = db.Options.Where(e => e.Uid == id).FirstOrDefault();
            return PartialView(_EditOption);
        }

        [HttpPost]
        public ActionResult Edit(Option model, FormCollection par)
        {
            Option _Option = db.Options.Where(e => e.Uid == model.Uid).FirstOrDefault();

            _Option.Key = model.Key;
            _Option.Value = model.Value;

            db.UpdateObject(_Option);
            db.SaveChanges();

            return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);

        }

    }
}