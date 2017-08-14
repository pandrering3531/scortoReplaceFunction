using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.Web.Models
{
	public class ScriptGenerator
	{
		#region Atributos y Constantes

		STPC_FormsFormEntities _stpcForms;

		#endregion Atributos y Constantes

		#region Propiedades

		#endregion Propiedades

		#region Constructor

		public ScriptGenerator()
		{
			this._stpcForms = new STPC_FormsFormEntities(); // new STPC_FormsFormEntities(new Uri("http://localhost:6615/FormsPersistenceService.svc/"));
		}

		/// <summary>
		/// Constructor Parametrizado
		/// Initializes a new instance of the <see cref="ScriptGenerator" /> class.
		/// </summary>
		/// <param name="forms">The forms.</param>
		public ScriptGenerator(STPC_FormsFormEntities forms)
		{
			this._stpcForms = forms;
		}

		#endregion

		#region Metodos Privados
		#endregion

		#region Metodos Publicos

		/// <summary>
		/// Generates the script string.
		/// </summary>
		/// <param name="formId">The form id.</param>
		/// <returns></returns>
		public string GenerateScriptString(Guid formId)
		{
			StringBuilder sb = new StringBuilder();
			try
			{
				// obtener el form a partir del GUID
				var thisForm = _stpcForms.Forms.Where(fr => fr.Uid == formId).FirstOrDefault();

				// generar script para la tabla principal
				// se elimina el script de la tabla principal => pasa a ser parte del modelo REQUEST
				//sb.Append(this.GenerateMainTableScript(thisForm.Uid));

				// generar script para los store procedures
				sb.Append(this.GenerateSPInsertRequest());
				sb.Append(this.GenerateSPSelectRequestById());
				sb.Append(this.GenerateSPUpdateRequest());

				// consultar las paginas hijas
				thisForm.Pages = _stpcForms.FormPages.Where(fp => fp.Form.Uid == formId).ToList();

				// generar el script para cada pagina hija 
				foreach (FormPage fp in thisForm.Pages)
				{
					sb.Append(this.GenerateTableScript(fp.Uid, GetMainTableTableSufix(thisForm.Uid)));
				}
			}
			catch (Exception ex)
			{
				sb.Append(ex.Message);
			}
			return sb.ToString();
		}

		#endregion

		#region Store Procedures

		/// <summary>
		/// Generates the SP insert request.
		/// </summary>
		/// <returns></returns>
		public string GenerateSPInsertRequest()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(Environment.NewLine);
			sb.AppendLine("/**** Begin of STORE PROCEDURE - Insert_Request *****/");
			sb.Append(Environment.NewLine);

			// IF EXISTS Block
			sb.AppendLine("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Insert_Request]') AND type in (N'P', N'PC'))");
			sb.AppendLine("DROP PROCEDURE [dbo].[Insert_Request]");
			sb.AppendLine("GO");

			sb.Append(Environment.NewLine);

			// CREATE Block
			sb.AppendLine("CREATE PROCEDURE Insert_Request");
			sb.AppendLine("@FormId uniqueidentifier,");
			sb.AppendLine("@WorkFlowState nvarchar(255) = null,");
			sb.AppendLine("@PageFlowState nvarchar (255) = null,");
			sb.AppendLine("@PageFlowId uniqueidentifier = null,");
			sb.AppendLine("@Created datetime,");
			sb.AppendLine("@CreatedBy nvarchar(255),");
			sb.AppendLine("@Updated datetime,");
			sb.AppendLine("@UpdatedBy nvarchar(255)= null,");
			sb.AppendLine("@AssignedTo nvarchar(255)= null");
			sb.AppendLine("@AplicationNameId int= null");
			sb.AppendLine("AS");

			// BEGIN Block
			sb.AppendLine(ScriptGeneratorConstants.BEGIN);
			sb.AppendLine("insert into [dbo].[Request](");
			sb.AppendLine("Created,");
			sb.AppendLine("Updated,");
			sb.AppendLine("FormId,");
			sb.AppendLine("WorkFlowState,");
			sb.AppendLine("PageFlowState,");
			sb.AppendLine("PageFlowId,");
			sb.AppendLine("CreatedBy,");
			sb.AppendLine("UpdatedBy,");
			sb.AppendLine("AssignedTo,");
			sb.AppendLine("AplicationNameId");
			sb.AppendLine(")");
			sb.AppendLine("values(");
			sb.AppendLine("@Created,");
			sb.AppendLine("@Updated,");
			sb.AppendLine("@FormId,");
			sb.AppendLine("@WorkFlowState,");
			sb.AppendLine("@PageFlowState,");
			sb.AppendLine("@PageFlowId,");
			sb.AppendLine("@CreatedBy,");
			sb.AppendLine("@UpdatedBy,");
			sb.AppendLine("@AssignedTo,");
			sb.AppendLine("@AplicationNameId");
			sb.AppendLine(")");
			sb.AppendLine("SELECT SCOPE_IDENTITY() as RequestId");

			// -----------------------------------------

			//END BLOCK
			sb.AppendLine("END");
			sb.AppendLine("GO");
			sb.AppendLine(Environment.NewLine);
			sb.AppendLine("/**** End of STORE PROCEDURE - Insert_Request *****/");
			return sb.ToString();
		}

		/// <summary>
		/// Generates the SP select request by id.
		/// </summary>
		/// <returns></returns>
		public string GenerateSPSelectRequestById()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(Environment.NewLine);
			sb.AppendLine("/**** Begin of STORE PROCEDURE - Select_RequestById *****/");
			sb.AppendLine(Environment.NewLine);

			// IF EXISTS Block
			sb.AppendLine("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Select_RequestById]') AND type in (N'P', N'PC'))");
			sb.AppendLine("DROP PROCEDURE [dbo].[Select_RequestById]");
			sb.AppendLine("GO");

			sb.Append(Environment.NewLine);

			// CREATE Block
			sb.AppendLine("CREATE PROCEDURE Select_RequestById");
			sb.AppendLine("@RequestId int");
			sb.AppendLine("AS");

			// BEGIN Block
			sb.AppendLine(ScriptGeneratorConstants.BEGIN);
			sb.AppendLine("SELECT RequestId,");
			sb.AppendLine("Created,");
			sb.AppendLine("Updated,");
			sb.AppendLine("FormId,");
			sb.AppendLine("WorkFlowState,");
			sb.AppendLine("PageFlowState,");
			sb.AppendLine("PageFlowId,");
			sb.AppendLine("CreatedBy,");
			sb.AppendLine("UpdatedBy");
			sb.AppendLine("from Request");
			sb.AppendLine("where RequestId = @RequestId");

			//END BLOCK
			sb.AppendLine("END");
			sb.AppendLine("GO");
			sb.AppendLine(Environment.NewLine);
			sb.AppendLine("/**** End of STORE PROCEDURE - Select_RequestById *****/");
			return sb.ToString();
		}

		/// <summary>
		/// Generates the SP update request.
		/// </summary>
		/// <returns></returns>
		public string GenerateSPUpdateRequest()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(Environment.NewLine);
			sb.AppendLine("/**** Begin of STORE PROCEDURE - Update_Request *****/");
			sb.AppendLine(Environment.NewLine);

			// IF EXISTS Block
			sb.AppendLine("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Update_Request]') AND type in (N'P', N'PC'))");
			sb.AppendLine("DROP PROCEDURE [dbo].[Update_Request]");
			sb.AppendLine("GO");

			sb.Append(Environment.NewLine);

			// CREATE Block
			sb.AppendLine("CREATE PROCEDURE Update_Request");
			sb.AppendLine("@RequestId int,");
			sb.AppendLine("@PageFlowState varchar(255),");
			sb.AppendLine("@PageFlowId uniqueidentifier,");
			sb.AppendLine("@Updated datetime,");
			sb.AppendLine("@UpdatedBy varchar(255),");
			sb.AppendLine("@WorkFlowState varchar(255)");
			sb.AppendLine("AS");

			// BEGIN Block
			sb.AppendLine(ScriptGeneratorConstants.BEGIN);
			sb.AppendLine("Update [dbo].[Request]	");
			sb.AppendLine("set PageFlowState = @PageFlowState,");
			sb.AppendLine("PageFlowId = @PageFlowId,");
			sb.AppendLine("UpdatedBy = @UpdatedBy,");
			sb.AppendLine("Updated = GETDATE(),");
			sb.AppendLine("WorkFlowState=@WorkFlowState");
			sb.AppendLine("where [RequestId] = @RequestId");

			//END BLOCK
			sb.AppendLine("END");
			sb.AppendLine("GO");
			sb.AppendLine(Environment.NewLine);
			sb.AppendLine("/**** End of STORE PROCEDURE - Update_Request *****/");
			return sb.ToString();

		}

		public string GenerateNotExistsProcedure(string StoreProcedureName)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[");
			sb.Append(StoreProcedureName);
			sb.Append("]') AND type in (N'P', N'PC'))");
			sb.AppendLine("DROP PROCEDURE [dbo].[");
			sb.Append(StoreProcedureName);
			sb.Append("]");
			sb.Append(Environment.NewLine);
			sb.AppendLine("GO");
			sb.Append(Environment.NewLine);

			return sb.ToString();
		}

		#endregion Store Procedures

		public string GenerateTableScript(Guid formPageId, string formName)
		{
			StringBuilder sb = new StringBuilder();
			// Get name
			string tablename = this.GetTableName(formPageId, formName);
			sb.Append(Environment.NewLine);
			sb.Append(Environment.NewLine);
			sb.Append("/**** Begin of ");
			sb.Append(tablename);
			sb.Append(" Table *****/");
			sb.Append(Environment.NewLine);

			// If NOT Object exists section
			//sb.Append(this.GenerateObjectExistsCheck(tablename));
			sb.Append(this.GenerateNotExistsTable(tablename));
			sb.Append(Environment.NewLine);

			// BEGIN Block
			sb.Append(ScriptGeneratorConstants.BEGIN);
			sb.Append(Environment.NewLine);
			// CreateTable
			sb.Append("CREATE TABLE [dbo].[");
			sb.Append(tablename);
			sb.Append("](");
			sb.Append(Environment.NewLine);

			// CONSTANT FIELDS
			sb.Append("[");
			sb.Append(GetFieldName(ScriptGeneratorConstants.PRIMARY_KEY_FIELD));
			sb.Append("] [uniqueidentifier] NOT NULL,");
			sb.Append(Environment.NewLine);
			sb.AppendLine("[F_Updated] [datetime] NULL,");
			sb.AppendLine("[F_UpdatedBy] [varchar](255) NULL,");

			//var panels = _stpcForms.Panels.Expand("PanelFields").Where(x => x.Page.Uid == formPageId);
			var panels = _stpcForms.Panels.Where(x => x.Page.Uid == formPageId);

			// COMMONKEYFIELD [RequestId] 
			sb.Append("[");
			sb.Append(GetFieldName(ScriptGeneratorConstants.COMMONKEYFIELD));
			sb.Append("] [int] NOT NULL,");
			sb.Append(Environment.NewLine);

            string fieldError = string.Empty;
			// PANEL ITERATION
			foreach (Panel p in panels)
			{
				try
				{
					// FIELD ITERATION
					//var fields = _stpcForms.PageFields.Where(y => y.Panel.Uid == p.Uid);
					p.PanelFields = _stpcForms.PageFields.Where(y => y.Panel.Uid == p.Uid).OrderBy(f => f.FormFieldName).ToList();

					foreach (PageField pf in p.PanelFields)
					{
						var ft = _stpcForms.PageFieldTypes.Where(t => t.Uid == pf.FormFieldType_Uid).FirstOrDefault();
                        fieldError = pf.FormFieldName;
						// validar el campo "espacio en blanco"
						if (!ft.ControlType.Equals("Blank") && !ft.ControlType.Equals("Literal"))
						{
							sb.Append(this.GenerateFieldScript(pf, ft));
							sb.Append(Environment.NewLine);
						}
					}

				}
				catch (Exception ex)
				{
					throw;
				}
			}

			sb.Append("CONSTRAINT [PK_dbo.");
			sb.Append(tablename);
			sb.Append("] PRIMARY KEY CLUSTERED (");
			sb.Append(Environment.NewLine);
			sb.Append("[");
			sb.Append(GetFieldName(ScriptGeneratorConstants.PRIMARY_KEY_FIELD));
			sb.Append("] ASC)");
			sb.Append(Environment.NewLine);
			sb.Append("WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ");
			sb.Append(Environment.NewLine);
			sb.Append(")");
			sb.Append(Environment.NewLine);
			//END BLOCK
			sb.Append(ScriptGeneratorConstants.END);
			sb.Append(Environment.NewLine);
			sb.Append("/**** End of ");
			sb.Append(tablename);
			sb.Append(" Table *****/");
			return sb.ToString();
		}

		public string GenerateMainTableScript(Guid formId)
		{
			StringBuilder sb = new StringBuilder();
			// Get name
			string tablename = this.GetMainTableName(formId);
			sb.Append(Environment.NewLine);
			sb.Append("/**** Begin of Main Table *****/");
			sb.Append(Environment.NewLine);
			// If NOT Object exists section
			sb.Append(this.GenerateObjectExistsCheck(tablename));
			// BEGIN Block
			sb.Append(ScriptGeneratorConstants.BEGIN);
			sb.Append(Environment.NewLine);
			// CreateTable
			sb.Append("CREATE TABLE [dbo].[");
			sb.Append(tablename);
			sb.Append("](");
			sb.Append(Environment.NewLine);
			sb.Append("[");
			sb.Append(ScriptGeneratorConstants.COMMONKEYFIELD);
			sb.Append("] [uniqueidentifier] NOT NULL,");
			sb.Append(Environment.NewLine);
			sb.Append("[Created] [datetime] NOT NULL,");
			sb.Append(Environment.NewLine);
			sb.Append("[Updated] [datetime] NOT NULL,");
			sb.Append(Environment.NewLine);
			sb.Append("[FormId] [uniqueidentifier] NOT NULL,");
			sb.Append(Environment.NewLine);
			//Index Block
			sb.Append("CONSTRAINT [PK_dbo.");
			sb.Append(tablename);
			sb.Append("] PRIMARY KEY CLUSTERED (");
			sb.Append(Environment.NewLine);
			sb.Append("[");
			sb.Append(ScriptGeneratorConstants.COMMONKEYFIELD);
			sb.Append("] ASC)");
			sb.Append(Environment.NewLine);
			sb.Append("WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]");
			sb.Append(Environment.NewLine);
			sb.Append(") ON [PRIMARY]");
			sb.Append(Environment.NewLine);
			//END BLOCK
			sb.Append(ScriptGeneratorConstants.END);
			sb.Append(Environment.NewLine);
			sb.Append("/**** End of Main Table *****/");
			sb.Append(Environment.NewLine);
			sb.Append(Environment.NewLine);
			return sb.ToString();
		}

		public string GetMainTableName(Guid formId)
		{
			StringBuilder sb = new StringBuilder();
			var thisForm = _stpcForms.Forms.Where(form => form.Uid == formId).FirstOrDefault();
			//var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == formId).FirstOrDefault();
			//Add prefix
			sb.Append(ScriptGeneratorConstants.MAINTABLEPREFIX);
			sb.Append(thisForm.Name.Replace(" ", ""));
			return sb.ToString();
		}

		public string GetMainTableTableSufix(Guid formId)
		{
			StringBuilder sb = new StringBuilder();
			var thisForm = _stpcForms.Forms.Where(form => form.Uid == formId).FirstOrDefault();
			//Add prefix
			sb.Append(thisForm.Name.Replace(" ", ""));
			return sb.ToString();
		}

		public string GenerateObjectExistsCheck(string objectName)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[");
			sb.Append(objectName);
			sb.Append("]') AND type in (N'U'))");
			sb.Append(Environment.NewLine);
			return sb.ToString();
		}

		public string GenerateNotExistsTable(string TableName)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[");
			sb.Append(TableName);
			sb.Append("]') AND type in (N'U'))");
			sb.Append(Environment.NewLine);
			sb.Append("DROP TABLE [dbo].[");
			sb.Append(TableName);
			sb.Append("]");
			sb.Append(Environment.NewLine);
			sb.Append("GO");
			sb.Append(Environment.NewLine);
			return sb.ToString();
		}

		public string GetTableName(Guid FormPageId, string formName)
		{
			StringBuilder sb = new StringBuilder();
			var thisformpage = _stpcForms.FormPages.Where(formp => formp.Uid == FormPageId).FirstOrDefault();
			//Add prefix
			sb.Append(ScriptGeneratorConstants.TABLEPREFIX);
			sb.Append(thisformpage.Name.Replace(" ", ""));
			sb.Append("_");
			sb.Append(formName);
			return sb.ToString();
		}

		public string GetFieldName(Guid PageFieldId)
		{
			StringBuilder sb = new StringBuilder();
			var thisfield = _stpcForms.PageFields.Where(formp => formp.Uid == PageFieldId).FirstOrDefault();
			//Add prefix
			sb.Append(ScriptGeneratorConstants.FIELDPREFIX);
			sb.Append(thisfield.FormFieldName.Replace(" ", ""));
			return sb.ToString();
		}

		public string GetFieldName(string iName)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(ScriptGeneratorConstants.FIELDPREFIX);
			sb.Append(iName.Replace(" ", ""));
			return sb.ToString();
		}

		public string GenerateFieldScript(PageField pf, PageFieldType ft)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("[");
			sb.Append(GetFieldName(pf.FormFieldName));
			sb.Append("] ");
			// 27/03/2013
			// se modifica la siguiente linea
			//sb.Append(this.GetFieldTypeScriptElement(ft));
			// para que el generador de scripts no genere los campos
			// con el tamaño maximo sino con el ingresado por el usuario

			int n;
			bool isNumeric = int.TryParse(pf.MaxSize, out n);



			sb.Append(this.GetFieldSize(pf, ft));
			//if (pf.IsRequired)//Se quita obligatoriedad de los campos
			//sb.Append("NOT NULL");
			sb.Append(",");
			return sb.ToString();
		}

		public string GetFieldTypeScriptElement(PageFieldType ft)
		{
			StringBuilder sb = new StringBuilder();
			switch (ft.FieldTypeName)
			{
				case FieldTypeNames.TEXTO:
					sb.Append(" [nvarchar](");
					sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.TEXTAREA:
					sb.Append(" [nvarchar](");
					sb.Append(ScriptGeneratorConstants.HUGESTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.CORREO:
					sb.Append(" [nvarchar](");
					sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.FILE:
					sb.Append(" [nvarchar](");
					sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.LINK:
					sb.Append(" [nvarchar](");
					sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.LISTAMULTIPLE:
					sb.Append(" [nvarchar](");
					sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.LISTASELECT:
					sb.Append(" [nvarchar](");
					sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.LISTAUNICA:
					sb.Append(" [nvarchar](");
					sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.LITERAL:
					sb.Append(" [nvarchar](");
					sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.NUMERO:
					sb.Append(" [int] ");
					break;
				case FieldTypeNames.DECIMAL:
					sb.Append(" [float] ");
					break;
				case FieldTypeNames.FECHA:
					sb.Append(" [datetime] ");
					break;
				case FieldTypeNames.CHECK:
					sb.Append(" [bit] ");
					break;
				case FieldTypeNames.MONEDA:
					sb.Append(" [money] ");
					break;
			}
			return sb.ToString();
		}

		public string GetFieldSize(PageField pf, PageFieldType ft)
		{
			StringBuilder sb = new StringBuilder();


			switch (ft.FieldTypeName)
			{
				case FieldTypeNames.TEXTO:
				case FieldTypeNames.SOLO_TEXTO:
					sb.Append(" [nvarchar](");


					if (string.IsNullOrEmpty(pf.MaxSizeBD))
						sb.Append(pf.MaxSize);
					else
					{
						if (!string.IsNullOrEmpty(pf.MaxSize))
						{
							if (Convert.ToInt64(pf.MaxSizeBD) < Convert.ToInt64(pf.MaxSize))
							{
								sb.Append(pf.MaxSizeBD);
							}
							else
							{
								sb.Append(pf.MaxSize);
							}
						}
						else
							sb.Append(pf.MaxSizeBD);
					}
					//sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.TEXTAREA:
					sb.Append(" [nvarchar](");
					if (string.IsNullOrEmpty(pf.MaxSizeBD))
						sb.Append(pf.MaxSize);
					else
					{
						if (!string.IsNullOrEmpty(pf.MaxSize))
						{
							if (Convert.ToInt64(pf.MaxSizeBD) < Convert.ToInt64(pf.MaxSize))
							{
								sb.Append(pf.MaxSizeBD);
							}
							else
							{
								sb.Append(pf.MaxSize);
							}
						}
						else
							sb.Append(pf.MaxSizeBD);
					}
					sb.Append(") ");
					break;
				case FieldTypeNames.CORREO:
					sb.Append(" [nvarchar](");
					if (string.IsNullOrEmpty(pf.MaxSizeBD))
						sb.Append(pf.MaxSize);
					else
					{
						if (!string.IsNullOrEmpty(pf.MaxSize))
						{
							if (Convert.ToInt64(pf.MaxSizeBD) < Convert.ToInt64(pf.MaxSize))
							{
								sb.Append(pf.MaxSizeBD);
							}
							else
							{
								sb.Append(pf.MaxSize);
							}
						}
						else
							sb.Append(pf.MaxSizeBD);
					}
					//sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.FILE:
					sb.Append(" [nvarchar](");
					//if (!string.IsNullOrEmpty(pf.MaxSize)) sb.Append(pf.MaxSize);
					//else sb.Append(pf.MaxSizeBD);
					sb.Append(pf.MaxSizeBD);
					//sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.LINK:
					sb.Append(" [nvarchar](");
					if (string.IsNullOrEmpty(pf.MaxSizeBD))
						sb.Append(pf.MaxSize);
					else
					{
						if (!string.IsNullOrEmpty(pf.MaxSize))
						{
							if (Convert.ToInt64(pf.MaxSizeBD) < Convert.ToInt64(pf.MaxSize))
							{
								sb.Append(pf.MaxSizeBD);
							}
							else
							{
								sb.Append(pf.MaxSize);
							}
						}
						else
							sb.Append(pf.MaxSizeBD);
					}
					//sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.LISTAMULTIPLE:
					sb.Append(" [nvarchar](");
					sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.LISTASELECT:
					sb.Append(" [nvarchar](");
					sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.LISTAUNICA:
					sb.Append(" [nvarchar](");
					sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.LITERAL:
					sb.Append(" [nvarchar](");
					if (string.IsNullOrEmpty(pf.MaxSizeBD))
						sb.Append(pf.MaxSize);
					else
					{
						if (!string.IsNullOrEmpty(pf.MaxSize))
						{
							if (Convert.ToInt64(pf.MaxSizeBD) < Convert.ToInt64(pf.MaxSize))
							{
								sb.Append(pf.MaxSizeBD);
							}
							else
							{
								sb.Append(pf.MaxSize);
							}
						}
						else
							sb.Append(pf.MaxSizeBD);
					}
					//sb.Append(ScriptGeneratorConstants.LONGSTRINGLEN);
					sb.Append(") ");
					break;
				case FieldTypeNames.NUMERO:
					sb.Append(" [numeric](11,0)");
					//sb.Append(" [int] ");
					break;
				case FieldTypeNames.DECIMAL:
					sb.Append(" [float] ");
					break;
				case FieldTypeNames.FECHA:
					sb.Append(" [datetime] ");
					break;
				case FieldTypeNames.CHECK:
					sb.Append(" [bit] ");
					break;
				case FieldTypeNames.MONEDA:
					sb.Append(" [money]");
					break;
			}

			return sb.ToString();
		}

	}
}


/**********************************************************

public static class FieldTypeNames
{
  public const string TEXTO = "Texto";
  public const string NUMERO = "Numero";
  public const string DECIMAL = "Decimal";
  public const string FECHA = "Fecha";
  public const string CORREO = "Correo Electronico";
  public const string LISTAUNICA = "Lista de opciones unicas";
  public const string LISTAMULTIPLE = "Lista de opciones multiples";
  public const string TEXTAREA = "Area de texto";
  public const string LISTASELECT = "Lista de seleccion";
  public const string CHECK = "Caja de chequeo";
  public const string FILE = "Subir Archivo";
  public const string LITERAL = "Texto literal";
  public const string LINK = "Hipervinculo";
}

**********************************************************/

public struct ScriptGeneratorConstants
{
	public const string MAINTABLEPREFIX = "MAIN_";
	public const string TABLEPREFIX = "TBL_";
	public const string FIELDPREFIX = "F_";
	public const string BEGIN = "BEGIN";
	public const string END = "END";
	public const string SMALLSTRINGLEN = "255";
	public const string LONGSTRINGLEN = "512";
	public const string HUGESTRINGLEN = "MAX";
	public const string TINYSTRINGLEN = "127";
	public const string COMMONKEYFIELD = "RequestId";
	public const string PRIMARY_KEY_FIELD = "PageFlowId";
}