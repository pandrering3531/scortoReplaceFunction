using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace STPC.DynamicForms.Core.Fields
{
	[Serializable]
	public class TextBox : TextField
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

				#region validar respuestas de estrategia

				#region habilitar el resultado

				// valida si este campo tiene una estrategia asociada para habilitar el resultado
				if (idStrategy != 0 && idStrategy != null)
				{
					var promptResultStrategy = new TagBuilder("label");
					promptResultStrategy.SetInnerText("");
					promptResultStrategy.Attributes.Add("for", "lblResultStrategy" + inputName);
					promptResultStrategy.Attributes.Add("class", _promptClass);
					promptResultStrategy.Attributes.Add("id", "ResultStrategy" + inputName);
					html.Replace(PlaceHolders.Prompt, prompt.ToString() + promptResultStrategy.ToString());
				}

				#endregion habilitar el resultado



				else
					html.Replace(PlaceHolders.Prompt, prompt.ToString());

				#endregion validar respuestas de estrategia

				#endregion prompt label

				#region input element

				// input element data-val="true" data-val-required="*"
				var txt = new TagBuilder("input");
				txt.Attributes.Add("name", inputName);
				txt.Attributes.Add("id", inputName);
				//txt.Attributes.Add("style", "width: 70%");

				txt.Attributes.Add("data-val", "true");
				txt.Attributes.Add("data-val-required", "* Required field");

				// valida que tenga una estrategia asociada
				if (idStrategy != 0 && idStrategy != null)
					txt.Attributes.Add("onchange", "myFunction(this," + idStrategy + ")");

				// validar si es trigger
				if (IsTriggerField && PageStrategyUid != null)
					txt.Attributes.Add("OnBlur", "ExecuteStrategy(this,'" + inputName + "','" + PageStrategyUid + "');");


				//txt.Attributes.Add("tabindex", Index.ToString());

				if (IsTriguerEventChange == true)
				{
					txt.Attributes.Add("onchange", "HideControl(this,'" + IdControlToHide + "','" + ValueWhenHideControl + "')");

				}


				#region tipo numerico
				decimal number;
				// valida que sea de tipo numerico para añadir el atributo class
				if (this.IsNumber)
				{
					txt.Attributes.Add("value", Value);

					if (Value.Contains(","))
					{
						if (!string.IsNullOrEmpty(MinSize))
						{
							txt.Attributes.Add("class", "CustomDecimal ValidateMinValue " + this.Style.Split(' ')[0] + "_input");
							txt.Attributes.Add("min", MinSize);
						}
						else
							txt.Attributes.Add("class", "CustomDecimal " + this.Style.Split(' ')[0] + "_input");
					}

					else
					{

						if (!string.IsNullOrEmpty(MinSize))
						{
							if (this.Style.Contains("CustomDecimal"))
								txt.Attributes.Add("class", "CustomDecimal ValidateMinValue " + this.Style.Split(' ')[0] + "_input");
							else
								txt.Attributes.Add("class", "CustomInteger ValidateMinValue " + this.Style.Split(' ')[0] + "_input");

							txt.Attributes.Add("min", MinSize);
						}
						else
						{
							if (this.Style.Contains("CustomDecimal"))
								txt.Attributes.Add("class", "CustomDecimal " + this.Style.Split(' ')[0] + "_input");
							else
								txt.Attributes.Add("class", "CustomInteger " + this.Style.Split(' ')[0] + "_input");
						}
					}

					if (!string.IsNullOrEmpty(MaxSize)) txt.Attributes.Add("max", MaxSize.ToString());
					else txt.Attributes.Add("max", Constants.TF_MAX_NUMERO.Length.ToString());
				}

				#endregion tipo numerico

				#region tipo currency : MONEY

				// valida que sea de tipo currency : MONEY
				else if (this.IsCurrency)
				{
					txt.Attributes.Add("value", addCommas(Value.Split(',')[0].Split('.')[0]));
					//txt.Attributes.Add("class", "CustomCurrency " + this.Style + "_input");

					if (!string.IsNullOrEmpty(MinSize))
					{
						txt.Attributes.Add("class", " CustomCurrency ValidateMinValue " + this.Style + "_input");
						txt.Attributes.Add("min", MinSize);
					}
					else
						txt.Attributes.Add("class", " CustomCurrency " + this.Style + "_input");

					if (!string.IsNullOrEmpty(MaxSize)) txt.Attributes.Add("max", MaxSize);
					else txt.Attributes.Add("max", Constants.TF_MAX_MONEDA.Length.ToString());

					// validar si contiene el evento onblur
					if (txt.Attributes.ContainsKey("onchange"))
						txt.Attributes["onchange"] = txt.Attributes["onchange"].Replace("ExecuteStrategy(", "applyFormatCurrency(this); ExecuteStrategy(");


				}

				#endregion tipo currency : MONEY

				#region solo texto

				// valida que sea de tipo solo texto para añadir el atributo class
				else if (this.IsText)
				{
					txt.Attributes.Add("value", Value);

					if (!isEmail)
						txt.Attributes.Add("class", this.Style + "_input ");
					else
						txt.Attributes.Add("class", this.Style + "_input email ");

					txt.Attributes.Add("onkeypress", "return soloLetras(event)");
					if (!string.IsNullOrEmpty(MaxSize)) txt.Attributes.Add("maxlength", MaxSize);
					else txt.Attributes.Add("maxlength", Constants.TF_MAX_TEXTO);
				}

				#endregion tipo numerico

				#region tipo texto

				else
				{
					txt.Attributes.Add("value", Value);

					if (!isEmail)
						txt.Attributes.Add("class", this.Style + "_input ");
					else
						txt.Attributes.Add("class", this.Style + "_input email ");

					if (!string.IsNullOrEmpty(MaxSize)) txt.Attributes.Add("maxlength", MaxSize);
					else txt.Attributes.Add("maxlength", Constants.TF_MAX_TEXTO);
				}

				#endregion tipo texto

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
        else {
          if (!Required && IsRequeried)
          {
            if (txt.Attributes.ContainsKey("class"))
            {
              txt.Attributes["class"] = txt.Attributes["class"] + " Not_required required ";
            }
            else
            {
              txt.Attributes.Add("class", "Not_required required");
            }
          }
        }
				#endregion

				if (txt.Attributes.ContainsKey("class"))
				{
					txt.Attributes["class"] = txt.Attributes["class"] + "  tooltip";
				}

				else
					txt.Attributes.Add("class", this.Style + "_input " + "tooltip");

				//if (isMathExpresion)
				//{
				//	if (txt.Attributes.ContainsKey("class"))
				//	{
				//		txt.Attributes["class"] = txt.Attributes["class"] + "  MathExpresion";
				//	}

				//	else
				//		txt.Attributes.Add("class", this.Style + "_input " + " MathExpresion");
				//}

				if (!flagEdit || IsReadOnly)
				{
					txt.Attributes.Add("disabled", "disabled");
					txt.Attributes.Add("readonly", "readonly");
					txt.Attributes.Add("title", ToolTip);
				}
				else
					if (ToolTip != string.Empty)
						txt.Attributes.Add("title", ToolTip);

				if (flagView) txt.Attributes.Add("type", "text");
				else txt.Attributes.Add("type", "hidden");

				if (eventName == "Hide")
          txt.Attributes.Add("style", "display: none !important; width: 70%");
				if (eventName == "Disabled")
					if (!txt.Attributes.ContainsKey("disabled"))
						txt.Attributes.Add("disabled", "disabled");

				if (txt.Attributes.ContainsKey("class"))
				{
					txt.Attributes["class"] = txt.Attributes["class"] + " " + listEvents;
				}
				else
				{
					txt.Attributes["class"] = " " + listEvents;
				}

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
			//html.Append("<hr/>");

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

			#region Strategy

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


			#endregion prompt label

			#region input element

			// input element data-val="true" data-val-required="*"
			var txt = new TagBuilder("input");
			txt.Attributes.Add("name", inputName);
			txt.Attributes.Add("id", inputName);

			/*if (idStrategy != 0)
				txt.Attributes.Add("onchange", "myFunction(this," + idStrategy + ")");*/

			#region tipo numerico

			// valida que sea de tipo numerico para añadir el atributo class
			if (this.IsNumber)
			{
				txt.Attributes.Add("value", Value);
				txt.Attributes.Add("class", "CustomInteger ValidateMinValue " + this.Style + "_input");
				if (!string.IsNullOrEmpty(MaxSize)) txt.Attributes.Add("maxlength", MaxSize.Length.ToString());
				else txt.Attributes.Add("maxlength", Constants.TF_MAX_NUMERO.Length.ToString());
			}

			#endregion tipo numerico

			#region tipo currency : MONEY

			// valida que sea de tipo currency : MONEY
			else if (this.IsCurrency)
			{
				txt.Attributes.Add("value", addCommas(Value.Split(',')[0].Split('.')[0]));
				//txt.Attributes.Add("class", "CustomCurrency " + this.Style + "_input");

				if (!string.IsNullOrEmpty(MinSize))
				{
					txt.Attributes.Add("class", " CustomCurrency CustomInteger ValidateMinValue " + this.Style + "_input");
					txt.Attributes.Add("min", MinSize);
				}
				else
					txt.Attributes.Add("class", " CustomCurrency CustomInteger" + this.Style + "_input");

				if (!string.IsNullOrEmpty(MaxSize)) txt.Attributes.Add("max", MaxSize);
				else txt.Attributes.Add("max", Constants.TF_MAX_MONEDA.Length.ToString());

				// validar si contiene el evento onblur
				if (txt.Attributes.ContainsKey("onchange"))
					txt.Attributes["onchange"] = txt.Attributes["onchange"].Replace("ExecuteStrategy(", "applyFormatCurrency(this); ExecuteStrategy(");
				else
					txt.Attributes.Add("onchange", "applyFormatCurrency(this);");
			}

			#endregion tipo currency : MONEY

			#region tipo texto

			else
			{
				txt.Attributes.Add("value", Value);
				if (!string.IsNullOrEmpty(MaxSize)) txt.Attributes.Add("maxlength", MaxSize);
				else txt.Attributes.Add("maxlength", Constants.TF_MAX_TEXTO);

				if (txt.Attributes.ContainsKey("class"))
				{
					txt.Attributes["class"] = txt.Attributes["class"] + "  onlyText";
				}

				else
					txt.Attributes.Add("class", "onlyText ");
			}

			#endregion tipo texto

			txt.Attributes.Add("type", "text");

			//if (txt.Attributes.ContainsKey("class"))
			//{
			//	txt.Attributes["class"] = txt.Attributes["class"];
			//}

			//else
			//	txt.Attributes.Add("class", this.Style + "_input ");

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
			//html.Append("<hr/>");

			return html.ToString();
		}

		private string addCommas(string nStr)
		{
			StringBuilder output = new StringBuilder();
			StringBuilder result = new StringBuilder();

			var rev = nStr.Reverse().ToArray();
			for (int i = 1; i <= rev.Length; i++)
			{
				output.Append(rev[i - 1]);
				if (i % 3 == 0 && i != rev.Length)
				{
					output.Append('.');
				}
			}

			result.Append("$ ");
			foreach (var item in output.ToString().Reverse().ToArray())
			{
				result.Append(item.ToString());
			}


			return result.ToString();
		}

	}
}

