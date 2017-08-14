#region

using System.Web.Mvc;

#endregion

namespace STPC.DynamicForms.Web.Helpers
{
    public class AjaxViewResult : ViewResult
    {
        public AjaxViewResult(string viewName, object model)
        {
            ViewName = viewName;
            ViewData = new ViewDataDictionary {Model = model};
        }

        public string UpdateValidationForFormId { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var result = base.FindView(context);
            var viewContext = new ViewContext(context, result.View, ViewData, TempData,
                                              context.HttpContext.Response.Output);

            BeginCapturingValidation(viewContext);
            base.ExecuteResult(context);
            EndCapturingValidation(viewContext);

            result.ViewEngine.ReleaseView(context, result.View);
        }

        private void BeginCapturingValidation(ViewContext viewContext)
        {
            if (string.IsNullOrEmpty(UpdateValidationForFormId))
                return;
            viewContext.ClientValidationEnabled = true;
            viewContext.FormContext = new FormContext {FormId = UpdateValidationForFormId};
        }

        private static void EndCapturingValidation(ViewContext viewContext)
        {
            if (!viewContext.ClientValidationEnabled)
                return;
            viewContext.OutputClientValidation();
            viewContext.Writer.WriteLine(
                "<script type=\"text/javascript\">Sys.Mvc.FormContext._Application_Load()</script>");
        }
    }
}