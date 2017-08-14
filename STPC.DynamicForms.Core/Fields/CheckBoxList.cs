using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System;

namespace STPC.DynamicForms.Core.Fields
{
  /// <summary>
  /// Represents a list of html checkbox inputs.
  /// </summary>
  [Serializable]
  public class CheckBoxList : OrientableField
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
        prompt.AddCssClass(_promptClass + " " + this.Style + "_title");
        prompt.SetInnerText(GetPrompt());
        prompt.Attributes.Add("for", inputName);


        if (eventName == "Hide") prompt.Attributes.Add("style", "display: none !important;");
        if (eventName == "Disabled")
          if (!prompt.Attributes.ContainsKey("disabled"))
            prompt.Attributes.Add("disabled", "disabled");

        html.Replace(PlaceHolders.Prompt, prompt.ToString());

        #endregion prompt label

        #region list of checkboxes

        // list of checkboxes			

        var input = new StringBuilder();
        var ul = new TagBuilder("ul");
        ul.Attributes.Add("class", _orientation == Orientation.Vertical ? _verticalClass + " " + listEvents : _horizontalClass + " " + listEvents);
        ul.AddCssClass(_listClass + " " + this.Style);
        ul.Attributes.Add("id", inputName);
        string controlsToHide = string.Empty;

        if (ListIdControlToHide != null)
          for (int i = 0; i < ListIdControlToHide.Length; i++)
          {
            IdControlToHide += ListIdControlToHide[i] + ",";

          }

        if (IsTriguerEventChange == true)
          ul.Attributes.Add("onclick", "HideCheckListControl(this,'" + IdControlToHide + "','" + ValueWhenHideControl + "')");



        // validar si es trigger
        if (IsTriggerField && PageStrategyUid != null)
          ul.Attributes.Add("onchange", "ExecuteStrategy(this,'" + inputName + "','" + PageStrategyUid + "')");

        //if (IsTriguerEventChange == true)
        //  ul.Attributes.Add("onchange", "HideCheckListControl(this,'" + IdControlToHide + "','" + ValueWhenHideControl + "')");

        #region valida requerido

        if (Required)
        {
          if (ul.Attributes.ContainsKey("class"))
          {
            if (eventName != "Disabled")
              if (!ul.Attributes.ContainsKey("disabled"))
                ul.Attributes["class"] = ul.Attributes["class"] + " requiredcheckBox";
          }
        }

        if (!Required && IsRequeried)
        {
          if (ul.Attributes.ContainsKey("class"))
          {
            ul.Attributes["class"] = ul.Attributes["class"] + " Not_requiredcheckBox";
          }
          else
          {
            ul.Attributes.Add("class", "Not_requiredcheckBox");
          }
        }


        ul.Attributes["class"] = ul.Attributes["class"] + "  tooltip";
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
          string chkId = inputName;

          #region open list item

          // open list item
          var li = new TagBuilder("li");

          if (!flagEdit || IsReadOnly)
          {
            li.Attributes.Add("disabled", "disabled");
            li.Attributes.Add("readonly", "readonly");
          }

          if (!flagView) li.Attributes.Add("type", "hidden");

          // valida el atributo horizontal o vertical
          if (_orientation == Orientation.Horizontal)
            li.Attributes.Add("style", "display: inline;list-style-type: none;");



          input.Append(li.ToString(TagRenderMode.StartTag));

          #endregion open list item

          #region checkbox input

          // checkbox input
          var chk = new TagBuilder("input");
          chk.Attributes.Add("type", "checkbox");
          chk.Attributes.Add("name", inputName);
          chk.Attributes.Add("id", chkId);
          chk.Attributes.Add("class", this.Style + "_input");

          string[] arrayGuid = choice.Value.Split('|');

          //chk.Attributes.Add("value", choice.Value);
          chk.Attributes.Add("value", arrayGuid[0]);
          chk.Attributes.Add("Text", arrayGuid[1]);

          if (choice.Selected)
            chk.Attributes.Add("checked", "checked");
          //chk.Attributes.Add("tabindex", Index.ToString());

          if (arrayGuid.Length > 2)
            if (!flagEdit || IsReadOnly || arrayGuid[2] == "False")
            {
              chk.Attributes.Add("disabled", "disabled");
              chk.Attributes.Add("readonly", "readonly");
            }


          if (eventName == "Hide")
            chk.Attributes.Add("style", "visibility: hidden;");
          if (eventName == "Disabled")
          {
            string value = chk.Attributes.FirstOrDefault(x => x.Value == "disabled").Key;
            if (string.IsNullOrEmpty(value))
              chk.Attributes.Add("disabled", "disabled");
          }

          if (arrayGuid.Length > 2)
          {
            if ((!choice.Selected && arrayGuid[2] == "True") || (choice.Selected && arrayGuid[2] == "False") || (choice.Selected && arrayGuid[2] == "True"))
            {
              chk.MergeAttributes(_inputHtmlAttributes);
              chk.MergeAttributes(choice.HtmlAttributes);
              input.Append(chk.ToString(TagRenderMode.SelfClosing));
            }
          }
          else
          {
            chk.MergeAttributes(_inputHtmlAttributes);
            chk.MergeAttributes(choice.HtmlAttributes);
            input.Append(chk.ToString(TagRenderMode.SelfClosing));
          }
          #endregion checkbox input

          #region checkbox label

          if (arrayGuid.Length > 2)
          {
            if ((!choice.Selected && arrayGuid[2] == "True") || (choice.Selected && arrayGuid[2] == "False") || (choice.Selected && arrayGuid[2] == "True"))
            {
              // checkbox label
              var lbl = new TagBuilder("label");
              lbl.Attributes.Add("for", chkId);
              lbl.AddCssClass(_inputLabelClass + " " + this.Style + "_label");
              lbl.SetInnerText(arrayGuid[1]);
              input.Append(lbl.ToString());
            }
          }
          else
          {
            var lbl = new TagBuilder("label");
            lbl.Attributes.Add("for", chkId);
            lbl.AddCssClass(_inputLabelClass + " " + this.Style + "_label");
            lbl.SetInnerText(arrayGuid[1]);
            input.Append(lbl.ToString());
          }
          #endregion checkbox label

          #region close list item

          // close list item
          input.Append(li.ToString(TagRenderMode.EndTag));

          #endregion close list item

        }

        #region open list item

        var li2 = new TagBuilder("li");
        input.Append(li2.ToString(TagRenderMode.StartTag));

        #endregion open list item


        var chk2 = new TagBuilder("input");
        chk2.Attributes.Add("type", "hidden");
        chk2.Attributes.Add("name", inputName);
        chk2.Attributes.Add("id", inputName);
        chk2.Attributes.Add("checked", "checked");
        chk2.Attributes.Add("value", "");
        chk2.MergeAttributes(_inputHtmlAttributes);
        input.Append(chk2.ToString(TagRenderMode.SelfClosing));
        input.Append(li2.ToString(TagRenderMode.EndTag));

        input.Append(ul.ToString(TagRenderMode.EndTag));

        #endregion list of checkboxes

        #region hidden tag

        // add hidden tag, so that a value always gets sent
        var hidden = new TagBuilder("input");
        hidden.Attributes.Add("type", "hidden");
        hidden.Attributes.Add("id", inputName + "_hidden");
        hidden.Attributes.Add("name", inputName);
        hidden.Attributes.Add("value", string.Empty);

        html.Replace(PlaceHolders.Input, input.ToString() + hidden.ToString(TagRenderMode.SelfClosing));

        #endregion hidden tag
      }

      #region error label

      // error label
      if (!ErrorIsClear)
      {
        var error = new TagBuilder("label");
        error.AddCssClass(_errorClass); ;
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

      #region list of checkboxes

      // list of checkboxes
      var input = new StringBuilder();
      var ul = new TagBuilder("ul");
      ul.AddCssClass(_orientation == Orientation.Vertical ? _verticalClass : _horizontalClass);
      ul.AddCssClass(_listClass);
      input.Append(ul.ToString(TagRenderMode.StartTag));

      var choicesList = _choices.ToList();
      for (int i = 0; i < choicesList.Count; i++)
      {
        ListItem choice = choicesList[i];
        string chkId = inputName + i;

        #region open list item

        // open list item
        var li = new TagBuilder("li");

        // valida el atributo horizontal o vertical
        if (_orientation == Orientation.Horizontal)
          li.Attributes.Add("style", "display: inline;list-style-type: none;");

        input.Append(li.ToString(TagRenderMode.StartTag));

        #endregion open list item

        #region checkbox input

        // checkbox input
        var chk = new TagBuilder("input");
        chk.Attributes.Add("type", "checkbox");
        chk.Attributes.Add("name", inputName);
        chk.Attributes.Add("id", chkId);
        chk.Attributes.Add("value", choice.Value);
        if (choice.Selected)
          chk.Attributes.Add("checked", "checked");
        //chk.Attributes.Add("tabindex", Index.ToString());

        chk.MergeAttributes(_inputHtmlAttributes);
        chk.MergeAttributes(choice.HtmlAttributes);
        input.Append(chk.ToString(TagRenderMode.SelfClosing));

        #endregion checkbox input

        #region checkbox label

        // checkbox label
        var lbl = new TagBuilder("label");
        lbl.Attributes.Add("for", chkId);
        lbl.AddCssClass(_inputLabelClass);
        lbl.SetInnerText(choice.Text);
        input.Append(lbl.ToString());

        #endregion checkbox label

        #region close list item

        // close list item
        input.Append(li.ToString(TagRenderMode.EndTag));

        #endregion close list item

      }


      #endregion list of checkboxes

      #region hidden tag

      // add hidden tag, so that a value always gets sent
      var hidden = new TagBuilder("input");
      hidden.Attributes.Add("type", "hidden");
      hidden.Attributes.Add("id", inputName + "_hidden");
      hidden.Attributes.Add("name", inputName);
      hidden.Attributes.Add("value", string.Empty);

      html.Replace(PlaceHolders.Input, input.ToString() + hidden.ToString(TagRenderMode.SelfClosing));

      #endregion hidden tag

      #region error label

      // error label
      if (!ErrorIsClear)
      {
        var error = new TagBuilder("label");
        error.AddCssClass(_errorClass); ;
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
