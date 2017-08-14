using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using STPC.DynamicForms.Core.Fields;
using STPC.DynamicForms.Core.Utilities;
using System.Xml.Linq;

namespace STPC.DynamicForms.Core
{
    /// <summary>
    /// Represents an html input form that can be dynamically rendered at runtime.
    /// </summary>

    [Serializable]
    [ModelBinder(typeof(DynamicFormModelBinder))]
    public class Form
    {
        #region attributes & constants

        private string _formWrapper = Constants.FR_formWrapper;
        private string _formWrapperClass = Constants.FR_formWrapperClass;
        private string _fieldPrefix = Constants.FR_fieldPrefix;


        private FieldList _fields;

        #endregion attributes & constants

        #region Public properties

        public int NumColumnsPanel;
        public Guid PanelId;
        public string Template { get; set; }
        public string panelCaption = string.Empty;
        public string DivCssStyle;
        public bool isDisabled;
        public bool isHiden;

        /////////////////////////////////
        //Se adiciona el parametro panelType para identificar si es un panel de detalle o es normal.
        //Por: Jorge Alonso
        //Fecha:2016-09-17
        /////////////////////////////////
        public string panelType = string.Empty;
        public string panelEditRol;
    /////////////////////////////////

        /// <summary>
        /// A collection of Field objects.
        /// </summary>
        public FieldList Fields
        {
            get
            {
                return _fields;
            }
        }
        /// <summary>
        /// Gets or sets the string that is used to prefix html input elements' id and name attributes.
        /// This value must comply with the naming rules for HTML id attributes and Input elements' name attributes.
        /// </summary>
        public string FieldPrefix
        {
            get
            {
                return _fieldPrefix;
            }
            set
            {
                _fieldPrefix = value ?? "";
            }
        }
        /// <summary>
        /// Gets or sets the boolean value determining if the form should serialize itself into the string returned by the RenderHtml() method.
        /// </summary>
        public bool Serialize { get; set; }
        /// <summary>
        /// Returns an enumeration of Field objects that are of type InputField.
        /// </summary>
        public IEnumerable<InputField> InputFields
        {
            get
            {
                return _fields.OfType<InputField>();
            }
        }

        #endregion Public properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Form"/> class.
        /// </summary>
        public Form()
        {
            _fields = new FieldList(this);
            Template = BuildDefaultTemplate();
        }

        #endregion Constructor

        #region private methods

        /// <summary>
        /// Builds the default template.
        /// </summary>
        /// <returns></returns>
        private string BuildDefaultTemplate()
        {
            var formWrapper = new TagBuilder(_formWrapper);
            formWrapper.AddCssClass(_formWrapperClass);
            formWrapper.InnerHtml = PlaceHolders.Fields + PlaceHolders.SerializedForm + PlaceHolders.DataScript;
            return formWrapper.ToString();
        }

        internal void FireModelBoundEvents()
        {
            foreach (var fileUpload in Fields.Where(x => x is FileUpload).Cast<FileUpload>())
            {
                fileUpload.FireFilePosted();
            }
        }


        #endregion private methods

        #region public methods

        /// <summary>
        /// Renders a script block containing a JSON representation of each fields' client side data.
        /// </summary>
        public string RenderDataScript(string jsVarName)
        {
            if (string.IsNullOrEmpty(jsVarName))
                jsVarName = Constants.FR_jsVarName;

            if (_fields.Any(x => x.HasClientData))
            {
                var data = new Dictionary<string, Dictionary<string, DataItem>>();
                foreach (var field in _fields.Where(x => x.HasClientData))
                    data.Add(field.Key, field.DataDictionary);

                var script = new TagBuilder("script");
                script.Attributes["type"] = "text/javascript";
                script.InnerHtml = string.Format("{0}var {1} = {2};",
                    Environment.NewLine,
                    jsVarName,
                    data.ToJson());

                return script.ToString();
            }

            return null;
        }

        /// <summary>
        /// Validates each displayed InputField object contained in the Fields collection. 
        /// Validation causes the Error property to be set for each InputField object.
        /// </summary>
        /// <returns>Returns true if every InputField object is valid. False is returned otherwise.</returns>
        public bool Validate()
        {
            return Validate(true);
        }

        /// <summary>
        /// Validates each displayed InputField object contained in the Fields collection. 
        /// Validation causes the Error property to be set for each InputField object.
        /// </summary>
        /// <param name="onlyDisplayed">Whether to validate only displayed fields.</param>
        /// <returns>Returns true if every InputField object is valid. False is returned otherwise.</returns>
        public bool Validate(bool onlyDisplayed)
        {
            bool isValid = true;

            foreach (var field in InputFields.Where(x => !onlyDisplayed || x.Display))
                isValid = isValid & field.Validate();

            return isValid;
        }

        /// <summary>
        /// Returns a string containing the rendered HTML of every contained Field object.
        /// Optionally, the form's serialized state and/or JavaScript data can be included in the returned HTML string.
        /// </summary>        
        /// <param name="formatHtml">Determines whether to format the generated html with indentation and whitespace for readability.</param>
        /// <returns>Returns a string containing the rendered html of every contained Field object.</returns>
        public string RenderHtml(bool formatHtml)
        {
            var fieldsHtml = new StringBuilder();
            List<Field> listFiedlds = _fields.Where(x => x.Display).OrderBy(x => x.DisplayOrder).ToList();

            foreach (var field in listFiedlds)
            {
                fieldsHtml.Append("<br/>");
                fieldsHtml.Append(field.RenderHtml());
            }

            var html = new StringBuilder(Template);
            html.Replace(PlaceHolders.Fields, fieldsHtml.ToString());

            if (Serialize)
            {
                var hdn = new TagBuilder("input");
                hdn.Attributes["type"] = "hidden";
                hdn.Attributes["id"] = MagicStrings.SerializedForm;
                hdn.Attributes["name"] = MagicStrings.SerializedForm;
                hdn.Attributes["value"] = SerializationUtility.Serialize(this);
                html.Replace(PlaceHolders.SerializedForm, hdn.ToString(TagRenderMode.SelfClosing));
            }

            html.Replace(PlaceHolders.DataScript, RenderDataScript(null));

            PlaceHolders.RemoveAll(html);

            if (formatHtml)
                return XDocument.Parse(html.ToString()).ToString();

            return html.ToString();
        }

        /// <summary>
        /// Renders the HTML query.
        /// </summary>
        /// <param name="formatHtml">if set to <c>true</c> [format HTML].</param>
        /// <returns></returns>
        public string RenderHtmlQuery(bool formatHtml)
        {
            var fieldsHtml = new StringBuilder();
            List<Field> listFiedlds = _fields.Where(x => x.Display).OrderBy(x => x.DisplayOrder).ToList();

            foreach (var field in listFiedlds)
            {
                fieldsHtml.Append("<br/>");
                fieldsHtml.Append(field.RenderHtmlQuery());
            }

            var html = new StringBuilder(Template);
            html.Replace(PlaceHolders.Fields, fieldsHtml.ToString());

            if (Serialize)
            {
                var hdn = new TagBuilder("input");
                hdn.Attributes["type"] = "hidden";
                hdn.Attributes["id"] = MagicStrings.SerializedForm;
                hdn.Attributes["name"] = MagicStrings.SerializedForm;
                hdn.Attributes["value"] = SerializationUtility.Serialize(this);
                html.Replace(PlaceHolders.SerializedForm, hdn.ToString(TagRenderMode.SelfClosing));
            }

            html.Replace(PlaceHolders.DataScript, RenderDataScript(null));

            PlaceHolders.RemoveAll(html);

            if (formatHtml)
                return XDocument.Parse(html.ToString()).ToString();

            return html.ToString();
        }

        /// <summary>
        /// Renders the HTML.
        /// </summary>
        /// <param name="formatHtml">if set to <c>true</c> [format HTML].</param>
        /// <param name="NumColumns">The num columns.</param>
        /// <returns></returns>
        public string RenderHtml(bool formatHtml, int NumColumns)
        {
            #region Construir la distribucion

            var fieldsHtml = new StringBuilder();

            fieldsHtml.Append("<table");

            fieldsHtml.Append(" width='98%'");
            fieldsHtml.Append(" >");

            // cambiar la consulta, los campos ya vienen ordenados
            List<Field> listFiedlds = _fields.Where(x => x.Display).ToList();

            // validar si existen columnas, por defecto el 100%
            double columnSize = NumColumns > 0 ? 100 / NumColumns : 100;

            // obtener el numero total de filas de campos
            var rowCount = _fields.Select(f => f.PanelColumnSortOrder).Max();

            #region realizar el render de acuerdo a las filas

            for (int r = 1; r <= rowCount; r++)
            {
                fieldsHtml.Append("<tr>");

                #region Contruccion de celdas

                for (int j = 1; j <= NumColumns; j++)
                {
                    fieldsHtml.Append("<td width='" + columnSize.ToString() + "%' class='Td_Style' >");

                    foreach (var item in listFiedlds)
                    {
                        if (item.PanelColumnSortOrder == r && item.PanelColumn == j)
                        {
                            fieldsHtml.Append(item.RenderHtml());
                            break;
                        }
                    }

                    fieldsHtml.Append("</td>");
                }

                #endregion Contruccion de celdas

                fieldsHtml.Append("</tr>");

                fieldsHtml.Append("<tr >");
                fieldsHtml.Append("<td >");
                fieldsHtml.Append("<br />");
                fieldsHtml.Append("</td >");
                fieldsHtml.Append("</tr>");

            }

            #endregion realizar el render de acuerdo a las filas

            // --------------------------------------------------------------

            fieldsHtml.Append("</table>");
            var html = new StringBuilder(Template);
            html.Replace(PlaceHolders.Fields, fieldsHtml.ToString());

            #endregion Construir la distribucion

            if (Serialize)
            {
                var hdn = new TagBuilder("input");
                hdn.Attributes["type"] = "hidden";
                hdn.Attributes["id"] = MagicStrings.SerializedForm;
                hdn.Attributes["name"] = MagicStrings.SerializedForm;
                hdn.Attributes["value"] = SerializationUtility.Serialize(this);
                html.Replace(PlaceHolders.SerializedForm, hdn.ToString(TagRenderMode.SelfClosing));
            }

            html.Replace(PlaceHolders.DataScript, RenderDataScript(null));

            PlaceHolders.RemoveAll(html);

            if (formatHtml)
            {
                //string result = XDocument.Parse(html.ToString()).ToString();
                return html.ToString();
                }

            return html.ToString();
        }

        /// <summary>
        /// Renders the HTML query.
        /// </summary>
        /// <param name="formatHtml">if set to <c>true</c> [format HTML].</param>
        /// <param name="NumColumns">The num columns.</param>
        /// <returns></returns>
        public string RenderHtmlQuery(bool formatHtml, int NumColumns)
        {
            var fieldsHtml = new StringBuilder();

            fieldsHtml.Append("<table width='98%' >");
            fieldsHtml.Append("<tr>");
            List<Field> listFiedlds = _fields.Where(x => x.Display).OrderBy(x => x.DisplayOrder).ToList();

            // validar si existen columnas, por defecto el 100%
            double columnSize = NumColumns > 0 ? 100 / NumColumns : 100;

            for (int i = 1; i <= NumColumns; i++)
            {

                fieldsHtml.Append("<td width='" + columnSize.ToString() + "%' class='Td_Style' >");

                foreach (var field in listFiedlds)
                {
                    if (field.PanelColumn == i)

                        //fieldsHtml.Append("<br/>");
                        fieldsHtml.Append(field.RenderHtmlQuery());
                }
                fieldsHtml.Append("</td>");
                //fieldsHtml.Append(" <td> <span class='field-validation-valid' data-valmsg-for=" + listFiedlds[i].Display + "data-valmsg-replace='true'></span></td>");

            }
            fieldsHtml.Append("</tr>");
            fieldsHtml.Append("</table>");
            var html = new StringBuilder(Template);
            html.Replace(PlaceHolders.Fields, fieldsHtml.ToString());

            if (Serialize)
            {
                var hdn = new TagBuilder("input");
                hdn.Attributes["type"] = "hidden";
                hdn.Attributes["id"] = MagicStrings.SerializedForm;
                hdn.Attributes["name"] = MagicStrings.SerializedForm;
                hdn.Attributes["value"] = SerializationUtility.Serialize(this);
                html.Replace(PlaceHolders.SerializedForm, hdn.ToString(TagRenderMode.SelfClosing));
            }

            html.Replace(PlaceHolders.DataScript, RenderDataScript(null));

            PlaceHolders.RemoveAll(html);

            if (formatHtml)
                return XDocument.Parse(html.ToString()).ToString();

            return html.ToString();
        }

        /// <summary>
        /// Returns a string containing the rendered html of every contained Field object. The html can optionally include the Form object's state serialized into a hidden field.
        /// </summary>
        /// <returns>Returns a string containing the rendered html of every contained Field object.</returns>
        public string RenderHtml()
        {
            return RenderHtml(false);
        }

        /// <summary>
        /// Renders the HTML query.
        /// </summary>
        /// <returns></returns>
        public string RenderHtmlQuery()
        {
            return RenderHtml(false);
        }

        /// <summary>
        /// This method clears the Error property of each contained InputField.
        /// </summary>
        public void ClearAllErrors()
        {
            foreach (var inputField in InputFields)
                inputField.ClearError();
        }

        /// <summary>
        /// This method provides a convenient way of adding multiple Field objects at once.
        /// </summary>
        /// <param name="fields">Field object(s)</param>
        public void AddFields(params Field[] fields)
        {
            foreach (var field in fields)
            {
                _fields.Add(field);
            }
        }

        /// <summary>
        /// Provides a convenient way the end users' responses to each InputField
        /// </summary>
        /// <param name="completedOnly">Determines whether to return a Response object for only InputFields that the end user completed.</param>
        /// <returns>List of Response objects.</returns>
        public List<Response> GetResponses(bool completedOnly)
        {
            var responses = new List<Response>();
            foreach (var field in InputFields.OrderBy(x => x.DisplayOrder))
            {
                var response = new Response
                {
                    Title = field.GetResponseTitle(),
                    Value = field.Response
                };

                if (completedOnly && string.IsNullOrEmpty(response.Value))
                    continue;

                responses.Add(response);
            }

            return responses;
        }

        /// <summary>
        /// Provides a convenient way to set the template for multiple fields.
        /// </summary>
        /// <param name="template">The fields' HTML template.</param>
        public void SetFieldTemplates(string template, params Field[] fields)
        {
            foreach (var field in fields)
                field.Template = template;
        }

        #endregion public methods
    }
}
