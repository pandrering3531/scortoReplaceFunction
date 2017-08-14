using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace STPC.DynamicForms.Core.Fields
{
  /// <summary>
  /// Represents html to be rendered on the form.
  [Serializable]
  public class Literal : Field
  {
    /// <summary>
    /// The html to be rendered on the form.
    /// </summary>
    public string Text { get; set; }

    public override string RenderHtml()
    {
      var html = new StringBuilder();
      var inputName = GetHtmlId();

      bool flagView = false;
      bool flagEdit = false;

      #region Validar roles

      if (this.UserRoles != null && this.UserRoles.Count > 0)
      {
        // recorrer roles asociados a usuario
        foreach (string itemRole in this.UserRoles)
        {
          // view roles
          if (!string.IsNullOrEmpty(this.ViewRoles) && this.ViewRoles.Contains(itemRole))
            flagView = true;

          // edit roles
          if (!string.IsNullOrEmpty(this.EditRoles) && this.EditRoles.Contains(itemRole))
            flagEdit = true;
        }
      }

      #endregion Validar roles

      if (flagView)
      {
        #region texto literal

        html.Append("<div class='MvcFieldWrapper' id='STPC_DFi_" + this.Key + "_wrapper'>");
        html.AppendLine();
        html.Append("<label");
        html.Append(" id='STPC_DFi_" + this.Key + "'");
        html.Append(" class= '" + this.Style + "'");
        // validar si es trigger
        if (IsTriggerField && PageStrategyUid != null)
			  html.Append(" onchange='ExecuteStrategy(this,'" + inputName + "','" + PageStrategyUid + "')'");

       

        if (!flagEdit || IsReadOnly) html.Append(" disabled='disabled'");
        if (!flagEdit || IsReadOnly) html.Append(" readonly='readonly'");
        html.Append(" >");
        html.Append(Text);
        html.Append("</label>");
        html.AppendLine();
        html.Append("</div>");
        html.AppendLine("<br />");

        #endregion texto literal
      }

      // wrapper id
      html.Replace(PlaceHolders.FieldWrapperId, GetWrapperId());

      return html.ToString();
    }

    public override string RenderHtmlQuery()
    {
      var html = new StringBuilder(Template);
      var inputName = GetHtmlId();

      #region texto literal

      html.Append("<div class='MvcFieldWrapper' id='STPC_DFi_" + this.Key + "_wrapper'>");
      html.AppendLine();
      html.Append("<label");
      html.Append(" >");
      html.Append(Text);
      html.Append("</label>");
      html.AppendLine();
      html.Append("</div>");
      html.AppendLine("<br />");

      #endregion texto literal

      // wrapper id
      html.Replace(PlaceHolders.FieldWrapperId, GetWrapperId());

      return html.ToString();
    }

    protected override string BuildDefaultTemplate()
    {
      return PlaceHolders.Literal;

    }
  }
}
