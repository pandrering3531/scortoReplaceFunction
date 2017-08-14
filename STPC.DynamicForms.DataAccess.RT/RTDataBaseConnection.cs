using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;


namespace STPC.DynamicForms.DataAccess.RT
{
	public class RTDataBaseConnection
	{
		public List<SqlParameter> parameter = null;
		private SqlConnection sqlCon = null;
		private SqlConnection sqlConLocal = null;

		public SqlConnection SqlConLocal
		{
			get { return sqlConLocal; }
			set { sqlConLocal = value; }
		}

		public SqlConnection SqlCon
		{
			get { return sqlCon; }
			set { sqlCon = value; }
		}

		/// <summary>
		/// Método que retorna un objeto de tipo sqlConection
		/// </summary>
		/// <param name="iSrStingConection"></param>
		/// <returns></returns>
		public SqlConnection GetConecttion(string iSrConection)
		{
			SqlConLocal = new SqlConnection(iSrConection);

			if (SqlConLocal.State == System.Data.ConnectionState.Closed)
				SqlConLocal.Open();

			return SqlConLocal;
		}

		/// <summary>
		/// Gets the list.
		/// </summary>
		/// <param name="srCommandText">The sr command text.</param>
		/// <param name="_CommandType">Type of the _ command.</param>
		/// <returns></returns>
		public List<T> GetCommand<T>(string srCommandText, System.Data.CommandType _CommandType) where T : class
		{
			SqlCommand _SqlCommand = null;
			if (SqlCon == null || SqlCon.State == System.Data.ConnectionState.Closed)
			{
				_SqlCommand = new SqlCommand(srCommandText, SqlConLocal);
			}
			else
				_SqlCommand = new SqlCommand(srCommandText, SqlCon);

			_SqlCommand.CommandType = _CommandType;
			_SqlCommand.CommandTimeout = 0;
			if (parameter != null)

				foreach (SqlParameter item in parameter)
				{
					_SqlCommand.Parameters.Add(item);
				}
			SqlDataReader _reader = null;
			try
			{
				_reader = _SqlCommand.ExecuteReader();
			}
			catch (Exception)
			{
				if (SqlConLocal != null && SqlConLocal.State == ConnectionState.Open)
					SqlConLocal.Close();
				if (SqlCon != null && SqlCon.State == ConnectionState.Open)
					SqlCon.Close();

			}
			//GenericReader genericReader = new GenericReader();
			List<T> arrGenericList = new List<T>();

			//arrGenericList = genericReader.CopyDataReaderToEntity<T>(_reader);
			//SqlCon.Close();
			_reader.Close();

			if (SqlConLocal != null)
			{
				SqlConLocal.Close();
			}
			return arrGenericList;
		}

		public SqlDataReader GetCommandReader(string srCommandText, System.Data.CommandType _CommandType)
		{
			SqlCommand _SqlCommand = null;
			if (SqlCon == null || SqlCon.State == System.Data.ConnectionState.Closed)
			{
				_SqlCommand = new SqlCommand(srCommandText, SqlConLocal);
			}
			else
				_SqlCommand = new SqlCommand(srCommandText, SqlCon);

			_SqlCommand.CommandType = _CommandType;
			_SqlCommand.CommandTimeout = 0;
			if (parameter != null)

				foreach (SqlParameter item in parameter)
				{
					_SqlCommand.Parameters.Add(item);
				}
			SqlDataReader _reader = null;
			try
			{
				_reader = _SqlCommand.ExecuteReader();

				//GenericReader genericReader = new GenericReader();
			}
			catch (Exception ex)
			{
				if (SqlConLocal != null && SqlConLocal.State == ConnectionState.Open)
					SqlConLocal.Close();
				if (SqlCon != null && SqlCon.State == ConnectionState.Open)
					SqlCon.Close();
			}

			return _reader;
		}

		/// <summary>
		/// Construye dinamicamente la lista de parametros con base en una lista llena de datos
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="arrGeneric"></param>
		public List<SqlParameter> SetParameter<T>() where T : new()
		{
			FieldInfo[] myFieldInfo;

			Type type = typeof(T);
			myFieldInfo = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance
					| BindingFlags.Public);

			for (int iCounField = 0; iCounField < myFieldInfo.Length; iCounField++)
			{
				if (parameter == null)
					parameter = new List<SqlParameter>();

				System.Data.SqlDbType _SqlDbType = new System.Data.SqlDbType();
				getType(myFieldInfo[iCounField].FieldType.FullName, ref _SqlDbType);
				parameter.Add(new SqlParameter { SqlValue = myFieldInfo.GetValue(0), SqlDbType = _SqlDbType, ParameterName = myFieldInfo[iCounField].Name });
			}
			return parameter;
		}

		/// Realiza un proceso de inserción sobre cualquier tabla
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="srCommandText"></param>
		/// <param name="_CommandType"></param>
		/// <param name="IEntity"></param>
		public void Post<T>(string srCommandText, System.Data.CommandType _CommandType, T objGeneric) where T : new()
		{
			SqlCommand _SqlCommand = null;
			if (SqlCon == null || SqlCon.State == System.Data.ConnectionState.Closed)
			{
				_SqlCommand = new SqlCommand(srCommandText, SqlConLocal);
			}
			else
				_SqlCommand = new SqlCommand(srCommandText, SqlCon);

			_SqlCommand.CommandType = _CommandType;
			_SqlCommand.CommandTimeout = 0;
			if (parameter != null)
				foreach (SqlParameter item in parameter)
				{
					item.Value = GetValueParameter<T>(item.ParameterName, objGeneric);
					item.ParameterName = "@" + item.ParameterName;
					_SqlCommand.Parameters.Add(item);
				}

			try
			{
				int rows = _SqlCommand.ExecuteNonQuery();
			}
			catch (Exception)
			{

				if (SqlConLocal != null && SqlConLocal.State == ConnectionState.Open)
					SqlConLocal.Close();
				if (SqlCon != null && SqlCon.State == ConnectionState.Open)
					SqlCon.Close();
			}

			if (SqlConLocal != null)
			{
				SqlConLocal.Close();
			}


		}

		public void Post(string srCommandText, System.Data.CommandType _CommandType)
		{
			SqlCommand _SqlCommand = null;
			if (SqlCon == null || SqlCon.State == System.Data.ConnectionState.Closed)
			{
				_SqlCommand = new SqlCommand(srCommandText, SqlConLocal);
			}
			else
				_SqlCommand = new SqlCommand(srCommandText, SqlCon);

			_SqlCommand.CommandType = _CommandType;

			if (parameter != null)
				foreach (SqlParameter item in parameter)
				{
					_SqlCommand.Parameters.Add(item);
				}

			try
			{
				int rows = _SqlCommand.ExecuteNonQuery();
			}
			catch (Exception)
			{

				if (SqlConLocal != null && SqlConLocal.State == ConnectionState.Open)
					SqlConLocal.Close();
				if (SqlCon != null && SqlCon.State == ConnectionState.Open)
					SqlCon.Close();
			}
			_SqlCommand.Connection.Close();

			if (SqlConLocal != null)
			{
				SqlConLocal.Close();
			}
		}

		public void getType(string srDataType, ref System.Data.SqlDbType _SqlDbType)
		{
			switch (srDataType)
			{
				case "System.Nullable`1[System.Int16]":
					_SqlDbType = System.Data.SqlDbType.Int;
					break;
				case "System.Nullable`1[System.Int32]":
					_SqlDbType = System.Data.SqlDbType.Int;
					break;
				case "System.Nullable`1[System.Int64]":
					_SqlDbType = System.Data.SqlDbType.Int;
					break;
				case "System.Int16":
					_SqlDbType = System.Data.SqlDbType.Int;
					break;
				case "System.Int32":
					_SqlDbType = System.Data.SqlDbType.Int;
					break;
				case "System.Int64":
					_SqlDbType = System.Data.SqlDbType.Int;
					break;
				case "System.Double":
					_SqlDbType = System.Data.SqlDbType.Float;
					break;
				case "System.String":
					_SqlDbType = System.Data.SqlDbType.VarChar;
					break;
				case "System.DateTime":
					_SqlDbType = System.Data.SqlDbType.DateTime;
					break;
				case "System.Guid":
					_SqlDbType = System.Data.SqlDbType.UniqueIdentifier;
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Obtiene el valor de la lista y lo asigna al parametro
		/// </summary>
		/// <param name="srNameParameter"></param>
		/// <returns></returns>
		public object GetValueParameter<T>(string srNameParameter, T objGeneric) where T : new()
		{
			FieldInfo myFieldInfo;
			Type myType = typeof(T);
			// Get the type and fields of FieldInfoClass.
			myFieldInfo = myType.GetField(srNameParameter, BindingFlags.NonPublic | BindingFlags.Instance
				 | BindingFlags.Public);
			object objValueParameter = myFieldInfo.GetValue(objGeneric);

			return objValueParameter;
		}
	}
}
