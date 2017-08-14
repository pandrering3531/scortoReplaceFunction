using Newtonsoft.Json;
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
    
    
	public class RedisCacheController : Controller
	{
		//
		// GET: /RedisCache/
		public AbcRedisCacheManager _AbcRedisCacheManager = new AbcRedisCacheManager();
		Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
		Services.Entities.STPC_FormsFormEntities _stpcFormsPage = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

		[Authorize(Roles = "Administrador")]
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public ActionResult getListOptionsFromRedis()
		{
			List<Option> listOptions = JsonConvert.DeserializeObject<List<Option>>(_AbcRedisCacheManager["listOptions"]);
			return PartialView("_list_data_by_object", listOptions.OrderBy(o => o.Category.Name).ToList());
		}

		[HttpPost]
		public ActionResult getListPageEventsFromRedis()
		{
			List<PageEvent> listOptions = JsonConvert.DeserializeObject<List<PageEvent>>(_AbcRedisCacheManager["listPageEvent"]);





			return PartialView("_list_data_by_pageEvents", listOptions);
		}


		[HttpPost]
		public ActionResult getListPageMathOperationsFromRedis()
		{
			List<PageMathOperation> listOptions = JsonConvert.DeserializeObject<List<PageMathOperation>>(_AbcRedisCacheManager["listPageMathOperation"]);

			return PartialView("_list_data_by_pageMathOperations", listOptions);
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public JsonResult UpdateCache()
		{
			try
			{
				//Actualiza caché de categorias
				_AbcRedisCacheManager.updateCacheOptioneByInstance();

				//Actualiza caché de eventos
				_AbcRedisCacheManager.updateCachePageEventsByInstance();

				//Actualiza cache de operaciones matemáticas
				_AbcRedisCacheManager.updateCachePageMathOperationByInstance();
				return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
			}
			catch (Exception)
			{

				return Json(JsonResponseFactory.ErrorResponse(), JsonRequestBehavior.AllowGet);
			}
			
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public JsonResult UpdateCacheEvents()
		{
			_AbcRedisCacheManager.updateCachePageEventsByInstance();
			return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
		}


		[AcceptVerbs(HttpVerbs.Post)]
		public JsonResult UpdateCachePageMathOperations()
		{
			_AbcRedisCacheManager.updateCachePageMathOperationByInstance();
			return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
		}

	}
}
