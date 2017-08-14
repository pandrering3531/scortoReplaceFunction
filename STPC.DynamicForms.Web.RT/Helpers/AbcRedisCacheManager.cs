using Newtonsoft.Json;
using StackExchange.Redis;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.RT.Services.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace STPC.DynamicForms.Web.RT.Helpers
{
	public class AbcRedisCacheManager
	{
		//ConnectionMultiplexer connection = ConnectionMultiplexer.Connect("syncTimeout=5000,sufiabc.redis.cache.windows.net,ssl=true,password=zmmKeYOI/tbLoTGd4b9/EPqEitiNUY2/UYdWqAIl0Pc=");

		public static ConnectionMultiplexer connection;// = ConnectionMultiplexer.Connect(System.Configuration.ConfigurationManager.AppSettings["RedisConfigurationOption"].ToString());


		Services.Entities.STPC_FormsFormEntities _stpcForms;
		String jsonResult = string.Empty;
		String jsonResultNotCache = string.Empty;
		public List<PageEvent> ListPageEventInCache;


		public AbcRedisCacheManager()
		{
			_stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
			_stpcForms.IgnoreResourceNotFoundException = true;
		}

		public string this[string key]
		{
			get
			{
				try
				{
					IDatabase db = null;
					if (connection == null)
					{
						setConection();

					}
					db = connection.GetDatabase();
					string keyApp = key + System.Configuration.ConfigurationManager.AppSettings["AplicationName"];
					jsonResult = db.StringGet(keyApp);


					if (jsonResult == null)
					{
						return SetCacheByKey(key);
						//Retorna json de la base de datos
					}
					return jsonResult;

				}
				catch (Exception ex)
				{

					WriteLogError(ex);
					return SetCacheByKey(key);
				}

			}
			set
			{

				if (connection != null)
				{
					IDatabase db = connection.GetDatabase();

					db.StringSet(key, value);

                    string keyApp = key + System.Configuration.ConfigurationManager.AppSettings["AplicationName"];
                    jsonResult = db.StringGet(keyApp);


                    if (jsonResult == null)
                    {
                       
                        //Retorna json de la base de datos
                    }
				}
			}
		}

		private void setConection()
		{
			if (connection == null)
				connection = ConnectionMultiplexer.Connect(System.Configuration.ConfigurationManager.AppSettings["RedisConfigurationOption"].ToString());
		}


		public void ClearCacheByKey(string key)
		{
			try
			{
				setConection();
				IDatabase db = connection.GetDatabase();
				db.KeyDelete(key);
			}
			catch (Exception ex)
			{
				WriteLogError(ex);

			}

		}

		private string SetCacheByKey(string key)
		{
			_stpcForms.MergeOption = System.Data.Services.Client.MergeOption.PreserveChanges;
			if (key.Equals("listPageEvent"))
			{
				List<PageEvent> listtest = _stpcForms.PageEvent.Expand("FormPage").ToList();

				var serializedPageEvents = JsonConvert.SerializeObject(listtest);
				return serializedPageEvents;
				//this["listPageEvent"] = serializedPageEvents;
			}
			if (key.Equals("listCategories"))
			{
				var serializedPageEvents = JsonConvert.SerializeObject(_stpcForms.Categories.ToList());
				return serializedPageEvents;
			}
			if (key.Equals("listOptions"))
			{
                var serializedPageEvents = JsonConvert.SerializeObject(_stpcForms.Options.Expand("Category").ToList());
				return serializedPageEvents;
			}
			if (key.Equals("listFields"))
			{
				var serializedPageEvents = JsonConvert.SerializeObject(_stpcForms.PageFields.Expand("FormFieldType").ToList());
				return serializedPageEvents;
			}
			if (key.Equals("listPageMathOperation"))
			{
				var serializedPageEvents = JsonConvert.SerializeObject(_stpcForms.PageMathOperation.ToList());
				return serializedPageEvents;
			}
			return string.Empty;
		}

		private IDatabase CreateConection()
		{
			IDatabase db = null;
			if (connection == null)
			{
				setConection();
			}

			db = connection.GetDatabase();
			return db;
		}

		private void SetCachePageEvents<T>(List<T> list, string key) where T : class
		{

			var serializedPageEvents = JsonConvert.SerializeObject(list);
			jsonResultNotCache = serializedPageEvents;
			this[key] = serializedPageEvents;
		}

		private void SerializeJsonCache<T>(string key, IDatabase db, List<T> ListPageEvent) where T : class
		{
			var serializedPageEvents = JsonConvert.SerializeObject(ListPageEvent);//Serializo a JSon la lista total de eventos
			ClearCacheByKey(key);
			db.StringSet(key, serializedPageEvents);//actualizo el cache
		}

		public void Create<T>(T _object, bool isEdit, bool isDelete, string key) where T : class
		{
			try
			{
				IDatabase db = CreateConection();
				string keyApp;
				string aplicationName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

				keyApp = key + aplicationName;
				jsonResult = db.StringGet(key + aplicationName);//Cargo el cache PageEvents

				List<T> ListGeneric = JsonConvert.DeserializeObject<List<T>>(this[key]);//Serializo la lista de eventos en cache


				string propertyValue = _object.GetType().GetProperty("Uid").GetValue(_object, null).ToString();

				T _ObjectToRemove = ListGeneric.Where(m => { return m.GetType().GetProperty("Uid").GetValue(m, null).ToString().StartsWith(propertyValue); }).FirstOrDefault();

				//PageEvent _PageEventToRemove = ListPageEvent.Where(e => e.Uid == _PageEvent.Uid).FirstOrDefault();

				if (_ObjectToRemove != null)
					ListGeneric.Remove(_ObjectToRemove);

				if (!isDelete)
					ListGeneric.Add(_object);


				SerializeJsonCache(keyApp, db, ListGeneric);
			}
			catch (Exception err)
			{

				WriteLogError(err);
			}
		}

		private List<T> Deserialize<T>(IDatabase db, List<T> ListObject, string key) where T : class
		{
			jsonResult = db.StringGet(key);//Cargo el cache PageEvents

			ListObject = JsonConvert.DeserializeObject<List<T>>(this[key]);//Serializo la lista de eventos en cache
			//return ListPageEvent;
			return ListObject;
		}

		private bool CompareCache<T>(List<T> ListPageEventInDB, string key, ref List<T> listPageEventInCache) where T : class
		{

			IDatabase db = null;

			if (connection == null)
			{
				setConection();
			}

			db = connection.GetDatabase();

			listPageEventInCache = Deserialize<T>(db, ListPageEventInDB, key);


			if (listPageEventInCache == null)
			{
				listPageEventInCache = new List<T>();
				return false;
			}
			return listPageEventInCache.Count == ListPageEventInDB.Count ? true : false;

		}

		private static void WriteLogError(Exception ex)
		{
			string errorMessage;
			ILogging eventWriter = LoggingFactory.GetInstance();
			bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
			Guid correlationID = Guid.NewGuid();
			errorMessage = string.Format(ex.Message, "AbcRedisCacheManager", "CompareCache", correlationID, ex.Message);
			System.Diagnostics.Debug.WriteLine("Exception: " + errorMessage);
			eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
		}

		public void UpdateCache<T>(List<T> ListPageEventInDB, string key) where T : class
		{
			try
			{
				string keyApp = key + System.Configuration.ConfigurationManager.AppSettings["AplicationName"];


				List<T> listCache = new List<T>();
				if (!CompareCache(ListPageEventInDB, keyApp, ref listCache))
				{

					foreach (var item in ListPageEventInDB)
					{
						string propertyValue = item.GetType().GetProperty("Uid").GetValue(item, null).ToString();
						T response = listCache.Where(m => { return m.GetType().GetProperty("Uid").GetValue(m, null).ToString().Equals(propertyValue); }).FirstOrDefault();

                        if (response == null)
                        {
                            listCache.Add(item);
                        }
                        else {
                            int a = 0;
                        }
					}

					SetCachePageEvents(listCache, keyApp);

						
				}
			}
			catch (Exception ex)
			{
				WriteLogError(ex);
			}
		}

		public void SetCacheAplicationName(List<string> listAplictionName, string keyCahe)
		{
			ClearCacheByKey(keyCahe);
			SetCachePageEvents(listAplictionName, keyCahe);
		}

		public void updateCacheOptioneByInstance()
		{


			string aplicationName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

			List<Option> _listOptions = new List<Option>();
			IDatabase db = null;
			if (connection == null)
			{
				setConection();

			}
			db = connection.GetDatabase();


			//Carga las categorias de la base de datos

			List<Option> _listCategory = _stpcForms.Options.Expand("Category").Where(ia => ia.IsActive == true).ToList();
			ClearCacheByKey("listOptions" + aplicationName);
			//_listOptions = JsonConvert.DeserializeObject<List<Option>>(db.StringGet("listOptions"));//Serializo la lista de eventos en cache

			//Recorre las opciones de la base de datos para agregarlas al cache

			foreach (var cat in _listCategory)
			{

				_listOptions.Add(cat);


			}
			UpdateCache(_listOptions, "listOptions");

		}

		public void updateCachePageEventsByInstance()
		{


			string aplicationName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

			List<Option> _listOptions = new List<Option>();
			IDatabase db = null;
			if (connection == null)
			{
				setConection();

			}
			db = connection.GetDatabase();


			//Carga las categorias de la base de datos

			List<FormPage> _listPage = _stpcForms.FormPages.Expand("PageEvents").ToList();
			ClearCacheByKey("listPageEvent" + aplicationName);
			//_listOptions = JsonConvert.DeserializeObject<List<Option>>(db.StringGet("listOptions"));//Serializo la lista de eventos en cache

			//Recorre las opciones de la base de datos para agregarlas al cache
			List<PageEvent> _listToAddCache = new List<PageEvent>();
			foreach (var pageEvent in _listPage)
			{
				foreach (var item in pageEvent.PageEvents)
				{
					_listToAddCache.Add(item);
				}


			}
			UpdateCache(_listToAddCache, "listPageEvent");

		}

		public void updateCachePageMathOperationByInstance()
		{


			string aplicationName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];

			List<PageMathOperation> _listPageMathOperation = new List<PageMathOperation>();
			IDatabase db = null;
			if (connection == null)
			{
				setConection();

			}
			db = connection.GetDatabase();


			//Carga las categorias de la base de datos

			List<PageMathOperation> _listCategory = _stpcForms.PageMathOperation.ToList();
            ClearCacheByKey("listPageMathOperation" + aplicationName);
			//_listOptions = JsonConvert.DeserializeObject<List<Option>>(db.StringGet("listOptions"));//Serializo la lista de eventos en cache

			//Recorre las opciones de la base de datos para agregarlas al cache

			foreach (var cat in _listCategory)
			{

				_listPageMathOperation.Add(cat);


			}
			UpdateCache(_listPageMathOperation, "listPageMathOperation");

		}


		public void Clear()
		{
			IDatabase db = null;
			if (connection == null)
			{
				setConection();

			}
			db = connection.GetDatabase();

			var endpoints = connection.GetEndPoints(true);
			var server = connection.GetServer(endpoints.First());


			var keys = server.Keys();
			foreach (var key in keys)
			{
				Console.WriteLine("Removing Key {0} from cache", key.ToString());
				db.KeyDelete(key);
			}
		}

	}


}