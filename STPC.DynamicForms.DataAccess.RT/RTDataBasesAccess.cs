using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace STPC.DynamicForms.DataAccess.RT
{
  public class RTDataBasesAccess
  {

   
    private SqlConnection sqlConn = null;

    public SqlConnection SqlConn
    {
      get { return sqlConn; }
      set { sqlConn = value; }
    }

    private string srStringConnection;

    /// <summary>
    /// Gets or sets the sr string connection.
    /// </summary>
    /// <value>The sr string connection.</value>
    public string SrStringConnection
    {
      get { return srStringConnection; }
      set { srStringConnection = value; }
    }

    /// <summary>
    /// Lista que guarda los parametros para una instruccion sql.
    /// </summary>
    private List<SqlParameter> parameter = null;

    public List<SqlParameter> Parameter
    {
      get { return parameter; }
      set { parameter = value; }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public RTDataBasesAccess()
    {
      RTDataBaseConnection _DBConnection = new RTDataBaseConnection { parameter = this.Parameter };
      _DBConnection.SqlCon = this.SqlConn;

      if (this.SqlConn == null)
        SqlConn=_DBConnection.GetConecttion(getConnectionString());
    }


    /// <summary>
    /// Gets the command.
    /// </summary>
    /// <param name="isrCommand">The isr command.</param>
    /// <returns></returns>
    public List<T> GetCommand<T>(string isrCommand, CommandType _CommandType) where T : class
    {
      RTDataBaseConnection _DBConnection = new RTDataBaseConnection { parameter = this.Parameter };
      _DBConnection.SqlCon = this.SqlConn;

      if (this.SqlConn == null)
        _DBConnection.GetConecttion(getConnectionString());

      return _DBConnection.GetCommand<T>(isrCommand, _CommandType);
    }


    public SqlDataReader GetCommandReader(string isrCommand, CommandType _CommandType)
    {
      RTDataBaseConnection _DBConnection = new RTDataBaseConnection { parameter = this.Parameter };
      _DBConnection.SqlCon = this.SqlConn;
	


      if (this.SqlConn == null)
        _DBConnection.GetConecttion(getConnectionString());

      return _DBConnection.GetCommandReader(isrCommand, _CommandType);
    }

    public List<SqlParameter> SetParameter<T>() where T : new()
    {
      RTDataBaseConnection _DBConnection = new RTDataBaseConnection { parameter = this.Parameter };
      this.Parameter = _DBConnection.SetParameter<T>();
      _DBConnection.parameter = this.Parameter;
      return this.Parameter;
    }

    /// <summary>
    /// Posts the specified isr command.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="isrCommand">Instrucción sql o nombre del procedimiento almacenado</param>
    /// <param name="_CommandType">Tipo de comando a ejecutar.</param>
    /// <param name="arrListGeneric">Objeto que contiene los datos a insertar.</param>
    public void Post<T>(string isrCommand, CommandType _CommandType, T arrListGeneric) where T : new()
    {
      RTDataBaseConnection _DBConnection = new RTDataBaseConnection { parameter = this.Parameter };
      _DBConnection.SqlCon = this.SqlConn;

      if (_DBConnection.parameter == null)
        _DBConnection.parameter = SetParameter<T>();

      _DBConnection.Post<T>(isrCommand, _CommandType, arrListGeneric);

      _DBConnection.parameter = null;
      this.Parameter = null;
    }

    /// <summary>
    /// Posts the specified isr command.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="isrCommand">Instrucción sql o nombre del procedimiento almacenado</param>
    /// <param name="_CommandType">Tipo deocmando a ejecutar.</param>
    /// <param name="arrListGeneric">Objeto que contiene los datos a insertar.</param>
    public void Post(string isrCommand, CommandType _CommandType)
    {
      RTDataBaseConnection _DBConnection = new RTDataBaseConnection { parameter = this.Parameter };
      _DBConnection.GetConecttion(getConnectionString());
      _DBConnection.Post(isrCommand, _CommandType);
      _DBConnection.parameter = null;
      this.Parameter = null;
    }

    /// <summary>
    /// Retorna la cadena de conección de la instancia actual.
    /// </summary>
    /// <returns></returns>
    public string getConnectionString()
    {
      return SrStringConnection = ConfigurationManager.ConnectionStrings["lisimabcdb"].ToString();

    }

    /// <summary>
    /// Agrega parametro a la consulta que se realiza en el getCommand
    /// </summary>
    /// <param name="key"></param>
    /// <param name="Value"></param>
    public void addParameter(SqlDbType type, object Value)
    {
      Parameter.Add(new SqlParameter { SqlDbType = type, SqlValue = Value });
    }
  }
}
