using STPC.DynamicForms.Core;
using STPC.DynamicForms.DataAccess.RT;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.ServiceModel;

namespace STPC.DynamicForms.Services.RequestService
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
	public class RequestService : IRequestService
	{
		List<Hierarchy> listHierarchy = null;
		List<Hierarchy> listHierarchylistTemp = null;
		RTDataBasesAccess dbAcccess = null;
		StringBuilder command = null;
		STPC_FormsFormEntities ctx = new STPC_FormsFormEntities();

		public List<PageField> getFieldByPage(Guid uid)
		{

			FormPage listPage = ctx.FormPages.Include("Panels.PanelFields").Include("Panels.PanelFields.FormFieldType").Where(p => p.Uid == uid).FirstOrDefault();

			List<PageField> listFields = new List<PageField>();

			foreach (var panel in listPage.Panels)
			{
				foreach (var field in panel.PanelFields)
				{
					PageFieldType _PageFieldType = new PageFieldType();
					field.Panel = null;
					_PageFieldType.FieldTypeName = field.FormFieldType.FieldTypeName;
					_PageFieldType.RegExDefault = field.FormFieldType.RegExDefault;
					field.FormFieldType = _PageFieldType;
					listFields.Add(field);
				}
			}
			return listFields;
		}
		public RequestService()
		{
			if (dbAcccess == null)
				dbAcccess = new RTDataBasesAccess();


		}

		public IEnumerable<STPC.DynamicForms.Web.Models.Request> GetRequest(Dictionary<string, string> values)
		{
			List<Request> requestList = new List<Request>();
			return requestList;
		}

		public int CreateRequest(STPC.DynamicForms.Web.Models.Request request)
		{

			int requestId = 0;

			if (dbAcccess.Parameter == null)
				dbAcccess.Parameter = new List<SqlParameter>();
			SqlDataReader reader = null;
			try
			{
				dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@FormId", SqlDbType = System.Data.SqlDbType.UniqueIdentifier, SqlValue = request.FormId });
				dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@PageFlowState", SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = string.IsNullOrEmpty(request.PageFlowState) ? string.Empty : request.PageFlowState });
				dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@PageFlowId", SqlDbType = System.Data.SqlDbType.UniqueIdentifier, SqlValue = request.PageFlowId });
				dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@Created", SqlDbType = System.Data.SqlDbType.DateTime, SqlValue = request.Created });
				dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@CreatedBy", SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = request.CreatedBy });
				dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@Updated", SqlDbType = System.Data.SqlDbType.DateTime, SqlValue = request.Updated });
				dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@UpdatedBy", SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = request.UpdatedBy });
				dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@AssignedTo", SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = request.AssignedTo });
				dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@AplicationNameId", SqlDbType = System.Data.SqlDbType.Int, SqlValue = request.AplicationNameId });

				reader = dbAcccess.GetCommandReader("Insert_Request", CommandType.StoredProcedure);
				while (reader.Read())
				{
					requestId = Convert.ToInt32(reader["RequestId"]);
				}

				reader.Close();
				dbAcccess.SqlConn.Close();
				return requestId;
			}
			catch (Exception ex)
			{
				ILogging eventWriter = LoggingFactory.GetInstance();
				eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace));
				return requestId;
			}
			finally
			{
				if (reader != null)
					if (!reader.IsClosed)
						reader.Close();
			}
		}



		public string CreatePageFlowStepInstance(STPC.DynamicForms.Web.Models.Request request, Guid formPageId, string formName, string xml)
		{
			ScriptGenerator scriptGenerator = new ScriptGenerator();

			request.PageFlowId = Guid.NewGuid();
			request.PageFlowState = scriptGenerator.GetTableName(formPageId, formName.Replace(" ", ""));
			//TODO: Update anmespace
			XNamespace ns = "http://schemas.datacontract.org/2004/07/STPC.DynamicForms.Web.RT.Models";
			XNamespace ns2 = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";

			StringBuilder columnName = new StringBuilder();
			StringBuilder columnValue = new StringBuilder();

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);

			XElement root = XElement.Parse(doc.InnerXml);

			var values = (from v in root.Elements(ns + "FieldsByPage").Elements(ns2 + "KeyValueOfstringstring")
							  select new Values
							  {
								  key = "F_" + v.Element(ns2 + "Key").Value,
								  value = SanitizeLiteral(v.Element(ns2 + "Value").Value)
							  }).ToList();


			string dateFormat = System.Configuration.ConfigurationManager.AppSettings["ShortDateFormat"];
			string longDateFormat = System.Configuration.ConfigurationManager.AppSettings["LongDateFormat"];

			values.Add(new Values() { key = "F_" + Constants.REQUEST_KEY, value = request.RequestId.ToString() });
			values.Add(new Values() { key = "F_" + Constants.PAGE_FLOW_ID, value = request.PageFlowId.ToString() });
			values.Add(new Values() { key = "F_" + Constants.UPDATED, value = string.Format("{0}", DateTime.Now.ToString(longDateFormat)) });
			values.Add(new Values() { key = "F_" + Constants.UPDATE_BY, value = request.UpdatedBy });
			var lastValue = values.Last().value;

			#region Consulta estado anterior del formulario1

			StringBuilder selectLastRequest = new StringBuilder();


			//selectLastRequest.AppendFormat("select  * {0} {1} {2} {3} F_PageFlowId=",Constants.FROM, request.PageFlowState, Constants.WHERE, request.PageFlowId.ToString());
			selectLastRequest.AppendFormat("select  * from {0} where F_RequestId={1} order by F_Updated desc", request.PageFlowState, request.RequestId);

			SqlDataReader _reader = dbAcccess.GetCommandReader(selectLastRequest.ToString(), CommandType.Text);

			DataTable table = new DataTable();
			//convierto un datareader aun datatable
			table.Load(_reader);
			_reader.Close();
			dbAcccess.SqlConn.Close();
			DataRow rows;

			#endregion

			foreach (var v in values)
			{

				if (table.Columns[v.key] == null)
				{
					throw new MissingFieldException("El repositorio y el formulario no coinciden.  Es necesario actualizar el repositorio del formulario.");
				}

				if (table.Columns[v.key].DataType == typeof(decimal) && string.IsNullOrEmpty(v.value))
					v.value = "0";

				columnName.Append(v.key);
				columnValue.Append("'");
				columnValue.Append(v.value);
				columnValue.Append("'");
				if (v.value != lastValue)
				{
					columnName.Append(",");
					columnValue.Append(",");
				}
				else
				{
					columnName.Append(string.Empty);
					columnValue.Append(string.Empty);
				}

			}
			if (table.Rows.Count > 0)
			{
				rows = table.Rows[0];
				int countColumns = 0;
				for (int indexColumn = 0; indexColumn < table.Columns.Count; indexColumn++)
				{

					string lastColumName = table.Columns[table.Columns.Count - 1].ColumnName;
					string fiedName = table.Columns[indexColumn].ColumnName;
					var myColumn = table.Columns.Cast<DataColumn>().SingleOrDefault(col => col.ColumnName == fiedName);

					if (myColumn != null)
						if (values.Where(e => e.key == fiedName).FirstOrDefault() == null)
						{
							if (myColumn != null)
							{
								// just some roww

								var tableRow = table.AsEnumerable().First();
								//var myData = tableRow.Field<string>(myColumn);
								// or if above does not work
								var myData = tableRow[myColumn].ToString();

								if (tableRow[myColumn].GetType() == Type.GetType("System.DateTime"))
								{
									DateTime fec = DateTime.Parse(tableRow[myColumn].ToString());
									myData = fec.ToString(longDateFormat);
								}

								if (myColumn.DataType == typeof(decimal))
								{
									decimal dec = !string.IsNullOrEmpty(tableRow[myColumn].ToString()) ? decimal.Parse(tableRow[myColumn].ToString()) : 0;
									myData = dec.ToString();
								}
								if (myData != null)
								{
									columnName.Append(",");
									columnValue.Append(",");
									columnName.Append(fiedName);
									columnValue.Append("'");
									columnValue.Append(myData);
									columnValue.Append("'");
								}
							}
						}

					countColumns++;
				}
			}
			command = new StringBuilder();

			// procesar el parametro updated, conflicto de fechas
			string rep = columnValue.ToString().Replace("MM/dd/yyyy", DateTime.Now.ToString(longDateFormat));
			columnValue.Clear();
			columnValue = new StringBuilder(rep);

			command.AppendFormat("{0} {1}({2}) values ({3}) ", Constants.INSERT, request.PageFlowState, columnName, columnValue);
			dbAcccess.Post(command.ToString(), CommandType.Text);

			// UpdateRequest(request);

			return request.PageFlowState;
		}


		public string NewPageFlowStepInstance(STPC.DynamicForms.Web.Models.Request request, FormData data)
		{
			ScriptGenerator scriptGenerator = new ScriptGenerator();
			request.PageFlowId = Guid.NewGuid();
			request.PageFlowState = scriptGenerator.GetTableName(data.FormPageId, data.FormName.Replace(" ", ""));

			StringBuilder columnName = new StringBuilder();
			StringBuilder columnValue = new StringBuilder();

			string dateFormat = System.Configuration.ConfigurationManager.AppSettings["ShortDateFormat"];
			string longDateFormat = System.Configuration.ConfigurationManager.AppSettings["LongDateFormat"];

			data.PageFields.Add(Constants.REQUEST_KEY, request.RequestId.ToString());
			data.PageFields.Add(Constants.PAGE_FLOW_ID, request.PageFlowId.ToString());
			data.PageFields.Add(Constants.UPDATED, string.Format("{0}", DateTime.Now.ToString(longDateFormat)));
			data.PageFields.Add(Constants.UPDATE_BY, request.UpdatedBy);
			var lastValue = data.PageFields.Last().Value;

			#region Consulta estado anterior del formulario1

			StringBuilder selectLastRequest = new StringBuilder();

			//selectLastRequest.AppendFormat("select  * {0} {1} {2} {3} F_PageFlowId=",Constants.FROM, request.PageFlowState, Constants.WHERE, request.PageFlowId.ToString());
			selectLastRequest.AppendFormat("select  * from {0} where F_RequestId={1} order by F_Updated desc", request.PageFlowState, request.RequestId);

			SqlDataReader _reader = dbAcccess.GetCommandReader(selectLastRequest.ToString(), CommandType.Text);

			DataTable table = new DataTable();
			//convierto un datareader aun datatable
			table.Load(_reader);
			_reader.Close();
			dbAcccess.SqlConn.Close();
			DataRow rows;

			#endregion

			string value;
			foreach (var v in data.PageFields)
			{

				if (table.Columns["F_" + v.Key] == null)
				{
					throw new MissingFieldException("El repositorio y el formulario no coinciden.  Es necesario actualizar el repositorio del formulario.");
				}

				if (table.Columns["F_" + v.Key].DataType == typeof(decimal) && string.IsNullOrEmpty(v.Value))
					value = null;
				else
					value = SanitizeLiteral(v.Value.Trim());

				if (value != null)
				{
					columnName.Append("F_" + v.Key);
					columnValue.Append("'");
					columnValue.Append(value.Trim());
					columnValue.Append("'");
					if (v.Value != lastValue)
					{
						columnName.Append(",");
						columnValue.Append(",");
					}
					else
					{
						columnName.Append(string.Empty);
						columnValue.Append(string.Empty);
					}
				}
				else
				{
					columnName.Append("F_" + v.Key);
					columnValue.Append("null");
					if (v.Value != lastValue)
					{
						columnName.Append(",");
						columnValue.Append(",");
					}
					else
					{
						columnName.Append(string.Empty);
						columnValue.Append(string.Empty);
					}
				}
			}
			if (table.Rows.Count > 0)
			{
				rows = table.Rows[0];
				int countColumns = 0;
				for (int indexColumn = 0; indexColumn < table.Columns.Count; indexColumn++)
				{

					string lastColumName = table.Columns[table.Columns.Count - 1].ColumnName;
					string fieldName = table.Columns[indexColumn].ColumnName;
					var myColumn = table.Columns.Cast<DataColumn>().SingleOrDefault(col => col.ColumnName == fieldName);

					if (myColumn != null)
					{
						data.PageFields.TryGetValue(fieldName.Substring(2, fieldName.Length - 2), out value);
						if (value == null)
						{
							if (myColumn != null)
							{
								// just some roww

								var tableRow = table.AsEnumerable().First();
								//var myData = tableRow.Field<string>(myColumn);
								// or if above does not work
								var myData = tableRow[myColumn].ToString();

								if (tableRow[myColumn].GetType() == Type.GetType("System.DateTime"))
								{
									DateTime fec = DateTime.Parse(tableRow[myColumn].ToString());
									myData = fec.ToString(longDateFormat);
								}

								if (myColumn.DataType == typeof(decimal))
								{
									decimal dec = !string.IsNullOrEmpty(tableRow[myColumn].ToString()) ? decimal.Parse(tableRow[myColumn].ToString()) : 0;

									myData = dec.ToString();

									if (myData == "0")
										myData = null;
								}
								if (myData != null)
								{
									columnName.Append(",");
									columnValue.Append(",");
									columnName.Append(fieldName);
									columnValue.Append("'");
									columnValue.Append(myData);
									columnValue.Append("'");
								}
								else
								{
									columnName.Append(",");
									columnValue.Append(",");
									columnName.Append(fieldName);

									columnValue.Append("null");

								}

							}
						}
					}
					countColumns++;
				}
			}
			command = new StringBuilder();

			command.AppendFormat("{0} {1}({2}) values ({3}) ", Constants.INSERT, request.PageFlowState, columnName, columnValue);

			dbAcccess.Post(command.ToString(), CommandType.Text);
			return request.PageFlowState;
			// UpdateRequest(request);
		}


		public void UpdateRequest(STPC.DynamicForms.Web.Models.Request request)
		{
			if (dbAcccess.Parameter == null)
				dbAcccess.Parameter = new List<SqlParameter>();

			if (request.WorkFlowState == null)
			{
				Guid guid = new Guid();
				dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@WorkFlowState", SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = guid.ToString() });
			}
			else
				dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@WorkFlowState", SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = request.WorkFlowState.ToString() });

			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@RequestId", SqlDbType = System.Data.SqlDbType.Int, SqlValue = request.RequestId });
			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@PageFlowState", SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = request.PageFlowState });
			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@PageFlowId", SqlDbType = System.Data.SqlDbType.UniqueIdentifier, SqlValue = request.PageFlowId });
			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@UpdatedBy", SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = request.UpdatedBy });
			dbAcccess.Post("Update_Request", CommandType.StoredProcedure);
			dbAcccess.SqlConn.Close();

		}
		public void UpdateRequest(STPC.DynamicForms.Web.Models.Request request, Guid formPageId, string formName)
		{
			ScriptGenerator scriptGenerator = new ScriptGenerator();

			if (dbAcccess.Parameter == null)
				dbAcccess.Parameter = new List<SqlParameter>();

			request.PageFlowState = scriptGenerator.GetTableName(formPageId, formName.Replace(" ", ""));
			if (request.WorkFlowState == null)
			{
				Guid guid = new Guid();
				dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@WorkFlowState", SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = guid.ToString() });
			}
			else
				dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@WorkFlowState", SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = request.WorkFlowState.ToString() });

			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@RequestId", SqlDbType = System.Data.SqlDbType.Int, SqlValue = request.RequestId });
			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@PageFlowState", SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = request.PageFlowState });
			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@PageFlowId", SqlDbType = System.Data.SqlDbType.UniqueIdentifier, SqlValue = request.PageFlowId });
			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@UpdatedBy", SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = request.UpdatedBy });
			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@Updated", SqlDbType = System.Data.SqlDbType.DateTime, SqlValue = DateTime.Now });

			dbAcccess.Post("Update_Request", CommandType.StoredProcedure);
			dbAcccess.SqlConn.Close();

		}

		/// <summary>
		/// Genera consulta de solicitudes desde la opción consultar solicitudes
		/// </summary>
		/// <param name="values"></param>
		/// <param name="ParametersRequestTable"></param>
		/// <param name="RequestType"></param>
		/// <returns></returns>
		public MyViewModel GetRequestDynamic(List<ReportParameters> values, Dictionary<string, string> ParametersRequestTable, string RequestType)
		{
			StringBuilder query = new StringBuilder();


			StringBuilder queryWhere = new StringBuilder();
			StringBuilder queryWhereDynamicFields = new StringBuilder();

			StringBuilder querySelect = new StringBuilder();
			StringBuilder querySelectDynamicFields = new StringBuilder();
			StringBuilder queryJoin = new StringBuilder();
			List<ReportParameters> ListGoupField = new List<ReportParameters>();
			List<ReportParameters> listAddedJoins = new List<ReportParameters>();
			string dtCreatedDateIni = string.Empty;
			string dtCreatedDateEnd = string.Empty;
			string dtUpdatedDateIni = string.Empty;
			string dtUpdatedDateEnd = string.Empty;

			string formatoFecha = System.Configuration.ConfigurationManager.AppSettings["ShortDateFormat"];
			StringBuilder strbuilderCreatedDateIni = new StringBuilder();
			StringBuilder strbuilderCreatedDateEnd = new StringBuilder();

			StringBuilder strbuilderUpdatedDateIni = new StringBuilder();
			StringBuilder strbuilderUpdatedDateEnd = new StringBuilder();


			//Construye select y where con base en la tabla solicitudes
			if (!string.IsNullOrEmpty(ParametersRequestTable["Created"]))
			{
				dtCreatedDateIni = Convert.ToDateTime(ParametersRequestTable["Created"]).ToString(formatoFecha);
				dtCreatedDateEnd = Convert.ToDateTime(ParametersRequestTable["Created_End"]).ToString(formatoFecha);

				queryWhere.Append(Constants.REQUEST_TABLE_NAME + ".Created between '" + dtCreatedDateIni + "'" + Constants.AND + "'" + dtCreatedDateEnd + "'");

			}

			if (!string.IsNullOrEmpty(ParametersRequestTable["Updated"]))
			{
				dtUpdatedDateIni = Convert.ToDateTime(ParametersRequestTable["Updated"]).ToString(formatoFecha);
				dtUpdatedDateEnd = Convert.ToDateTime(ParametersRequestTable["Updated_End"]).ToString(formatoFecha);


				if (!string.IsNullOrEmpty(queryWhere.ToString()))
				{
					queryWhere.Append(Constants.AND);

				}

				queryWhere.Append(Constants.REQUEST_TABLE_NAME + ".Updated between '" + dtUpdatedDateIni + "'" + Constants.AND + "'" + dtUpdatedDateEnd + "'");

			}
			querySelect.Append(" Request.RequestId as Número, Request.FormId ,Request.Created as Fecha,  FS.StateName as Estado");
			//querySelect.Append(" Request.RequestId as Número, Request.FormId as Formulario,Request.Created as Fecha, Request.Updated as Actualizado, Request.CreatedBy as Creador_por, Request.UpdatedBy as Actualizado_por, FS.StateName");

			//querySelect.Append(Constants.REQUEST_TABLE_NAME + ".*");

			foreach (var item in ParametersRequestTable)
			{
				if (item.Key != "Created" && item.Key != "Updated" && item.Key != "Created_End" && item.Key != "Updated_End" && item.Key != "Hierarchies")
				{
					if (!string.IsNullOrEmpty(item.Value))
					{
						if (!string.IsNullOrEmpty(queryWhere.ToString()))
						{
							queryWhere.Append(Constants.AND);
						}

						queryWhere.Append(Constants.REQUEST_TABLE_NAME + "." + item.Key + " = '" + item.Value + "'");

					}

				}
			}

			//Construye inner join para determinar la jerarquia que se esta consultando
			if (!string.IsNullOrEmpty(ParametersRequestTable["Hierarchies"]))
			{

				queryJoin.Append(Constants.JOIN + Constants.TABLE_USER + Constants.ON + Constants.REQUEST_TABLE_NAME + ".UpdatedBy = " + Constants.TABLE_USER + ".username");
				queryJoin.Append(Constants.JOIN + Constants.TABLE_HIERARCHY + Constants.ON + Constants.TABLE_USER + ".Hierarchy_Id=Hierarchy.Id");

				if (!string.IsNullOrEmpty(queryWhere.ToString()))
				{
					queryWhere.Append(Constants.AND);

				}
				queryWhere.Append(Constants.TABLE_USER + ".Hierarchy_Id in(" + ParametersRequestTable["Hierarchies"] + ")");

			}


			//Construye select y where con base en las tablas dinamicas
			foreach (var fields in values)
			{
				if (!string.IsNullOrEmpty(fields.value))
				{
					if (!string.IsNullOrEmpty(queryWhere.ToString()))
					{
						queryWhere.Append(Constants.AND);
					}
					queryWhere.Append(Constants.PREFIX_DYNAMIC_TABLE + fields.NamePage.Replace(" ", "") + "_" + RequestType + "." + Constants.PREFIX_DYNAMIC_FIELD + fields.NameField + " = '" + fields.value + "'");
					queryWhereDynamicFields.Append(Constants.PREFIX_DYNAMIC_TABLE + fields.NamePage.Replace(" ", "") + "_" + RequestType + "." + Constants.PREFIX_DYNAMIC_FIELD + fields.NameField + " = '" + fields.value + "'");

				}
			}

			////Agrupa los nomrbes de los panels para crear join con la tabla solicitudes
			//var groupedList = values.GroupBy(c => new { c.NamePage }).ToList();

			foreach (var tables in values)
			{
				if (listAddedJoins.Where(e => e.NamePage == tables.NamePage).Count() == 0)
				{
					queryJoin.Append(Constants.JOIN + Constants.PREFIX_DYNAMIC_TABLE + tables.NamePage.Replace(" ", "") + "_" + RequestType);
					queryJoin.Append(Constants.ON + Constants.REQUEST_TABLE_NAME + "." + Constants.REQUEST_KEY + "=" + Constants.PREFIX_DYNAMIC_TABLE + tables.NamePage.Replace(" ", "") + "_" + RequestType + "." + Constants.REQUEST_ID_NAME_DINAMYC_TABLE);
					querySelect.Clear();
					//querySelect.Append(" " + Constants.PREFIX_DYNAMIC_TABLE + tables.NamePage.Replace(" ", "") + "_" + RequestType + ".F_Updated as Fecha_Instancia, Request.RequestId as Número, Request.FormId as Formulario,Request.Created as Fecha,FS.StateName as Estado ");
					querySelect.Append(" " + Constants.PREFIX_DYNAMIC_TABLE + tables.NamePage.Replace(" ", "") + "_" + RequestType + ".F_Updated as Fecha_Instancia, Request.RequestId as Número, Request.FormId ,Request.Created as Fecha,FS.StateName as Estado ");
					listAddedJoins.Add(tables);
				}

			}

			query.Append(Constants.SELECT + querySelect);
			query.Append(Constants.FROM + Constants.REQUEST_TABLE_NAME);

			if (!string.IsNullOrEmpty(queryJoin.ToString()))
				query.Append(queryJoin);

			query.Append(" INNER JOIN FormStates FS ON FS.Uid=Request.WorkFlowState");

			if (!string.IsNullOrEmpty(queryWhere.ToString()))
			{
				query.Append(Constants.WHERE + queryWhere);
			}

			RTDataBasesAccess _RTDataBasesAccess = new RTDataBasesAccess();
			SqlDataReader _reader = _RTDataBasesAccess.GetCommandReader(query.ToString(), System.Data.CommandType.Text);
			DataTable table = new DataTable();
			//convierto un datareader a un datatable
			table.Load(_reader);
			_reader.Close();
			dbAcccess.SqlConn.Close();
			//Mapea campos que vienen de las tablas dinamicas
			MyViewModel modelQuery = new MyViewModel();
			modelQuery.Rows = new List<RowViewModel>();
			modelQuery.Columns = new List<ColumnViewModel>();

			for (int i = 0; i < table.Columns.Count; i++)
			{
				ColumnViewModel col = new ColumnViewModel();
				string[] roolSeleccionado = null;
				roolSeleccionado = table.Columns[i].ColumnName.Split(new Char[] { '_' });


				if (roolSeleccionado.Length == 2)
					col.Name = roolSeleccionado[0] + " " + roolSeleccionado[1];
				else
					col.Name = table.Columns[i].ColumnName;
				if (col.Name != "Fecha Instancia")
					modelQuery.Columns.Add(col);
			}
			for (int i = 0; i < table.Rows.Count; i++)
			{
				DataRow row = table.Rows[i];

				List<CellValueViewModel> listValues = new List<CellValueViewModel>();
				for (int j = 0; j < table.Columns.Count; j++)
				{
					ColumnViewModel col = new ColumnViewModel();
					string[] roolSeleccionado = null;
					roolSeleccionado = table.Columns[j].ColumnName.Split(new Char[] { '_' });


					if (roolSeleccionado.Length == 2)
						col.Name = roolSeleccionado[0] + " " + roolSeleccionado[1];
					else
						col.Name = table.Columns[j].ColumnName;
					if (col.Name != "Fecha Instancia")
					{
						CellValueViewModel cel = new CellValueViewModel();
						cel.Value = row[j].ToString();
						cel.ColumnName = col.Name;
						cel.Index = j;
						listValues.Add(cel);
					}
				}
				RowViewModel _RowViewModel = new RowViewModel();
				_RowViewModel.Values = listValues;
				modelQuery.Rows.Add(_RowViewModel);
			}


			return modelQuery;
		}

		public MyViewModel GetRequestDynamicSearcher(List<ReportParameters> values, Dictionary<string, string> ParametersRequestTable, string RequestType)
		{
			StringBuilder query = new StringBuilder();
			StringBuilder queryWhere = new StringBuilder();
			StringBuilder queryWhereDynamicFields = new StringBuilder();

			StringBuilder querySelect = new StringBuilder();
			string[] querySelectDynamicFields = new string[values.Count];
			string srquerySelectDynamicFields = string.Empty;
			StringBuilder queryJoin = new StringBuilder();
			List<ReportParameters> ListGoupField = new List<ReportParameters>();
			List<ReportParameters> listAddedJoins = new List<ReportParameters>();
			string dtCreatedDateIni = string.Empty;
			string dtCreatedDateEnd = string.Empty;
			string dtUpdatedDateIni = string.Empty;
			string dtUpdatedDateEnd = string.Empty;

			string formatoFecha = System.Configuration.ConfigurationManager.AppSettings["ShortDateFormat"];
			StringBuilder strbuilderCreatedDateIni = new StringBuilder();
			StringBuilder strbuilderCreatedDateEnd = new StringBuilder();

			StringBuilder strbuilderUpdatedDateIni = new StringBuilder();
			StringBuilder strbuilderUpdatedDateEnd = new StringBuilder();
			MyViewModel modelQuery = new MyViewModel();
			try
			{

				//Construye select y where con base en la tabla solicitudes
				if (!string.IsNullOrEmpty(ParametersRequestTable["Created"]))
				{
					dtCreatedDateIni = Convert.ToDateTime(ParametersRequestTable["Created"]).ToString(formatoFecha);
					dtCreatedDateEnd = Convert.ToDateTime(ParametersRequestTable["Created_End"]).ToString(formatoFecha);
					ParametersRequestTable["Created"] = dtCreatedDateIni;
					ParametersRequestTable["Created_End"] = dtCreatedDateEnd;

					queryWhere.Append(Constants.REQUEST_TABLE_NAME + ".Created between '" + dtCreatedDateIni + "'" + Constants.AND + "'" + dtCreatedDateEnd + "'");

				}

				if (!string.IsNullOrEmpty(ParametersRequestTable["Updated"]))
				{
					dtUpdatedDateIni = Convert.ToDateTime(ParametersRequestTable["Updated"]).ToString(formatoFecha);
					dtUpdatedDateEnd = Convert.ToDateTime(ParametersRequestTable["Updated_End"]).ToString(formatoFecha);
					ParametersRequestTable["Updated"] = dtUpdatedDateIni;
					ParametersRequestTable["Updated_End"] = dtUpdatedDateIni;

					if (!string.IsNullOrEmpty(queryWhere.ToString()))
					{
						queryWhere.Append(Constants.AND);

					}

					queryWhere.Append(Constants.REQUEST_TABLE_NAME + ".Updated between '" + dtUpdatedDateIni + "'" + Constants.AND + "'" + dtUpdatedDateEnd + "'");

				}
				querySelect.Append(" Request.RequestId as Número, Request.FormId ,Request.Created as Fecha,  FS.StateName as Estado");
				//querySelect.Append(" Request.RequestId as Número, Request.FormId as Formulario,Request.Created as Fecha, Request.Updated as Actualizado, Request.CreatedBy as Creador_por, Request.UpdatedBy as Actualizado_por, FS.StateName");

				//querySelect.Append(Constants.REQUEST_TABLE_NAME + ".*");

				foreach (var item in ParametersRequestTable)
				{
					if (item.Key != "Created" && item.Key != "Updated" && item.Key != "Created_End" && item.Key != "UserName" && item.Key != "Updated_End" && item.Key != "Hierarchies")
					{
						if (!string.IsNullOrEmpty(item.Value))
						{
							if (!string.IsNullOrEmpty(queryWhere.ToString()))
							{
								queryWhere.Append(Constants.AND);
							}

							queryWhere.Append(Constants.REQUEST_TABLE_NAME + "." + item.Key + " = '" + item.Value + "'");

						}

					}
				}

				//Construye inner join para determinar la jerarquia que se esta consultando
				if (!string.IsNullOrEmpty(ParametersRequestTable["Hierarchies"]))
				{

					queryJoin.Append(Constants.JOIN + Constants.TABLE_USER + Constants.ON + Constants.REQUEST_TABLE_NAME + ".UpdatedBy = " + Constants.TABLE_USER + ".username");
					queryJoin.Append(Constants.JOIN + Constants.TABLE_HIERARCHY + Constants.ON + Constants.TABLE_USER + ".Hierarchy_Id=Hierarchy.Id");

					if (!string.IsNullOrEmpty(queryWhere.ToString()))
					{
						queryWhere.Append(Constants.AND);

					}
					queryWhere.Append(Constants.TABLE_USER + ".Hierarchy_Id in(" + ParametersRequestTable["Hierarchies"] + ")");

				}


				//Construye select y where con base en las tablas dinamicas
				foreach (var fields in values)
				{
					if (!string.IsNullOrEmpty(fields.value))
					{
						if (!string.IsNullOrEmpty(queryWhere.ToString()))
						{
							queryWhere.Append(Constants.AND);
							queryWhereDynamicFields.Append(Constants.AND);
						}
						queryWhere.Append(Constants.PREFIX_DYNAMIC_TABLE + fields.NamePage.Replace(" ", "") + "_" + RequestType + "." + Constants.PREFIX_DYNAMIC_FIELD + fields.NameField + " = '" + fields.value + "'");
						queryWhereDynamicFields.Append(Constants.PREFIX_DYNAMIC_TABLE + fields.NamePage.Replace(" ", "") + "_" + RequestType + "." + Constants.PREFIX_DYNAMIC_FIELD + fields.NameField + " = '" + fields.value + "'");

					}
				}

				////Agrupa los nomrbes de los panels para crear join con la tabla solicitudes
				//var groupedList = values.GroupBy(c => new { c.NamePage }).ToList();
				int countDinamycFields = 0;
				foreach (var tables in values)
				{
					if (listAddedJoins.Where(e => e.NamePage == tables.NamePage).Count() == 0)
					{
						queryJoin.Append(Constants.JOIN + Constants.PREFIX_DYNAMIC_TABLE + tables.NamePage.Replace(" ", "") + "_" + RequestType);
						queryJoin.Append(Constants.ON + Constants.REQUEST_TABLE_NAME + "." + Constants.REQUEST_KEY + "=" + Constants.PREFIX_DYNAMIC_TABLE + tables.NamePage.Replace(" ", "") + "_" + RequestType + "." + Constants.REQUEST_ID_NAME_DINAMYC_TABLE);
						querySelect.Clear();
						//querySelect.Append(" " + Constants.PREFIX_DYNAMIC_TABLE + tables.NamePage.Replace(" ", "") + "_" + RequestType + ".F_Updated as Fecha_Instancia, Request.RequestId as Número, Request.FormId as Formulario,Request.Created as Fecha,FS.StateName as Estado ");
						querySelect.Append(" " + Constants.PREFIX_DYNAMIC_TABLE + tables.NamePage.Replace(" ", "") + "_" + RequestType + ".F_Updated as Fecha_Instancia, Request.RequestId as Número, Request.FormId ,Request.Created as Fecha,FS.StateName as Estado ");
						//querySelectDynamicFields.Append(" " + tables.NamePage + "." + tables.NameField);
						listAddedJoins.Add(tables);
					}
					querySelectDynamicFields[countDinamycFields] = tables.NamePage + "." + tables.NameField;
					countDinamycFields++;

				}
				srquerySelectDynamicFields = String.Join(",", querySelectDynamicFields);

				query.Append(Constants.SELECT + querySelect);
				query.Append(Constants.FROM + Constants.REQUEST_TABLE_NAME);

				if (!string.IsNullOrEmpty(queryJoin.ToString()))
					query.Append(queryJoin);

				query.Append(" INNER JOIN FormStates FS ON FS.Uid=Request.WorkFlowState");

				if (!string.IsNullOrEmpty(queryWhere.ToString()))
				{
					query.Append(Constants.WHERE + queryWhere);
				}

				RTDataBasesAccess _RTDataBasesAccess = new RTDataBasesAccess();
				_RTDataBasesAccess.Parameter = new List<SqlParameter>();

				if (ParametersRequestTable["FormId"] == string.Empty)
				{
					ParametersRequestTable["FormId"] = Guid.NewGuid().ToString();
				}
				foreach (KeyValuePair<string, string> row in ParametersRequestTable)
				{
					_RTDataBasesAccess.Parameter.Add(new SqlParameter { ParameterName = row.Key, SqlDbType = System.Data.SqlDbType.NVarChar, SqlValue = row.Value });
				}
				//_RTDataBasesAccess.Parameter.Add(new SqlParameter { ParameterName = "UserName", SqlDbType = System.Data.SqlDbType.NVarChar, SqlValue = ParametersRequestTable["UserName"].ToString()});
				_RTDataBasesAccess.Parameter.Add(new SqlParameter { ParameterName = "DynamicWhere", SqlDbType = System.Data.SqlDbType.NVarChar, SqlValue = queryWhereDynamicFields.ToString() });
				_RTDataBasesAccess.Parameter.Add(new SqlParameter { ParameterName = "DynamicJoin", SqlDbType = System.Data.SqlDbType.NVarChar, SqlValue = queryJoin.ToString() });
				_RTDataBasesAccess.Parameter.Add(new SqlParameter { ParameterName = "DynamicSelect", SqlDbType = System.Data.SqlDbType.NVarChar, SqlValue = srquerySelectDynamicFields.ToString() });
				//_RTDataBasesAccess.Parameter.Add(new SqlParameter { ParameterName = "@SqlStringOutPut", SqlDbType = System.Data.SqlDbType.NVarChar, SqlValue = srquerySelectDynamicFields.ToString() });
				SqlParameter parm2 = new SqlParameter("SqlStringOutPut", SqlDbType.VarChar);
				parm2.Size = 250;
				parm2.Direction = ParameterDirection.Output;
				_RTDataBasesAccess.Parameter.Add(parm2);


				SqlDataReader _reader = _RTDataBasesAccess.GetCommandReader("SearchRequests", System.Data.CommandType.StoredProcedure);

				//string QueryResult = _RTDataBasesAccess.Parameter["@SqlStringOutPut"].Value;
				DataTable table = new DataTable();
				//convierto un datareader aun datatable
				table.Load(_reader);
				_reader.Close();
				dbAcccess.SqlConn.Close();
				//Mapea campos que vienen de las tablas dinamicas
				modelQuery.Rows = new List<RowViewModel>();
				modelQuery.Columns = new List<ColumnViewModel>();

				for (int i = 0; i < table.Columns.Count; i++)
				{
					ColumnViewModel col = new ColumnViewModel();
					string[] roolSeleccionado = null;
					roolSeleccionado = table.Columns[i].ColumnName.Split(new Char[] { '_' });


					if (roolSeleccionado.Length == 2)
						col.Name = roolSeleccionado[0] + " " + roolSeleccionado[1];
					else
						col.Name = table.Columns[i].ColumnName;
					if (col.Name != "Fecha Instancia")
						modelQuery.Columns.Add(col);
				}
				for (int i = 0; i < table.Rows.Count; i++)
				{
					DataRow row = table.Rows[i];

					List<CellValueViewModel> listValues = new List<CellValueViewModel>();
					for (int j = 0; j < table.Columns.Count; j++)
					{
						ColumnViewModel col = new ColumnViewModel();
						string[] roolSeleccionado = null;
						roolSeleccionado = table.Columns[j].ColumnName.Split(new Char[] { '_' });


						if (roolSeleccionado.Length == 2)
							col.Name = roolSeleccionado[0] + " " + roolSeleccionado[1];
						else
							col.Name = table.Columns[j].ColumnName;
						if (col.Name != "Fecha Instancia")
						{
							CellValueViewModel cel = new CellValueViewModel();
							cel.Value = row[j].ToString();
							cel.ColumnName = col.Name;
							cel.Index = j;
							listValues.Add(cel);
						}
					}
					RowViewModel _RowViewModel = new RowViewModel();
					_RowViewModel.Values = listValues;
					modelQuery.Rows.Add(_RowViewModel);
				}

			}

			catch (Exception ex)
			{
				ILogging eventWriter = LoggingFactory.GetInstance();
				eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace));
				return modelQuery;
			}

			return modelQuery;


		}

		public MyViewModel GetRequestByProcedure(string storedProcedure)
		{
			MyViewModel modelQuery = new MyViewModel();
			modelQuery.Rows = new List<RowViewModel>();
			modelQuery.Columns = new List<ColumnViewModel>();

			if (storedProcedure.StartsWith("sp_"))
				return modelQuery;
			try
			{
				StringBuilder query = new StringBuilder();
				query.Append(storedProcedure);

				RTDataBasesAccess _RTDataBasesAccess = new RTDataBasesAccess();
				SqlDataReader _reader = _RTDataBasesAccess.GetCommandReader(query.ToString(), System.Data.CommandType.StoredProcedure);
				DataTable table = new DataTable();
				//convierto un datareader aun datatable
				table.Load(_reader);
				_reader.Close();
				dbAcccess.SqlConn.Close();
				//Mapea campos que vienen de las tablas dinamicas


				for (int i = 0; i < table.Columns.Count; i++)
				{
					ColumnViewModel col = new ColumnViewModel();
					string[] roolSeleccionado = null;
					roolSeleccionado = table.Columns[i].ColumnName.Split(new Char[] { '_' });

					if (roolSeleccionado.Length == 2)
						col.Name = roolSeleccionado[0] + " " + roolSeleccionado[1];
					else
						col.Name = table.Columns[i].ColumnName;

					modelQuery.Columns.Add(col);
				}
				for (int i = 0; i < table.Rows.Count; i++)
				{
					DataRow row = table.Rows[i];

					List<CellValueViewModel> listValues = new List<CellValueViewModel>();
					for (int j = 0; j < table.Columns.Count; j++)
					{

						CellValueViewModel cel = new CellValueViewModel();
						cel.Value = row[j].ToString();
						cel.ColumnName = table.Columns[j].ColumnName;
						cel.Index = j;
						listValues.Add(cel);
					}
					RowViewModel _RowViewModel = new RowViewModel();
					_RowViewModel.Values = listValues;
					modelQuery.Rows.Add(_RowViewModel);
				}

				return modelQuery;
			}
			catch (Exception ex)
			{
				ILogging eventWriter = LoggingFactory.GetInstance();
				//string errorMessage = string.Format(CustomMessages.E0007, "FormPageController", "SaveFile", correlationID, ex.Message);
				eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace));
				return modelQuery;
			}

		}



		public MyViewModel GetIndicatorByProcedure(string storedProcedure, Dictionary<string, string> parameters)
		{
			MyViewModel modelQuery = new MyViewModel();
			modelQuery.Rows = new List<RowViewModel>();
			modelQuery.Columns = new List<ColumnViewModel>();
			DataTable table = new DataTable();

			try
			{
				StringBuilder query = new StringBuilder();
				query.Append(storedProcedure);

				RTDataBasesAccess _RTDataBasesAccess = new RTDataBasesAccess();
				_RTDataBasesAccess.Parameter = new List<SqlParameter>();

				foreach (KeyValuePair<string, string> row in parameters)
				{
					_RTDataBasesAccess.Parameter.Add(new SqlParameter { ParameterName = row.Key, SqlDbType = System.Data.SqlDbType.NVarChar, SqlValue = row.Value });
				}

				SqlDataReader _reader = _RTDataBasesAccess.GetCommandReader(query.ToString(), System.Data.CommandType.StoredProcedure);

				//convierto un datareader a un datatable
				table.Load(_reader);

				_reader.Close();
				dbAcccess.SqlConn.Close();
				//Mapea campos que vienen de las tablas dinamicas


				for (int i = 0; i < table.Columns.Count; i++)
				{
					ColumnViewModel col = new ColumnViewModel();
					string[] roolSeleccionado = null;
					roolSeleccionado = table.Columns[i].ColumnName.Split(new Char[] { '_' });

					if (roolSeleccionado.Length == 2)
						col.Name = roolSeleccionado[0] + " " + roolSeleccionado[1];
					else
						col.Name = table.Columns[i].ColumnName;

					modelQuery.Columns.Add(col);
				}
				for (int i = 0; i < table.Rows.Count; i++)
				{
					DataRow row = table.Rows[i];

					List<CellValueViewModel> listValues = new List<CellValueViewModel>();
					for (int j = 0; j < table.Columns.Count; j++)
					{

						CellValueViewModel cel = new CellValueViewModel();
						cel.Value = row[j].ToString();
						cel.Index = j;
						listValues.Add(cel);
					}
					RowViewModel _RowViewModel = new RowViewModel();
					_RowViewModel.Values = listValues;
					modelQuery.Rows.Add(_RowViewModel);
				}


				return modelQuery;
			}
			catch (Exception ex)
			{
				ILogging eventWriter = LoggingFactory.GetInstance();
				//string errorMessage = string.Format(CustomMessages.E0007, "FormPageController", "SaveFile", correlationID, ex.Message);
				eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace));
				return modelQuery;
			}

		}


		/// <summary>
		///
		/// </summary>
		/// <param name="storedProcedure"></param>
		/// <param name="userName"></param>
		/// <param name="offSet">indice del registro a partir del cual se retornan los resultados</param>
		/// /// <param name="fetch">Indica la cantidad de registros por página</param>
		/// <returns></returns>
		public MyViewModel GetRequestsByParamProcedure(string storedProcedure, string userName, int offSet, int fetch, string filter)
		{
			MyViewModel modelQuery = new MyViewModel();
			modelQuery.Rows = new List<RowViewModel>();
			modelQuery.Columns = new List<ColumnViewModel>();
			if (storedProcedure.StartsWith("sp_"))
				return modelQuery;
			try
			{
				StringBuilder query = new StringBuilder();
				query.Append(storedProcedure);

				RTDataBasesAccess _RTDataBasesAccess = new RTDataBasesAccess();
				_RTDataBasesAccess.Parameter = new List<SqlParameter>();


				SqlCommand cmd = new SqlCommand(query.ToString(), _RTDataBasesAccess.SqlConn);
				cmd.CommandType = CommandType.StoredProcedure;

				SqlCommandBuilder.DeriveParameters(cmd);
				foreach (SqlParameter p in cmd.Parameters)
				{
					Console.WriteLine(p.ParameterName);
				}


				_RTDataBasesAccess.Parameter.Add(new SqlParameter { ParameterName = "@userid", SqlDbType = System.Data.SqlDbType.NVarChar, SqlValue = userName });

				if (cmd.Parameters.Contains("@offSet"))
					_RTDataBasesAccess.Parameter.Add(new SqlParameter { ParameterName = "@offSet", SqlDbType = System.Data.SqlDbType.Int, SqlValue = offSet });

				if (cmd.Parameters.Contains("@fetch"))
					_RTDataBasesAccess.Parameter.Add(new SqlParameter { ParameterName = "@fetch", SqlDbType = System.Data.SqlDbType.Int, SqlValue = fetch });

				if (cmd.Parameters.Contains("@filters"))
					_RTDataBasesAccess.Parameter.Add(new SqlParameter { ParameterName = "@filters", SqlDbType = System.Data.SqlDbType.NVarChar, SqlValue = filter });


				SqlDataReader _reader = _RTDataBasesAccess.GetCommandReader(query.ToString(), System.Data.CommandType.StoredProcedure);

				DataTable table = new DataTable();
				//convierto un datareader aun datatable
				table.Load(_reader);
				_reader.Close();
				_RTDataBasesAccess.SqlConn.Close();
				//Mapea campos que vienen de las tablas dinamicas

				for (int i = 0; i < table.Columns.Count; i++)
				{
					ColumnViewModel col = new ColumnViewModel();
					string[] roolSeleccionado = null;
					roolSeleccionado = table.Columns[i].ColumnName.Split(new Char[] { '_' });

					if (roolSeleccionado.Length == 2)
						col.Name = roolSeleccionado[0] + " " + roolSeleccionado[1];
					else
						col.Name = table.Columns[i].ColumnName;

					modelQuery.Columns.Add(col);
				}
				for (int i = 0; i < table.Rows.Count; i++)
				{
					DataRow row = table.Rows[i];

					List<CellValueViewModel> listValues = new List<CellValueViewModel>();
					for (int j = 0; j < table.Columns.Count; j++)
					{

						CellValueViewModel cel = new CellValueViewModel();
						cel.ColumnName = table.Columns[j].ColumnName;
						cel.Value = row[j].ToString();
						cel.Index = j;
						listValues.Add(cel);
					}
					RowViewModel _RowViewModel = new RowViewModel();
					_RowViewModel.Values = listValues;
					modelQuery.Rows.Add(_RowViewModel);
				}

				return modelQuery;
			}
			catch (Exception ex)
			{
				ILogging eventWriter = LoggingFactory.GetInstance();
				//string errorMessage = string.Format(CustomMessages.E0007, "FormPageController", "SaveFile", correlationID, ex.Message);
				eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace));
				return modelQuery;
			}
		}

		public MyViewModel GetRequestsByParamProcedureSearcher(string storedProcedure, string userName)
		{
			MyViewModel modelQuery = new MyViewModel();
			modelQuery.Rows = new List<RowViewModel>();
			modelQuery.Columns = new List<ColumnViewModel>();
			if (storedProcedure.StartsWith("sp_"))
				return modelQuery;
			try
			{
				StringBuilder query = new StringBuilder();
				query.Append(storedProcedure);

				RTDataBasesAccess _RTDataBasesAccess = new RTDataBasesAccess();
				_RTDataBasesAccess.Parameter = new List<SqlParameter>();

				_RTDataBasesAccess.Parameter.Add(new SqlParameter { ParameterName = "@userid", SqlDbType = System.Data.SqlDbType.NVarChar, SqlValue = userName });

				SqlDataReader _reader = _RTDataBasesAccess.GetCommandReader(query.ToString(), System.Data.CommandType.StoredProcedure);

				DataTable table = new DataTable();
				//convierto un datareader aun datatable
				table.Load(_reader);
				_reader.Close();
				_RTDataBasesAccess.SqlConn.Close();
				//Mapea campos que vienen de las tablas dinamicas

				for (int i = 0; i < table.Columns.Count; i++)
				{
					ColumnViewModel col = new ColumnViewModel();
					string[] roolSeleccionado = null;
					roolSeleccionado = table.Columns[i].ColumnName.Split(new Char[] { '_' });

					if (roolSeleccionado.Length == 2)
						col.Name = roolSeleccionado[0] + " " + roolSeleccionado[1];
					else
						col.Name = table.Columns[i].ColumnName;

					modelQuery.Columns.Add(col);
				}
				for (int i = 0; i < table.Rows.Count; i++)
				{
					DataRow row = table.Rows[i];

					List<CellValueViewModel> listValues = new List<CellValueViewModel>();
					for (int j = 0; j < table.Columns.Count; j++)
					{

						CellValueViewModel cel = new CellValueViewModel();
						cel.ColumnName = table.Columns[j].ColumnName;
						cel.Value = row[j].ToString();
						cel.Index = j;
						listValues.Add(cel);
					}
					RowViewModel _RowViewModel = new RowViewModel();
					_RowViewModel.Values = listValues;
					modelQuery.Rows.Add(_RowViewModel);
				}

				return modelQuery;
			}
			catch (Exception ex)
			{
				ILogging eventWriter = LoggingFactory.GetInstance();
				//string errorMessage = string.Format(CustomMessages.E0007, "FormPageController", "SaveFile", correlationID, ex.Message);
				eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace));
				return modelQuery;
			}
		}

		public List<Values> GetPageFlowLastStepInstance(int RequestId)
		{
			List<Values> result = new List<Values>();
			Values value = null;

			command = new StringBuilder();
			string pageFlowState = string.Empty;
			string pageFlowId = string.Empty;

			//Se obtiene el ultimo paso

			if (dbAcccess.Parameter == null)
				dbAcccess.Parameter = new List<SqlParameter>();

			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@RequestId", SqlDbType = System.Data.SqlDbType.UniqueIdentifier, SqlValue = RequestId });


			var requestReader = dbAcccess.GetCommandReader("Select_RequestById", CommandType.StoredProcedure);

			while (requestReader.Read())
			{
				pageFlowState = requestReader["PageFlowState"].ToString();
				pageFlowId = requestReader["PageFlowId"].ToString();
			}
			requestReader.Close();

			//Se obtiene la data del ultimo paso

			command.AppendFormat("select * from {0} where Id = '{1}'", pageFlowState, pageFlowId);

			dbAcccess.Parameter.Clear();

			var TBLReader = dbAcccess.GetCommandReader(command.ToString(), CommandType.Text);

			while (TBLReader.Read())
			{
				for (int col = 0; col < TBLReader.FieldCount; col++)
				{
					value = new Values()
					{
						key = TBLReader.GetName(col).ToString(),
						value = TBLReader.GetValue(col).ToString()
					};

					result.Add(value);
				}
			}

			TBLReader.Close();
			dbAcccess.SqlConn.Close();
			return result;
		}

		public List<Values> GetPageFlowStepInstance(int requestId, Guid formPageId, Guid PageFlowId, string formName)
		{
			ScriptGenerator scriptGenerator = new ScriptGenerator();
			command = new StringBuilder();
			dbAcccess = new RTDataBasesAccess();
			List<Values> result = new List<Values>();
			Values value = null;

			string pageFlowState = scriptGenerator.GetTableName(formPageId, formName.Replace(" ", ""));
			string requestIdField = scriptGenerator.GetFieldName(Constants.REQUEST_KEY);
			string updatedField = scriptGenerator.GetFieldName(Constants.UPDATED);

			//command.AppendFormat("{0} * {1} {2} {3} {4} = {5} AND {6} = '{7}'",
			//  Constants.SELECT, Constants.FROM, pageFlowState, Constants.WHERE, requestIdField, requestId, "F_PageFlowId", PageFlowId);

			//Se modifica el where, solo debe filtrar por requesId y se agrega order by or fecha de actualizacion 
			command.AppendFormat("{0} top(1) * {1} {2} {3} {4} = {5} order by {6}",
			 Constants.SELECT, Constants.FROM, pageFlowState, Constants.WHERE, requestIdField, requestId, "F_Updated desc");



			#region ejecutar consulta

			//try
			//{
			//  // TODO: validar si la consulta retorna valores
			var TBLReader = dbAcccess.GetCommandReader(command.ToString(), CommandType.Text);
			

			while (TBLReader.Read())
			{
				for (int col = 0; col < TBLReader.FieldCount; col++)
				{
					var idControl = TBLReader.GetName(col).ToString();
					value = new Values()
					{
						key = TBLReader.GetName(col).ToString(),
						value = TBLReader.GetValue(col).ToString()
					};

					result.Add(value);
				}
				break;
			}

			TBLReader.Close();
			dbAcccess.SqlConn.Close();

			//}
			//catch (Exception Ex)
			//{
			//  result.Add(new Values() { key = "error", value = Ex.Message });
			//}

			#endregion ejecutar consulta

			return result;
		}

		public STPC.DynamicForms.Web.Models.Request GetRequestById(int iRequestId)
		{
			Request myRequest = new Request();

			StringBuilder _queryCommand = new StringBuilder();
			_queryCommand.AppendLine("SELECT [RequestId]");
			_queryCommand.AppendLine(",[Created]");
			_queryCommand.AppendLine(",[Updated]");
			_queryCommand.AppendLine(",[FormId]");
			_queryCommand.AppendLine(",[WorkFlowState]");
			_queryCommand.AppendLine(",[PageFlowState]");
			_queryCommand.AppendLine(",[PageFlowId]");
			_queryCommand.AppendLine(",[CreatedBy]");
			_queryCommand.AppendLine(",[UpdatedBy]");
			_queryCommand.AppendLine("FROM [dbo].[Request]");
			_queryCommand.AppendLine("WHERE RequestId = ");
			_queryCommand.AppendLine(iRequestId.ToString());

			RTDataBasesAccess _RTDataBasesAccess = new RTDataBasesAccess();
			SqlDataReader _reader = _RTDataBasesAccess.GetCommandReader(_queryCommand.ToString(), System.Data.CommandType.Text);
			DataTable table = new DataTable();
			//convierto un datareader aun datatable
			table.Load(_reader);
			_reader.Close();
			dbAcccess.SqlConn.Close();

			// validar si la tabla es nula es por que no existen los store procedures
			//try
			//{
			myRequest.CreatedBy = table.Rows[0]["CreatedBy"].ToString();
			myRequest.Created = DateTime.Parse(table.Rows[0]["Created"].ToString());
			myRequest.FormId = Guid.Parse(table.Rows[0]["FormId"].ToString());
			myRequest.PageFlowId = Guid.Parse(table.Rows[0]["PageFlowId"].ToString());
			myRequest.PageFlowState = table.Rows[0]["PageFlowState"].ToString();
			myRequest.RequestId = int.Parse(table.Rows[0]["RequestId"].ToString());
			myRequest.UpdatedBy = table.Rows[0]["UpdatedBy"].ToString();
			myRequest.Updated = DateTime.Parse(table.Rows[0]["Updated"].ToString());

			if (!string.IsNullOrEmpty(table.Rows[0]["WorkFlowState"].ToString()))
				myRequest.WorkFlowState = Guid.Parse(table.Rows[0]["WorkFlowState"].ToString());
			else
				myRequest.WorkFlowState = null;
			//}
			//catch (Exception ex)
			//{
			//	ILogging eventWriter = LoggingFactory.GetInstance();
			//	eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace));
			//	throw new Exception("Could not find stored procedure");
			//}

			return myRequest;
		}


		/// <summary>
		/// Metodo para insertar la respuesta del motor de decisión
		/// </summary>
		public void InsertDecisionEngineResult(System.Xml.XmlElement xmlDecisionEngineResult, int requestId, string updatedBy, string tableToInsert)
		{
			StringBuilder command = null;
			//Carga la lista de nodos del XML
			XmlNodeList _XmlNodeList = xmlDecisionEngineResult.SelectNodes("/application_result");
			StringBuilder columnName = new StringBuilder();
			StringBuilder columnValue = new StringBuilder();
			StringBuilder strbuilderUpdateDate = new StringBuilder();

			columnName.Append("uid, ");

			columnValue.Append("newid(), ");
			columnName.Append("updated, ");
			string dateFormat = System.Configuration.ConfigurationManager.AppSettings["LongDateFormat"];
			columnName.Append("updatedby, ");
			//columnValue.Append(String.Format(dateFormat, DateTime.Now));

			columnValue.Append(string.Format("'{0}',", DateTime.Now.ToString(dateFormat)));
			columnValue.Append(string.Format("'{0}', ", updatedBy));

			columnName.Append("RequestId, ");
			columnValue.Append(string.Format("{0}, ", requestId));

			//Recorre cada uno de los nodos
			int iCountFIelds = 1;
			foreach (XmlNode item in xmlDecisionEngineResult)
			{

				var value = item.LastChild == null ? string.Empty : item.LastChild.InnerText;
				var fieldName = item.Name;

				if (!string.IsNullOrEmpty(value))
				{
					//TODO: Evaluate a better algorithm and based in configuration
					value = Regex.Replace(value, @",", ".");
					columnName.Append(fieldName);
					if (fieldName.ToLower() == "workflowstate" || fieldName.ToLower() == "formtoshow" || fieldName.ToLower() == "pagetoshow")
					{
						columnValue.Append("Convert(uniqueidentifier, '");
						columnValue.Append(value);
						columnValue.Append("')");
					}

					if (fieldName.Substring(0, 3).ToLower() == "str" || fieldName.ToLower() == "errormessage" || fieldName.ToLower() == "messagetodisplay")
					{
						columnValue.Append("'");
						columnValue.Append(value);
						columnValue.Append("'");
					}

					if (fieldName.Substring(0, 3).ToLower() == "int" || fieldName.ToLower() == "errorcode")
					{
						columnValue.Append(value);
					}

					if (iCountFIelds < xmlDecisionEngineResult.ChildNodes.Count)
					{
						columnName.Append(",");
						columnValue.Append(",");
					}
				}

				iCountFIelds++;
			}
			command = new StringBuilder();
			command.AppendFormat("{0} {1}({2}) values ({3}) ", Constants.INSERT, tableToInsert, columnName, columnValue);
			dbAcccess.Post(command.ToString(), CommandType.Text);
			dbAcccess.SqlConn.Close();
		}

		public MyViewModel GetSchemaTable(string tableName, int nodeId)
		{

			SqlDataReader _reader = dbAcccess.GetCommandReader(string.Format("select * from {0} where NodeId={1}", tableName, nodeId), System.Data.CommandType.Text);
			DataTable table = new DataTable();
			//convierto un datareader aun datatable
			table.Load(_reader);
			_reader.Close();
			dbAcccess.SqlConn.Close();

			//Mapea campos que vienen de las tablas dinamicas
			MyViewModel modelQuery = new MyViewModel();
			modelQuery.Rows = new List<RowViewModel>();
			modelQuery.Columns = new List<ColumnViewModel>();

			for (int i = 0; i < table.Columns.Count; i++)
			{
				ColumnViewModel col = new ColumnViewModel();
				string[] roolSeleccionado = null;
				roolSeleccionado = table.Columns[i].ColumnName.Split(new Char[] { '_' });

				if (roolSeleccionado.Length == 2)
					col.Name = roolSeleccionado[0] + " " + roolSeleccionado[1];
				else
					col.Name = table.Columns[i].ColumnName;

				col.dataType = table.Columns[i].DataType.ToString();
				modelQuery.Columns.Add(col);
			}
			for (int i = 0; i < table.Rows.Count; i++)
			{
				DataRow row = table.Rows[i];

				List<CellValueViewModel> listValues = new List<CellValueViewModel>();
				for (int j = 0; j < table.Columns.Count; j++)
				{

					CellValueViewModel cel = new CellValueViewModel();
					cel.Value = row[j].ToString();
					cel.ColumnName = table.Columns[j].Caption;
					listValues.Add(cel);
				}
				RowViewModel _RowViewModel = new RowViewModel();
				_RowViewModel.Values = listValues;
				modelQuery.Rows.Add(_RowViewModel);
			}

			return modelQuery;
		}

		public void InsertAtributesHierarchy(Dictionary<string, string> listFieldsValues, string table)
		{
			string query = "insert into " + table + "(";
			string columns = string.Empty;
			string values = string.Empty;

			int countColumns = 1;
			Dictionary<string, string> dicLocaField = new Dictionary<string, string>();
			dicLocaField = listFieldsValues;
			int countNotNullFieds = dicLocaField.Where(e => e.Value != string.Empty).Count();
			dbAcccess.Parameter = new List<SqlParameter>();

			foreach (var item in listFieldsValues.Where(e => e.Value != string.Empty))
			{

				if (countColumns < countNotNullFieds)
				{
					columns += string.Format("{0},", item.Key);
					values += string.Format("@{0},", SanitizeLiteral(item.Key));
					dbAcccess.Parameter.Add(new SqlParameter { ParameterName = string.Format("@{0}", item.Key), SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = item.Value });
				}
				else
				{
					columns += string.Format("{0}", item.Key);
					values += string.Format("@{0}", item.Key);
					dbAcccess.Parameter.Add(new SqlParameter { ParameterName = string.Format("@{0}", item.Key), SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = item.Value });
				}
				countColumns++;
			}
			query += columns + ") values(" + values + ")";


			dbAcccess.Post(query.ToString(), System.Data.CommandType.Text);
			dbAcccess.SqlConn.Close();

		}

		public void UpdateAtributesHierarchy(Dictionary<string, string> listFieldsValues, string table)
		{
			string query = "update " + table + " set ";
			string columns = string.Empty;
			string values = string.Empty;

			int countColumns = 1;
			Dictionary<string, string> dicLocaField = new Dictionary<string, string>();
			dicLocaField = listFieldsValues;
			int countNotNullFieds = dicLocaField.Where(e => e.Value != string.Empty).Count();
			dbAcccess.Parameter = new List<SqlParameter>();
			string idTable = string.Empty;

			foreach (var item in listFieldsValues)
			{


				if (item.Key == Constants.NAME_FIELD_ID_DEFAULT_ATTR_HIERARCHY)
				{
					idTable = item.Value;
				}

				if (countColumns < countNotNullFieds)
				{
					columns += string.Format("{0}=@{1},", item.Key, item.Key);

					dbAcccess.Parameter.Add(new SqlParameter { ParameterName = string.Format("@{0}", item.Key), SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = item.Value });
				}
				else
				{
					columns += string.Format("{0}=@{1}", item.Key, item.Key);
					dbAcccess.Parameter.Add(new SqlParameter { ParameterName = string.Format("@{0}", item.Key), SqlDbType = System.Data.SqlDbType.VarChar, SqlValue = item.Value });
				}
				countColumns++;
			}
			query += columns + " where " + Constants.NAME_FIELD_ID_DEFAULT_ATTR_HIERARCHY + "=" + idTable;


			dbAcccess.Post(query.ToString(), System.Data.CommandType.Text);
			dbAcccess.SqlConn.Close();
		}

		public int FindRecordIntoAtributeTable(string tableName, int nodeId)
		{

			string query = "select * from " + tableName + " where NodeId=@nodeId";
			dbAcccess.Parameter = new List<SqlParameter>();

			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@nodeId", SqlDbType = System.Data.SqlDbType.Int, SqlValue = nodeId });

			SqlDataReader _reader = dbAcccess.GetCommandReader(query.ToString(), System.Data.CommandType.Text);

			DataTable table = new DataTable();
			//convierto un datareader aun datatable
			table.Load(_reader);
			_reader.Close();
			dbAcccess.SqlConn.Close();

			return table.Rows.Count;
		}

		public List<FormPageActions> GetUserFormPageActionsByState(string userName, Guid pageId, Guid formStateId)
		{
			List<FormPageActions> result = new List<FormPageActions>();
			FormPageActions value = null;

			string pageFlowState = string.Empty;
			string pageFlowId = string.Empty;

			//Se obtiene el ultimo paso

			if (dbAcccess.Parameter == null)
				dbAcccess.Parameter = new List<SqlParameter>();
			else
				dbAcccess.Parameter.Clear();

			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@PageId", SqlDbType = System.Data.SqlDbType.UniqueIdentifier, SqlValue = pageId });
			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@FormStateUId", SqlDbType = System.Data.SqlDbType.UniqueIdentifier, SqlValue = formStateId });
			dbAcccess.Parameter.Add(new SqlParameter { ParameterName = "@UserName", SqlDbType = System.Data.SqlDbType.NVarChar, SqlValue = userName });

			var requestReader = dbAcccess.GetCommandReader("GetUserFormPageActionsByState", CommandType.StoredProcedure);

			Guid? goToPageId;
			Guid? formStatesUid;
			//string successMessage;
			//string failureMessage;
			while (requestReader.Read())
			{
				if (requestReader.IsDBNull(requestReader.GetOrdinal("GoToPageId")))
					goToPageId = null;
				else
					goToPageId = requestReader.GetGuid(9);

				if (requestReader.IsDBNull(requestReader.GetOrdinal("FormStatesUid")))
					formStatesUid = null;
				else
					formStatesUid = requestReader.GetGuid(10);

				//if (requestReader.IsDBNull(14))
				//	successMessage = null;
				//else
				//	successMessage = requestReader.GetString(14);

				//if (requestReader.IsDBNull(15))
				//	failureMessage = null;
				//else
				//	failureMessage = requestReader.GetString(15);

				value = new FormPageActions()
				{
					Uid = requestReader.GetGuid(0),
					Name = requestReader.GetString(1),
					Description = requestReader.GetString(2),
					Caption = requestReader.GetString(3),
					IsAssociated = requestReader.GetBoolean(4),
					IsExecuteStrategy = requestReader.GetBoolean(5),
					PageId = requestReader.GetGuid(6),
					DisplayOrder = requestReader.GetInt32(7),
					Save = requestReader.GetBoolean(8),
					GoToPageId = goToPageId,
					FormStatesUid = formStatesUid,
					ShowSuccessMessage = requestReader.GetBoolean(12),
					ShowFailureMessage = requestReader.GetBoolean(13),
					StrategyID = requestReader.GetInt32(14)
					//SuccessMessage = successMessage,
					//FailureMessage = failureMessage,
				};
				result.Add(value);
			}
			requestReader.Close();
			dbAcccess.SqlConn.Close();

			return result;
		}

		private string SanitizeLiteral(string inputText)
		{
			if (inputText == "True,False")
				inputText = "True";

			return inputText.Replace("'", "''");

		}
	}

	/// <summary>
	/// Clase para guardar la data del formulario y luego convertirlo a XML
	/// </summary>
	public class FormData
	{
		public string FormName { get; set; }

		public Guid FormId { get; set; }

		public string FormPageName { get; set; }

		public Guid FormPageId { get; set; }

		public string PanelName { get; set; }

		public Guid PanelId { get; set; }

		public int RequestId { get; set; }

		public Dictionary<string, string> PageFields { get; set; }

	}
}