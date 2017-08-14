using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace STPC.DynamicForms.Core.Fields
{
	/// <summary>
	/// Represents an html textarea element.
	/// </summary>
	[Serializable]
	public class TextArea : TextField
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
				prompt.Attributes.Add("class", this.Style);

				if (eventName == "Hide")
					prompt.Attributes.Add("style", "display: none !important; width: 70%");
				if (eventName == "Disabled")
					if (!prompt.Attributes.ContainsKey("disabled"))
						prompt.Attributes.Add("disabled", "disabled");


				html.Replace(PlaceHolders.Prompt, prompt.ToString());				
				#endregion prompt label

				#region input element

				// input element
				var txt = new TagBuilder("textarea");
				txt.Attributes.Add("name", inputName);
				txt.Attributes.Add("id", inputName);
				txt.SetInnerText(Value);
				//txt.Attributes.Add("tabindex", Index.ToString());

				// validar si es trigger
				if (IsTriggerField && PageStrategyUid != null)
					txt.Attributes.Add("onchange", "ExecuteStrategy(this,'" + inputName + "','" + PageStrategyUid + "')");

				#region validar max size

				if (!string.IsNullOrEmpty(MaxSize))
				{
					txt.Attributes.Add("onchange", "textLimit(this, " + MaxSize + ");");
				}

				else
				{

					txt.Attributes.Add("onchange", "textLimit(this, " + Constants.TF_MAX_TEXTAREA + ");");
				}

				#endregion validar max size

				if (!flagEdit || IsReadOnly)
				{
					txt.Attributes.Add("disabled", "disabled");
					txt.Attributes.Add("readonly", "readonly");
				}
				else
					if (ToolTip != string.Empty)
						txt.Attributes.Add("title", ToolTip);

				if (!flagView) txt.Attributes.Add("type", "hidden");

				#region valida requerido

				if (Required)
				{
					if (txt.Attributes.ContainsKey("class"))
					{
						txt.Attributes["class"] = txt.Attributes["class"] + " required " + this.Style + "_input";
					}
					else
					{
						txt.Attributes.Add("class", "required " + this.Style + "_input");
					}
				}
				else
					txt.Attributes.Add("class", this.Style + "_input");

        if (!Required && IsRequeried)
        {
          if (txt.Attributes.ContainsKey("class"))
          {
            txt.Attributes["class"] = txt.Attributes["class"] + " Not_required";
          }
          else
          {
            txt.Attributes.Add("class", "Not_required");
          }
        }

				if (eventName == "Hide")
          txt.Attributes.Add("style", "display: none !important; width: 70%");
				if (eventName == "Disabled")
					if (!txt.Attributes.ContainsKey("disabled"))
						txt.Attributes.Add("disabled", "disabled");

				txt.Attributes["class"] = txt.Attributes["class"] + "  tooltip";
				txt.Attributes.Add("maxlength", MaxSize);

				#endregion

				txt.MergeAttributes(_inputHtmlAttributes);
				html.Replace(PlaceHolders.Input, txt.ToString());

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
			html.Replace(PlaceHolders.Prompt, prompt.ToString());

			#endregion prompt label

			#region input element

			// input element
			var txt = new TagBuilder("textarea");
			txt.Attributes.Add("name", inputName);
			txt.Attributes.Add("id", inputName);
			txt.SetInnerText(Value);
			//txt.Attributes.Add("tabindex", Index.ToString());
			if (!string.IsNullOrEmpty(MaxSize)) txt.Attributes.Add("onKeyPress", "return ( this.value.length < " + MaxSize + " );");
			else txt.Attributes.Add("onKeyPress", "return ( this.value.length < " + Constants.TF_MAX_TEXTAREA + " );");

			txt.MergeAttributes(_inputHtmlAttributes);
			html.Replace(PlaceHolders.Input, txt.ToString());

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