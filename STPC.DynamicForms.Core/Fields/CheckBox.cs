using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace STPC.DynamicForms.Core.Fields
{
	/// <summary>
	/// Represents a single html checkbox input field.
	/// </summary>
	[Serializable]
	public class CheckBox : InputField
	{
		private string _checkedValue = Constants.CB_checkedValue;
		private string _uncheckedValue = Constants.CB_uncheckedValue;
		/// <summary>
		/// The text to be used as the user's response when they check the checkbox.
		/// </summary>
		public string CheckedValue
		{
			get
			{
				return _checkedValue;
			}
			set
			{
				_checkedValue = value;
			}
		}
		/// <summary>
		/// The text to be used as the user's response when they do not check the checkbox.
		/// </summary>
		public string UncheckedValue
		{
			get
			{
				return _uncheckedValue;
			}
			set
			{
				_uncheckedValue = value;
			}
		}
		/// <summary>
		/// The state of the checkbox.
		/// </summary>
		public bool Checked { get; set; }

		public override string Response
		{
			get
			{
				return Checked ? _checkedValue : _uncheckedValue;
			}
		}

		public CheckBox()
		{
			// give the checkbox a different default prompt class
			_promptClass = Constants.CB_promptClass;
		}

		public override bool Validate()
		{
			ClearError();

			if (Required && !Checked)
			{
				// Isn't valid
				Error = _requiredMessage;
			}

			FireValidated();
			return ErrorIsClear;
		}

		public override string RenderHtml()
		{
			var inputName = GetHtmlId();
			var html = new StringBuilder(Template);

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
				prompt.Attributes.Add("class", _promptClass + " tooltip" + " " + this.Style + "_label");

				prompt.Attributes.Add("title", ToolTip);

        if (eventName == "Hide") prompt.Attributes.Add("style", "display: none !important; width: 70%");
				if (eventName == "Disabled")
					if (!prompt.Attributes.ContainsKey("disabled"))
						prompt.Attributes.Add("disabled", "disabled");
					else
						prompt.Attributes.Add("title", ToolTip);

				html.Replace(PlaceHolders.Prompt, prompt.ToString());

				#endregion prompt label

				#region checkbox input

				// checkbox input
				var chk = new TagBuilder("input");
				chk.Attributes.Add("id", inputName);
				chk.Attributes.Add("name", inputName);
				//chk.Attributes.Add("style", "width: 70%");

				if (Checked) chk.Attributes.Add("checked", "checked");

				// validar si es trigger
				if (IsTriggerField && PageStrategyUid != null)
					chk.Attributes.Add("onchange", "ExecuteStrategy(this,'" + inputName + "','" + PageStrategyUid + "')");

				if (!flagEdit || IsReadOnly)
				{
					chk.Attributes.Add("disabled", "disabled");
					chk.Attributes.Add("readonly", "readonly");
				}
				else
				{
					if (ToolTip != string.Empty)
						chk.Attributes.Add("title", ToolTip);
				}

        if (eventName == "Hide") chk.Attributes.Add("style", "display: none !important; width: 70%");
				if (eventName == "Disabled")
					if (!chk.Attributes.ContainsKey("disabled"))
						chk.Attributes.Add("disabled", "disabled");
					else
						chk.Attributes.Add("title", ToolTip);

				if (flagView) chk.Attributes.Add("type", "checkbox");
				else chk.Attributes.Add("type", "hidden");

				chk.Attributes.Add("value", bool.TrueString);
				chk.MergeAttributes(_inputHtmlAttributes);

				chk.Attributes.Add("class", "tooltip" + " " + this.Style + "_input");
				// si true configura evento para inactivar un control
				if (IsTriguerEventChange == true)
					chk.Attributes.Add("onchange", "HideCheckControl(this,'" + IdControlToHide + "','" + ValueWhenHideControl + "')");

				if (eventName != "Hide" || eventName == "Disabled")
					if (Required)
					{
						if (chk.Attributes.ContainsKey("class"))
						{
							chk.Attributes["class"] = chk.Attributes["class"] + " required";
						}
						else
						{
							chk.Attributes.Add("class", "required");
						}
            if (!Required && IsRequeried)
            {
              if (chk.Attributes.ContainsKey("class"))
              {
                chk.Attributes["class"] = chk.Attributes["class"] + " Not_required required ";
              }
              else
              {
                chk.Attributes.Add("class", "Not_required required");
              }
            }
					}

				#endregion checkbox input

				#region hidden input

				// hidden input (so that value is posted when checkbox is unchecked)
				var hdn = new TagBuilder("input");
				hdn.Attributes.Add("type", "hidden");
				hdn.Attributes.Add("id", inputName + "_hidden");
				hdn.Attributes.Add("name", inputName);
				hdn.Attributes.Add("value", bool.FalseString);

				html.Replace(PlaceHolders.Input, chk.ToString(TagRenderMode.SelfClosing) + hdn.ToString(TagRenderMode.SelfClosing));

				#endregion hidden input
			}

			#region error label

			// error label
			if (!ErrorIsClear)
			{
				var error = new TagBuilder("label");
				error.SetInnerText(Error);
				error.Attributes.Add("for", inputName);
				error.AddCssClass(_errorClass);
				html.Replace(PlaceHolders.Error, error.ToString());
			}

			#endregion error label

			// wrapper id
			html.Replace(PlaceHolders.FieldWrapperId, GetWrapperId());

			return html.ToString();
		}

		public override string RenderHtmlQuery()
		{
			var inputName = GetHtmlId();
			var html = new StringBuilder(Template);

			#region prompt label

			// prompt label
			var prompt = new TagBuilder("label");
			prompt.SetInnerText(GetPrompt());
			prompt.Attributes.Add("for", inputName);
			prompt.Attributes.Add("class", _promptClass);
			html.Replace(PlaceHolders.Prompt, prompt.ToString());

			#endregion prompt label

			#region checkbox input

			// checkbox input
			var chk = new TagBuilder("input");
			chk.Attributes.Add("id", inputName);
			chk.Attributes.Add("name", inputName);
			if (Checked) chk.Attributes.Add("checked", "checked");

			chk.Attributes.Add("type", "checkbox");

			chk.Attributes.Add("value", bool.TrueString);
			chk.MergeAttributes(_inputHtmlAttributes);

			#endregion checkbox input

			#region hidden input

			// hidden input (so that value is posted when checkbox is unchecked)
			var hdn = new TagBuilder("input");
			hdn.Attributes.Add("type", "hidden");
			hdn.Attributes.Add("id", inputName + "_hidden");
			hdn.Attributes.Add("name", inputName);
			hdn.Attributes.Add("value", bool.FalseString);
			//hdn.Attributes.Add("tabindex", Index.ToString());

			html.Replace(PlaceHolders.Input, chk.ToString(TagRenderMode.SelfClosing) + hdn.ToString(TagRenderMode.SelfClosing));

			#endregion hidden input

			#region error label

			// error label
			if (!ErrorIsClear)
			{
				var error = new TagBuilder("label");
				error.SetInnerText(Error);
				error.Attributes.Add("for", inputName);
				error.AddCssClass(_errorClass);
				html.Replace(PlaceHolders.Error, error.ToString());
			}

			#endregion error label

			// wrapper id
			html.Replace(PlaceHolders.FieldWrapperId, GetWrapperId());

			return html.ToString();
		}

		protected override string BuildDefaultTemplate()
		{
			var wrapper = new TagBuilder("div");
			wrapper.AddCssClass(Constants.CB_CSSClass);
			wrapper.Attributes["id"] = PlaceHolders.FieldWrapperId;
			wrapper.InnerHtml = PlaceHolders.Error + PlaceHolders.Input + PlaceHolders.Prompt;
			return wrapper.ToString();
		}

		public bool IsTriguerEventChange { get; set; }

		public string ValueWhenHideControl { get; set; }

		public string IdControlToHide { get; set; }
	}
}
