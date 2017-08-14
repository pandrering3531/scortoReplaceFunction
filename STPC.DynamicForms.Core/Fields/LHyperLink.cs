using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace STPC.DynamicForms.Core.Fields
{
	[Serializable]
	public class LHyperLink : Field
	{
		/// <summary>
		/// The html to be rendered on the form.
		/// </summary>
		public string Text { get; set; }
		public string Target { get; set; }

		public override string RenderHtml()
		{
			//var html = new StringBuilder(Template);
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
				#region hyperlink

				html.Append("<div class='MvcFieldWrapper' id='STPC_DFi_" + this.Key + "_wrapper'>");
				html.AppendLine();
				html.Append("<a ");


				html.Append(" target='_blank'");

				// validar si es trigger
				if (IsTriggerField && PageStrategyUid != null)
					html.Append(" onchange='ExecuteStrategy(this,'" + inputName + "','" + PageStrategyUid + "')'");

				if (!string.IsNullOrEmpty(Target))
					html.Append(" href='http://" + Target + "'");

				if (IsReadOnly)
					html.Append(" disabled='disabled'");

				html.Append(" >");
				html.Append(Target);
				html.Append("</a>");
				html.AppendLine();
				html.Append("</div>");
				html.AppendLine("<br />");

				#endregion hyperlink
			}

			// wrapper id
			html.Replace(PlaceHolders.FieldWrapperId, GetWrapperId());

			return html.ToString();
		}

		public override string RenderHtmlQuery()
		{
			//var html = new StringBuilder(Template);
			var html = new StringBuilder();
			var inputName = GetHtmlId();

			#region hyperlink

			html.Append("<div class='MvcFieldWrapper' id='STPC_DFi_" + this.Key + "_wrapper'>");
			html.AppendLine();
			html.Append("<a ");
			html.Append(" target='_blank'");
			html.Append(" >");
			html.Append(Text);
			html.Append("</a>");
			html.AppendLine();
			html.Append("</div>");
			html.AppendLine("<br />");

			#endregion hyperlink

			// wrapper id
			html.Replace(PlaceHolders.FieldWrapperId, GetWrapperId());

			return html.ToString();
		}

		protected override string BuildDefaultTemplate()
		{
			return PlaceHolders.LHyperLink;

		}
	}
}
