using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.Mvc;
using System.Xml.Serialization;


namespace STPC.DynamicForms.Core.Fields
{
  public delegate void FilePostedEventHandler(FileUpload fileUploadField, EventArgs e);

  [Serializable]
  public class FileUpload : InputField
  {
    public event FilePostedEventHandler Posted;

    [NonSerialized]
    private HttpPostedFileBase _postedFile;
    private string _invalidExtensionError = "Invalid File Type";

    public string InvalidExtensionError
    {
      get { return _invalidExtensionError; }
      set { _invalidExtensionError = value; }
    }

    public HttpPostedFileBase PostedFile
    {
      get { return _postedFile; }
      set { _postedFile = value; }
    }

    /// <summary>
    /// A comma delimited list of acceptable file extensions.
    /// </summary>
    public string ValidExtensions { get; set; }
    public string ErrorExtensions { get; set; }

    public bool FileWasPosted
    {
      get
      {
        return PostedFile != null && !string.IsNullOrEmpty(PostedFile.FileName);
      }
    }

    /// <summary>
    /// Gets or sets the URI file path.
    /// </summary>
    /// <value>
    /// The URI file path.
    /// </value>
    public string UriFilePath { get; set; }

    public override string Response
    {
      get { return PostedFile.FileName; }
    }

    public override bool Validate()
    {
      ClearError();

      if (Required && !FileWasPosted)
      {
        Error = RequiredMessage;
      }
      else if (!string.IsNullOrEmpty(ValidExtensions))
      {
        var exts = ValidExtensions.ToUpper().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        if (!exts.Contains(Path.GetExtension(PostedFile.FileName).ToUpper()))
        {
          Error = InvalidExtensionError;
        }
      }

      FireValidated();
      return ErrorIsClear;
    }

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


        prompt.Attributes.Add("class", _promptClass);


        if (eventName == "Hide")
          prompt.Attributes.Add("style", "display: none !important; width: 70%");
        if (eventName == "Disabled")
          if (!prompt.Attributes.ContainsKey("disabled"))
            prompt.Attributes.Add("disabled", "disabled");

        html.Replace(PlaceHolders.Prompt, prompt.ToString());

        #endregion prompt label

        #region input element

        // input element
        var txt = new TagBuilder("input");
        txt.Attributes.Add("name", inputName);
        txt.Attributes.Add("id", inputName);
        //txt.Attributes.Add("type", "button");
        txt.Attributes.Add("value", "Cargar Archivo");

        //txt.Attributes.Add("onClick", "ModalFileUpload(this);");
        //txt.Attributes.Add("onClick", "ModalFileUpload(this, " + MaxSize + ");");
        txt.Attributes.Add("onClick", "ModalFileUpload(this, " + MaxSize + ", '" + ValidExtensions + "', '" + ErrorExtensions + "');");

        // validar si es trigger
        if (IsTriggerField && PageStrategyUid != null)
          txt.Attributes.Add("onchange", "ExecuteStrategy(this,'" + inputName + "','" + PageStrategyUid + "')");

        if (!flagEdit || IsReadOnly)
        {
          txt.Attributes.Add("disabled", "disabled");
          txt.Attributes.Add("readonly", "readonly");
        }
        if (ToolTip != string.Empty)
          txt.Attributes.Add("title", ToolTip);

        if (flagView) txt.Attributes.Add("type", "button");
        else txt.Attributes.Add("type", "hidden");
        txt.Attributes.Add("class", "tooltip");

        if (eventName == "Hide")
          txt.Attributes.Add("style", "display: none !important; width: 70%");
        if (eventName == "Disabled")
          if (!txt.Attributes.ContainsKey("disabled"))
            txt.Attributes.Add("disabled", "disabled");

        txt.MergeAttributes(_inputHtmlAttributes);
        //txt.Attributes.Add("tabindex", Index.ToString());
        html.Replace(PlaceHolders.Input, txt.ToString(TagRenderMode.SelfClosing));



        #endregion input element

        #region link element

        // link element
        StringBuilder lnk = new StringBuilder();
        lnk.Append("<a ");
        lnk.Append(" id='UriFile_" + inputName + "'");
        lnk.Append(" target='_blank'");
        lnk.Append(" tabindex='" + Index.ToString() + "'");

        if (!string.IsNullOrEmpty(UriFilePath))
        {
          //if (!UriFilePath.Contains("http://")) UriFilePath = "http://" + UriFilePath;

          lnk.Append(" href='" + UriFilePath + "'");
          lnk.Append(" text='" + UriFilePath + "'");
          lnk.Append(" value='" + UriFilePath + "'");
        }
        else
        {
          lnk.Append(" text=''");
          lnk.Append(" value=''");
        }
        //else
        //	lnk.Append(" style='visibility:hidden'");



        if (eventName == "Hide")
        {
          lnk.Append(" style='visibility:hidden'");
        }
        else
        {
          //txt.Attributes.Add("style", "visibility: hidden; width: 70%");
          if (eventName == "Disabled")
          {
            lnk.Append(" style='disabled:disabled'");
            if (this.Required)
              lnk.Append(" class='Not_required FileView'");

          }
          else
          {
            if (this.Required)
              lnk.Append(" class='required FileView'");
            if (!Required && IsRequeried)
            {
              if (lnk.ToString().Contains("class"))
              {
                //select.Attributes["class"] = select.Attributes["class"] + " Not_required required";
              }
              else
              {
                lnk.Append(" class='Not_required required'");
              }
            }
          }
        }


        //if (eventName == "Hide")
        //	lnk.Append(" style='visibility:hidden'");
        //else
        //{
        //	if (this.Required)
        //		lnk.Append(" class='required FileView'");

        //	lnk.Append(" style='visibility:visible'");
        //}

        lnk.Append(" >");
        if (!string.IsNullOrEmpty(UriFilePath))
        {
          lnk.Append("Ver");
        }
        else
        {
          lnk.Append("");
        }
        lnk.Append("</a>");


        html.Replace("</div>", lnk.ToString() + "</div>");

        #endregion link element

        #region link element borrar

        if (flagEdit)
        {
          // link element
          StringBuilder lnkDeleteFile = new StringBuilder();
          lnkDeleteFile.Append("<a ");
          lnkDeleteFile.Append(" id='UriFileDelete_" + inputName + "'");
          lnkDeleteFile.Append(" target='_blank'");
          lnkDeleteFile.Append(" tabindex='" + Index.ToString() + "'");

          if (!string.IsNullOrEmpty(UriFilePath))
          {
            //if (!UriFilePath.Contains("http://")) UriFilePath = "http://" + UriFilePath;
            lnkDeleteFile.Append(" onClick='DeleteFileLink(this)'");

            lnkDeleteFile.Append(" text='" + UriFilePath + "'");
            lnkDeleteFile.Append(" value='" + UriFilePath + "'");
          }
          else
          {
            lnkDeleteFile.Append(" text=''");
            lnkDeleteFile.Append(" value=''");
          }
          //else
          //	lnk.Append(" style='visibility:hidden'");





          if (eventName == "Hide")
            lnkDeleteFile.Append(" style='visibility:hidden'");
          else
          {
            lnkDeleteFile.Append(" style='visibility:visible;cursor:pointer;'");
          }

          lnkDeleteFile.Append(" >");
          if (!string.IsNullOrEmpty(UriFilePath))
          {
            lnkDeleteFile.Append("Borrar");
          }
          else
          {
            lnkDeleteFile.Append(" ");
          }
          lnkDeleteFile.Append("</a>");


          html.Replace("</div>", lnkDeleteFile.ToString() + "</div>");
        }
        #endregion link element

        #region hidden input element

        // input element
        var hiddenInput = new TagBuilder("input");
        hiddenInput.Attributes.Add("name", inputName);
        hiddenInput.Attributes.Add("id", "hidden_" + inputName);
        if (!string.IsNullOrEmpty(UriFilePath)) hiddenInput.Attributes.Add("value", FileId);
        hiddenInput.Attributes.Add("type", "text");
        hiddenInput.Attributes.Add("style", "visibility:hidden");
        html.Replace("</div>", hiddenInput.ToString(TagRenderMode.SelfClosing) + "</div>");

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
      var txt = new TagBuilder("input");
      txt.Attributes.Add("name", inputName);
      txt.Attributes.Add("id", inputName);
      //txt.Attributes.Add("type", "button");
      txt.Attributes.Add("value", "Cargar Archivo");

      //txt.Attributes.Add("onClick", "ModalFileUpload(this);");
      //txt.Attributes.Add("onClick", "ModalFileUpload(this, " + MaxSize + ");");
      txt.Attributes.Add("onClick", "ModalFileUpload(this, " + MaxSize + ", '" + ValidExtensions + "', '" + ErrorExtensions + "');");

      txt.Attributes.Add("type", "button");

      txt.MergeAttributes(_inputHtmlAttributes);
      //txt.Attributes.Add("tabindex", Index.ToString());
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

    internal void FireFilePosted()
    {
      if (FileWasPosted && Posted != null)
        Posted(this, new EventArgs());
    }
  }
}
