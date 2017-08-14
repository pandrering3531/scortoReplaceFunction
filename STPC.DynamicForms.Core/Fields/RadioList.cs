using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using STPC.DynamicForms.Core.Fields;

namespace STPC.DynamicForms.Core.Fields
{
	/// <summary>
	/// Represents a list of html radio button inputs.
	/// </summary>
	[Serializable]
	public class RadioList : OrientableField
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
				prompt.AddCssClass(_promptClass + this.Style + "_title");
				prompt.SetInnerText(GetPrompt());
				prompt.Attributes.Add("for", inputName);

        if (eventName == "Hide") prompt.Attributes.Add("style", "display: none !important;");
				if (eventName == "Disabled")
					if (!prompt.Attributes.ContainsKey("disabled"))
						prompt.Attributes.Add("disabled", "disabled");

				html.Replace(PlaceHolders.Prompt, prompt.ToString());

				#endregion prompt label

				#region list of radio buttons

				// list of radio buttons
				var input = new StringBuilder();
				var ul = new TagBuilder("ul");
				ul.Attributes.Add("class", _orientation == Orientation.Vertical ? _verticalClass + " " + listEvents : _horizontalClass + " " + listEvents);
				ul.AddCssClass(_listClass + " " + this.Style);
				ul.Attributes.Add("id", inputName);
				IdControlToHide = string.Empty;
				// validar si es trigger
				if (IsTriggerField && PageStrategyUid != null)
					ul.Attributes.Add("onchange", "ExecuteStrategy(this,'" + inputName + "','" + PageStrategyUid + "')");

				if (ListIdControlToHide != null)
					for (int i = 0; i < ListIdControlToHide.Length; i++)
					{
						IdControlToHide += ListIdControlToHide[i] + ",";

					}

				if (IsTriguerEventChange == true)
					ul.Attributes.Add("onclick", "HideCheckListControl(this,'" + IdControlToHide + "','" + ValueWhenHideControl + "')");

				#region valida requerido


				if (Required)
				{
					if (ul.Attributes.ContainsKey("class"))
					{
						if (eventName != "Disabled")
							if (!ul.Attributes.ContainsKey("disabled"))
								ul.Attributes["class"] = ul.Attributes["class"] + " requiredRadioButon";
					}
					else
					{
						ul.Attributes.Add("class", "requiredRadioButon");
					}
				}

        if (!Required && IsRequeried)
        {
          if (ul.Attributes.ContainsKey("class"))
          {
            ul.Attributes["class"] = ul.Attributes["class"] + " Not_requiredRadioButon";
          }
          else
          {
            ul.Attributes.Add("class", "Not_requiredcheckBox");
          }
        }

				ul.Attributes["class"] = ul.Attributes["class"] + "  tooltip";

				if (flagEdit || !IsReadOnly)
					if (ToolTip != string.Empty)
						ul.Attributes.Add("title", ToolTip);

        if (eventName == "Hide") ul.Attributes.Add("style", "display: none !important;");
				if (eventName == "Disabled")
					if (!ul.Attributes.ContainsKey("disabled"))
						ul.Attributes.Add("disabled", "disabled");
				#endregion

				input.Append(ul.ToString(TagRenderMode.StartTag));

				var choicesList = _choices.ToList();
				for (int i = 0; i < choicesList.Count; i++)
				{
					ListItem choice = choicesList[i];
					string radId = inputName;

					#region open list item

					// open list item
					var li = new TagBuilder("li");

					if (!flagEdit || IsReadOnly)
					{
						li.Attributes.Add("disabled", "disabled");
						li.Attributes.Add("readonly", "readonly");
					}

					//if (!flagView) li.Attributes.Add("type", "hidden");

					if (_orientation == Orientation.Horizontal)
						li.Attributes.Add("style", "display: inline;list-style-type: none;");



					input.Append(li.ToString(TagRenderMode.StartTag));
					li.Attributes.Add("id", inputName);
					#endregion open list item

					#region radio button input

					// radio button input
					var rad = new TagBuilder("input");
					rad.Attributes.Add("type", "radio");
					rad.Attributes.Add("name", inputName);
					rad.Attributes.Add("id", radId);
					rad.Attributes.Add("class", this.Style + "_input");
					//rad.Attributes.Add("class", listEvents);

					string[] arrayGuid = choice.Value.Split('|');

					//chk.Attributes.Add("value", choice.Value);
					rad.Attributes.Add("value", arrayGuid[0]);

					if (arrayGuid.Length > 1)
						rad.Attributes.Add("Text", arrayGuid[1]);
					else
						rad.Attributes.Add("Text", arrayGuid[0]);

					if (!flagEdit || IsReadOnly)
					{
						rad.Attributes.Add("disabled", "disabled");
						rad.Attributes.Add("readonly", "readonly");
					}

					if (!rad.Attributes.ContainsKey("disabled"))
					{
						if (eventName == "Disabled") rad.Attributes.Add("disabled", "disabled");
					}
					if (eventName == "Hide") rad.Attributes.Add("style", "visibility: hidden;");

					//rad.Attributes.Add("value", choice.Value);
					//rad.Attributes.Add("tabindex", Index.ToString());

					if (choice.Selected)
						rad.Attributes.Add("checked", "checked");




					rad.MergeAttributes(_inputHtmlAttributes);
					rad.MergeAttributes(choice.HtmlAttributes);
					input.Append(rad.ToString(TagRenderMode.SelfClosing));

					#endregion radio button input

					#region checkbox label

					// checkbox label
					var lbl = new TagBuilder("label");
					lbl.Attributes.Add("for", radId);
					lbl.Attributes.Add("class", _inputLabelClass + this.Style + "_label");

					if (arrayGuid.Length > 1)
					{
						lbl.Attributes.Add("text", arrayGuid[1]);
						lbl.SetInnerText(arrayGuid[1]);
					}
					else
					{
						lbl.Attributes.Add("text", arrayGuid[0]);
						lbl.SetInnerText(arrayGuid[0]);
					}
					input.Append(lbl.ToString());

					#endregion checkbox label

					#region close list item

					// close list item
					input.Append(li.ToString(TagRenderMode.EndTag));

					#endregion close list item
				}

				if (!flagEdit || IsReadOnly)
				{

					if (!ul.Attributes.ContainsKey("disabled"))
					{
						ul.Attributes.Add("disabled", "disabled");
						ul.Attributes.Add("readonly", "readonly");
					}
				}




				//if (!flagView) ul.Attributes.Add("type", "hidden");

				#endregion list of radio buttons

				#region open list item

				var li2 = new TagBuilder("li");
				input.Append(li2.ToString(TagRenderMode.StartTag));

				#endregion open list item

				#region radio button input

				// radio button input
				var rad2 = new TagBuilder("input");
				rad2.Attributes.Add("type", "hidden");
				rad2.Attributes.Add("name", inputName);
				rad2.Attributes.Add("id", inputName);
				rad2.Attributes.Add("value", "");
				//rad2.Attributes.Add("tabindex", Index.ToString());

				rad2.Attributes.Add("checked", "checked");
				rad2.MergeAttributes(_inputHtmlAttributes);
				input.Append(rad2.ToString(TagRenderMode.SelfClosing));
				input.Append(li2.ToString(TagRenderMode.EndTag));
				input.Append(ul.ToString(TagRenderMode.EndTag));


				html.Replace(PlaceHolders.Input, input.ToString());

				#endregion radio button input
			}

			#region error label

			// error label
			if (!ErrorIsClear)
			{
				var error = new TagBuilder("label");
				error.AddCssClass(_errorClass);
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
			prompt.AddCssClass(_promptClass);
			prompt.SetInnerText(GetPrompt());
			html.Replace(PlaceHolders.Prompt, prompt.ToString());

			#endregion prompt label

			#region list of radio buttons

			// list of radio buttons
			var input = new StringBuilder();
			var ul = new TagBuilder("ul");
			ul.Attributes.Add("class", _orientation == Orientation.Vertical ? _verticalClass : _horizontalClass);
			ul.AddCssClass(_listClass);
			input.Append(ul.ToString(TagRenderMode.StartTag));

			var choicesList = _choices.ToList();
			for (int i = 0; i < choicesList.Count; i++)
			{
				ListItem choice = choicesList[i];
				string radId = inputName + i;

				#region open list item

				// open list item
				var li = new TagBuilder("li");

				if (_orientation == Orientation.Horizontal)
					li.Attributes.Add("style", "display: inline;list-style-type: none;");

				input.Append(li.ToString(TagRenderMode.StartTag));

				#endregion open list item

				#region radio button input

				// radio button input
				var rad = new TagBuilder("input");
				rad.Attributes.Add("type", "radio");
				rad.Attributes.Add("name", inputName);
				rad.Attributes.Add("id", radId);
				rad.Attributes.Add("value", choice.Value);
				//rad.Attributes.Add("tabindex", Index.ToString());

				if (choice.Selected)
					rad.Attributes.Add("checked", "checked");
				rad.MergeAttributes(_inputHtmlAttributes);
				rad.MergeAttributes(choice.HtmlAttributes);
				input.Append(rad.ToString(TagRenderMode.SelfClosing));

				#endregion radio button input

				#region checkbox label

				// checkbox label
				var lbl = new TagBuilder("label");
				lbl.Attributes.Add("for", radId);
				lbl.Attributes.Add("class", _inputLabelClass);
				lbl.SetInnerText(choice.Text);
				input.Append(lbl.ToString());

				#endregion checkbox label

				#region close list item

				// close list item
				input.Append(li.ToString(TagRenderMode.EndTag));

				#endregion close list item
			}

			#endregion list of radio buttons

			#region open list item

			var li2 = new TagBuilder("li");
			input.Append(li2.ToString(TagRenderMode.StartTag));

			#endregion open list item

			#region radio button input

			// radio button input
			var rad2 = new TagBuilder("input");
			rad2.Attributes.Add("type", "hidden");
			rad2.Attributes.Add("name", inputName);
			rad2.Attributes.Add("id", inputName);
			rad2.Attributes.Add("value", "");
			//rad2.Attributes.Add("tabindex", Index.ToString());

			rad2.Attributes.Add("checked", "checked");
			rad2.MergeAttributes(_inputHtmlAttributes);
			input.Append(rad2.ToString(TagRenderMode.SelfClosing));
			input.Append(li2.ToString(TagRenderMode.EndTag));
			input.Append(ul.ToString(TagRenderMode.EndTag));

			html.Replace(PlaceHolders.Input, input.ToString());

			#endregion radio button input

			#region error label

			// error label
			if (!ErrorIsClear)
			{
				var error = new TagBuilder("label");
				error.AddCssClass(_errorClass);
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