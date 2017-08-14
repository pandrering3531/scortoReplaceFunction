using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace STPC.DynamicForms.Core.Fields
{
  /// <summary>
  /// Represents an html select element.
  /// </summary>
  [Serializable]
  public class Select : ListField
  {
    /// <summary>
    /// The number of options to display at a time.
    /// </summary>
    public int Size
    {
      get
      {
        string size;
        return _inputHtmlAttributes.TryGetValue("size", out size) ? int.Parse(size) : 1;
      }
      set { _inputHtmlAttributes["size"] = value.ToString(); }
    }
    /// <summary>
    /// Determines whether the select element will accept multiple selections.
    /// </summary>
    public bool MultipleSelection
    {
      get
      {
        string multiple;
        if (_inputHtmlAttributes.TryGetValue("multiple", out multiple))
        {
          return multiple.ToLower() == "multiple";
        }
        return false;
      }
      set
      {
        if (value)
        {
          _inputHtmlAttributes["multiple"] = "multiple";
        }
        else
        {
          _inputHtmlAttributes["multiple"] = string.Empty;
        }
      }
    }
    /// <summary>
    /// The text to be rendered as the first option in the select list when ShowEmptyOption is set to true.
    /// </summary>
    public string EmptyOption { get; set; }
    /// <summary>
    /// Determines whether a valueless option is rendered as the first option in the list.
    /// </summary>
    public bool ShowEmptyOption { get; set; }

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
        #region prompt

        // prompt

        var prompt = new TagBuilder("label");

        //prompt.AddCssClass(_promptClass);
        prompt.Attributes.Add("class", this.Style);
        prompt.Attributes.Add("for", inputName);
        prompt.SetInnerText(GetPrompt());
        if (eventName == "Hide") prompt.Attributes.Add("style", "display: none !important; width: 70%");
        if (eventName == "Disabled") prompt.Attributes.Add("disabled", "disabled");





        html.Replace(PlaceHolders.Prompt, prompt.ToString());


        #endregion prompt

        #region select list

        #region open select element

        // open select element
        var input = new StringBuilder();
        var select = new TagBuilder("select");
        select.Attributes.Add("id", inputName);
        select.Attributes.Add("name", inputName);

        // validar si es trigger de estrategia
        if (IsTriggerField && PageStrategyUid != null)
          select.Attributes.Add("onchange", "ExecuteStrategy(this,'" + inputName + "','" + PageStrategyUid + "')");

        // si true configura evento para inactivar un control
        if (IsTriguerEventChange == true)
        {
          if (!select.Attributes.ContainsKey("onchange"))
            select.Attributes.Add("onchange", "HideControl(this,'" + IdControlToHide + "','" + ValueWhenHideControl + "')");
          else
          {
            select.Attributes.Remove("onchange");
            select.Attributes.Add("onchange", "HideControl(this,'" + IdControlToHide + "','" + ValueWhenHideControl + "'); ExecuteStrategy(this,'" + inputName + "','" + PageStrategyUid + "')");

          }
          //select.Attributes.Add("onafterprint", "HideControl(this,'" + IdControlToHide + "','" + ValueWhenHideControl + "')");
        }
        #endregion open select element

        if (!flagEdit || IsReadOnly)
        {
          select.Attributes.Add("disabled", "disabled");
          select.Attributes.Add("readonly", "readonly");
          if (ToolTip != string.Empty)
            select.Attributes.Add("title", ToolTip);
        }
        else
        {
          if (ToolTip != string.Empty)
            select.Attributes.Add("title", ToolTip);
        }


        if (!flagView) select.Attributes.Add("type", "hidden");

        if (eventName == "Hide") select.Attributes.Add("style", "display: none !important; width: 70%");
        if (eventName == "Disabled")
        {
          if (!select.Attributes.ContainsKey("disabled"))
            select.Attributes.Add("disabled", "disabled");

        }

        #region valida requerido


        if (Required)
        {
          select.Attributes.Add("class", this.Style + "_select " + listEvents + " required tooltip");
        }
        else
          select.Attributes.Add("class", this.Style + "_select tooltip " + listEvents);

        if (!Required && IsRequeried)
        {
          if (select.Attributes.ContainsKey("class"))
          {
            select.Attributes["class"] = select.Attributes["class"] + " Not_required required";
          }
          else
          {
            select.Attributes.Add("class", "Not_required required");
          }
        }



        #endregion

        select.MergeAttributes(_inputHtmlAttributes);
        input.Append(select.ToString(TagRenderMode.StartTag));
        //select.Attributes.Add("tabindex", Index.ToString());


        #region initial empty option

        // initial empty option
        if (ShowEmptyOption)
        {
          var opt = new TagBuilder("option");
          opt.Attributes.Add("value", null);
          opt.SetInnerText(EmptyOption);
          input.Append(opt.ToString());
        }

        #endregion initial empty option

        #region options

        // options
        //if (!IsDependency)
        foreach (var choice in _choices)
        {
          var opt = new TagBuilder("option");
          string[] arrayGuid = choice.Value.Split('|');

          if (arrayGuid.Length > 1)
          {
            opt.Attributes.Add("value", arrayGuid[0]);
            opt.Attributes.Add("Text", arrayGuid[1]);

            if (arrayGuid.Length > 2)
              if (arrayGuid[2] == "False")
              {
                opt.Attributes.Add("disabled", "");
              }

          }
          else
          {
            opt.Attributes.Add("value", arrayGuid[0]);
            opt.Attributes.Add("Text", arrayGuid[0]);
          }
          if (choice.Selected)
            opt.Attributes.Add("selected", "selected");

          opt.Attributes.Add("name", inputName);

          opt.MergeAttributes(choice.HtmlAttributes);

          if (arrayGuid.Length > 1)
            opt.SetInnerText(arrayGuid[1]);
          else
            opt.SetInnerText(arrayGuid[0]);

          if (arrayGuid.Length > 2)
          {
            if ((!choice.Selected && arrayGuid[2] == "True") || (choice.Selected && arrayGuid[2] == "False") || (choice.Selected && arrayGuid[2] == "True"))
              input.Append(opt.ToString());
          }
          else
            input.Append(opt.ToString());

        }

        #endregion options

        #region close select element

        // close select element
        input.Append(select.ToString(TagRenderMode.EndTag));

        #endregion close select element

        #region hidden tag

        // add hidden tag, so that a value always gets sent for select tags
        var hidden = new TagBuilder("input");
        hidden.Attributes.Add("type", "hidden");
        hidden.Attributes.Add("id", inputName + "_hidden");
        hidden.Attributes.Add("name", inputName);
        hidden.Attributes.Add("value", string.Empty);

        #endregion hidden tag




        html.Replace(PlaceHolders.Input, input.ToString() + hidden.ToString(TagRenderMode.SelfClosing));

        #endregion select list
      }

      #region error label

      // error label
      if (!ErrorIsClear)
      {
        var error = new TagBuilder("label");
        error.AddCssClass(_errorClass);
        error.Attributes.Add("for", inputName);
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

      #region prompt

      // prompt
      var prompt = new TagBuilder("label");
      prompt.AddCssClass(_promptClass);
      prompt.Attributes.Add("for", inputName);
      prompt.SetInnerText(GetPrompt());
      html.Replace(PlaceHolders.Prompt, prompt.ToString());

      #endregion prompt

      #region select list

      #region open select element

      // open select element
      var input = new StringBuilder();
      var select = new TagBuilder("select");
      select.Attributes.Add("id", inputName);
      select.Attributes.Add("name", inputName);

      #endregion open select element

      select.MergeAttributes(_inputHtmlAttributes);
      input.Append(select.ToString(TagRenderMode.StartTag));
      //select.Attributes.Add("tabindex", Index.ToString());

      #region initial empty option

      // initial empty option
      if (ShowEmptyOption)
      {
        var opt = new TagBuilder("option");
        opt.Attributes.Add("value", null);
        opt.SetInnerText(EmptyOption);
        input.Append(opt.ToString());
      }

      #endregion initial empty option

      #region options

      // options
      foreach (var choice in _choices)
      {
        var opt = new TagBuilder("option");
        string[] arrayGuid = choice.Value.Split('|');

        if (arrayGuid.Length > 1)
        {
          opt.Attributes.Add("value", arrayGuid[0]);
          opt.Attributes.Add("Text", arrayGuid[1]);
        }
        else
        {
          opt.Attributes.Add("value", arrayGuid[0]);
          opt.Attributes.Add("Text", arrayGuid[0]);
        }
        if (choice.Selected)
          opt.Attributes.Add("selected", "selected");

        opt.Attributes.Add("name", inputName);

        opt.MergeAttributes(choice.HtmlAttributes);

        if (arrayGuid.Length > 1)
          opt.SetInnerText(arrayGuid[1]);
        else
          opt.SetInnerText(arrayGuid[0]);

        input.Append(opt.ToString());

      }

      #endregion options

      #region close select element

      // close select element
      input.Append(select.ToString(TagRenderMode.EndTag));

      #endregion close select element

      #region hidden tag

      // add hidden tag, so that a value always gets sent for select tags
      var hidden = new TagBuilder("input");
      hidden.Attributes.Add("type", "hidden");
      hidden.Attributes.Add("id", inputName + "_hidden");
      hidden.Attributes.Add("name", inputName);
      hidden.Attributes.Add("value", string.Empty);
      html.Replace(PlaceHolders.Input, input.ToString() + hidden.ToString(TagRenderMode.SelfClosing));

      #endregion hidden tag

      #endregion select list

      #region error label

      // error label
      if (!ErrorIsClear)
      {
        var error = new TagBuilder("label");
        error.AddCssClass(_errorClass);
        error.Attributes.Add("for", inputName);
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

