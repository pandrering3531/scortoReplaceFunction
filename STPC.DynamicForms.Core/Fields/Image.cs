using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.Core.Fields
{
	[Serializable]
	public class Image : Field
	{
		public string imageUrl;
		
		public override string RenderHtml()
		{
			var html = new StringBuilder();

			html.Append("<div class='MvcFieldWrapper' id='STPC_DFi_" + this.Key + "_wrapper'>");
			html.AppendLine();


			html.AppendLine("<br />");

			#region espacio en blanco

			html.Append("<img");
			html.Append(" id='STPC_DFi_" + this.Key + "'");

			if (!imageUrl.Equals(string.Empty))
				html.Append(" src='" + this.imageUrl + "'");

			if (this.Style != null)
			{
				if (!this.Style.Equals(string.Empty))
					html.Append(" class='" + this.Style + "'");
			}

			html.Append(" >");
			html.Append("</img>");


			#endregion espacio en blanco

			html.AppendLine();
			html.Append("</div>");
			html.AppendLine("<br />");

			// wrapper id
			html.Replace(PlaceHolders.FieldWrapperId, GetWrapperId());

			return html.ToString();
		}

		public override string RenderHtmlQuery()
		{
			var html = new StringBuilder(Template);

			#region espacio en blanco

			html.Append("<div class='MvcFieldWrapper' id='STPC_DFi_" + this.Key + "_wrapper'>");
			html.AppendLine();
			html.Append("<label");
			html.Append(" id='STPC_DFi_" + this.Key + "'");
			html.Append(" style='width: 70%, whitespace:wrap, word-wrap: break-word'");

			html.Append(" >");
			html.Append("</label>");
			html.AppendLine();
			html.Append("</div>");
			html.AppendLine("<br />");

			#endregion espacio en blanco

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
