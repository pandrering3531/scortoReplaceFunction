using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace STPC.DynamicForms.Core.Fields
{
	[Serializable]
	public class Calendar : TextField
	{
		public override string RenderHtml()
		{
			var html = new StringBuilder(Template);
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
				#region prompt label

				// prompt label
				var prompt = new TagBuilder("label");
				prompt.SetInnerText(GetPrompt());
				prompt.Attributes.Add("for", inputName);
				prompt.Attributes.Add("class", _promptClass + " " + this.Style);

				#endregion prompt label

				#region Strategy

				//html.Replace(PlaceHolders.Prompt, prompt.ToString());
				if (idStrategy != 0)
				{
					var promptResultStrategy = new TagBuilder("label");
					promptResultStrategy.SetInnerText("");
					promptResultStrategy.Attributes.Add("for", "lblResultStrategy" + inputName);
					promptResultStrategy.Attributes.Add("class", _promptClass);
					promptResultStrategy.Attributes.Add("id", "ResultStrategy" + inputName);
					html.Replace(PlaceHolders.Prompt, prompt.ToString() + promptResultStrategy.ToString());

				}
				else
					html.Replace(PlaceHolders.Prompt, prompt.ToString());

				#endregion Strategy

				#region input element

				// input element
				var txt = new TagBuilder("input");
				txt.Attributes.Add("name", inputName);
				txt.Attributes.Add("id", inputName);

				if (idStrategy != 0 && idStrategy != null)
					txt.Attributes.Add("onchange", "myFunction(this," + idStrategy + ")");

				// validar si es trigger
				if (IsTriggerField && PageStrategyUid != null)
					txt.Attributes.Add("onchange", "ExecuteStrategy(this,'" + inputName + "','" + PageStrategyUid + "')");

				if (IsTriguerEventChange == true)
					txt.Attributes.Add("onchange", "HideControl(this,'" + IdControlToHide + "','" + ValueWhenHideControl + "')");

				//txt.Attributes.Add("type", "text");
				txt.Attributes.Add("value", Value);
				txt.Attributes.Add("class", "jQueryCalendarInput " + this.Style + "_input ");


				if (!flagEdit || IsReadOnly)
				{
					txt.Attributes.Add("disabled", "disabled");
					txt.Attributes.Add("readonly", "readonly");
				}

				if (flagView) txt.Attributes.Add("type", "text");
				else txt.Attributes.Add("type", "hidden");

				if (eventName == "Hide")
          txt.Attributes.Add("style", "display: none !important; width: 70%");
				if (eventName == "Disabled")
					if (!txt.Attributes.ContainsKey("disabled"))
						txt.Attributes.Add("disabled", "disabled");

				#region valida requerido

				if (Required)
				{
					if (txt.Attributes.ContainsKey("class"))
					{
						txt.Attributes["class"] = txt.Attributes["class"] + " required";
					}
					else
					{
						txt.Attributes.Add("class", "required");
					}
				}
        if (!Required && IsRequeried)
        {
          if (txt.Attributes.ContainsKey("class"))
          {
            txt.Attributes["class"] = txt.Attributes["class"] + " Not_required required";
          }
          else
          {
            txt.Attributes.Add("class", "Not_required required");
          }
        }

				#endregion

				if (txt.Attributes.ContainsKey("class"))
				{
					txt.Attributes["class"] = txt.Attributes["class"] + "  tooltip";
				}

				else
					txt.Attributes.Add("class", this.Style + "_input " + "tooltip");

				if (ToolTip != string.Empty)
					txt.Attributes.Add("title", ToolTip);

				txt.MergeAttributes(_inputHtmlAttributes);
				html.Replace(PlaceHolders.Input, txt.ToString(TagRenderMode.SelfClosing));



				#endregion input element
			}

			#region error label

			// error label
			if (!ErrorIsClear)
			{
				var error = new TagBuilder("label");
				error.Attributes.Add("for", inputName);
				error.Attributes.Add("class", _errorClass);
				error.SetInnerText(Error);
				html.Replace(PlaceHolders.Error, error.ToString());
			}

			#endregion error label

			// wrapper id
			html.Replace(PlaceHolders.FieldWrapperId, GetWrapperId());

			return html.ToString();
		}

		public override string RenderHtmlQuery()
		{
			var html = new StringBuilder(Template);
			var inputName = GetHtmlId();

			#region prompt label

			// prompt label
			var prompt = new TagBuilder("label");
			prompt.SetInnerText(GetPrompt());
			prompt.Attributes.Add("for", inputName);
			prompt.Attributes.Add("class", _promptClass);

			#endregion prompt label

			#region Strategy

			//html.Replace(PlaceHolders.Prompt, prompt.ToString());
			if (idStrategy != 0)
			{
				var promptResultStrategy = new TagBuilder("label");
				promptResultStrategy.SetInnerText("");
				promptResultStrategy.Attributes.Add("for", "lblResultStrategy" + inputName);
				promptResultStrategy.Attributes.Add("class", _promptClass);
				promptResultStrategy.Attributes.Add("id", "ResultStrategy" + inputName);
				html.Replace(PlaceHolders.Prompt, prompt.ToString() + promptResultStrategy.ToString());

			}
			else
				html.Replace(PlaceHolders.Prompt, prompt.ToString());

			#endregion Strategy

			#region input element

			// input element
			var txt = new TagBuilder("input");
			txt.Attributes.Add("name", inputName);
			txt.Attributes.Add("id", inputName + Value);

			// valida que tenga una estrategia asociada
			if (idStrategy != 0 && idStrategy != null)
				txt.Attributes.Add("onchange", "myFunction(this," + idStrategy + ")");

			// validar si es trigger
			if (IsTriggerField && PageStrategyUid != null)
				txt.Attributes.Add("onchange", "ExecuteStrategy(this,'" + inputName + "','" + PageStrategyUid + "')");


			//txt.Attributes.Add("type", "text");
			txt.Attributes.Add("value", Value);
			//txt.Attributes.Add("tabindex", Index.ToString());
			txt.Attributes.Add("class", "jQueryCalendarInput");

			txt.Attributes.Add("type", "text");

			txt.MergeAttributes(_inputHtmlAttributes);
			html.Replace(PlaceHolders.Input, txt.ToString(TagRenderMode.SelfClosing));

			#endregion input element

			#region error label

			// error label
			if (!ErrorIsClear)
			{
				var error = new TagBuilder("label");
				error.Attributes.Add("for", inputName);
				error.Attributes.Add("class", _errorClass);
				error.SetInnerText(Error);
				html.Replace(PlaceHolders.Error, error.ToString());
			}

			#endregion error label

			// wrapper id
			html.Replace(PlaceHolders.FieldWrapperId, GetWrapperId());

			return html.ToString();
		}
	}
}
