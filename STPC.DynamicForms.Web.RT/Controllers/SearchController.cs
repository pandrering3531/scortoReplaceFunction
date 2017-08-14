using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.Common;
using STPC.DynamicForms.Web.Common.Messages;
using STPC.DynamicForms.Web.RT.Helpers;
using STPC.DynamicForms.Web.RT.Models;

namespace STPC.DynamicForms.Web.RT.Controllers
{
    [Authorize]

    public class SearchController : Controller
    {
        //
        Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
        // GET: /Search/
        CustomRequestProvider _RequestServiceClient = new CustomRequestProvider();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SearchContractPost(string srNumRequest, string srNumCedula)
        {
            //string storeProcedureName = System.Configuration.ConfigurationManager.AppSettings["InterviewsProcedureName"];
            int fetch = 20;

            if (System.Web.Configuration.WebConfigurationManager.AppSettings["RecordCountByPage"] != null)
            {
                fetch = Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["RecordCountByPage"].ToString());
            }

            // usar el servicio para consultar el store procedure

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@requestId", srNumRequest);
            if (srNumCedula.Trim() == "")
                parameters.Add("@Cedula", "0");
            else
                parameters.Add("@Cedula", srNumCedula);


            var request = _RequestServiceClient.GetIndicatorByProcedure("spGetConsultaSolicitudes", parameters);


            int totlaCount = 0;
            if (request.Rows.Count > 0)
            {
                foreach (var item in request.Rows[0].Values)
                {
                    if (item.ColumnName == "TotalCount")
                    {
                        totlaCount = Convert.ToInt32(item.Value);
                    }
                }
            }

            ViewBag.nameSp = "spGetConsultaSolicitudes";
            ViewBag.PageCount = totlaCount / fetch;
            ViewBag.fetchCount = fetch.ToString();


            return PartialView("_QuerySolicitud", request);
        }

        [HttpPost]
        public ActionResult DetailContractPost(string srNumRequest)
        {
            Models.DetailRequestModel model = new Models.DetailRequestModel();

            //string storeProcedureName = System.Configuration.ConfigurationManager.AppSettings["InterviewsProcedureName"];
            int fetch = 20;

            if (System.Web.Configuration.WebConfigurationManager.AppSettings["RecordCountByPage"] != null)
            {
                fetch = Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["RecordCountByPage"].ToString());
            }

            // usar el servicio para consultar el store procedure

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@requestId", srNumRequest);

            var request = _RequestServiceClient.GetIndicatorByProcedure("spGetConsultaSolicitudesDetalle", parameters);


            if (request.Rows.Count > 0)
            {
                foreach (var item in request.Rows[0].Values)
                {
                    if (request.Columns[item.Index].Name == "Radicado")
                        model.Radicado = item.Value;
                    else if (request.Columns[item.Index].Name == "Solicitante")
                        model.Solicitante = item.Value;
                    else if (request.Columns[item.Index].Name == "Valor")
                        model.Valor = item.Value;
                    else if (request.Columns[item.Index].Name == "Identificacion")
                        model.Identificacion = item.Value;
                    else if (request.Columns[item.Index].Name == "Plazo")
                        model.Plazo = item.Value;
                    else if (request.Columns[item.Index].Name == "ObservacionAnalista")
                        model.ObservacionAnalista = item.Value;
                    else if (request.Columns[item.Index].Name == "ObservacionCooordinador")
                        model.ObservacionCooordinador = item.Value;
                    else if (request.Columns[item.Index].Name == "ObservacionComite")
                        model.ObservacionComite = item.Value;
                    else if (request.Columns[item.Index].Name == "ObservacionVisacion")
                        model.ObservacionVisacion = item.Value;
                    else if (request.Columns[item.Index].Name == "ObservacionCapacidadPago")
                        model.ObservacionCapacidadPago = item.Value;

                }
            }

            //Carga y llena la informacion del log de actividades
            List<Models.RegistroEventos> ListaEventos = new List<Models.RegistroEventos>();

            Dictionary<string, string> parametersLog = new Dictionary<string, string>();
            parametersLog.Add("@requestId", srNumRequest);

            var requestLog = _RequestServiceClient.GetIndicatorByProcedure("spGetConsultaSolicitudesLogActividades", parametersLog);

            if (requestLog.Rows.Count > 0)
            {
                foreach (var row in requestLog.Rows)
                {
                    Models.RegistroEventos Evento = new Models.RegistroEventos();

                    foreach (var item in row.Values)
                    {
                        if (requestLog.Columns[item.Index].Name == "Area")
                        {
                            Evento.Area = item.Value;
                        }
                        else if (requestLog.Columns[item.Index].Name == "Actividad")
                        {
                            Evento.Actividad = item.Value;
                        }
                        else if (requestLog.Columns[item.Index].Name == "Fecha")
                        {
                            Evento.Fecha = item.Value;
                        }
                        else if (requestLog.Columns[item.Index].Name == "Hora")
                        {
                            Evento.Hora = item.Value;
                        }
                        else if (requestLog.Columns[item.Index].Name == "Usuario")
                        {
                            Evento.Usuario = item.Value;
                        }
                        else if (requestLog.Columns[item.Index].Name == "Radicado")
                        {
                            Evento.Radicado = srNumRequest;
                        }
                    }
                    ListaEventos.Add(Evento);
                }
            }

            model.LogEventos = ListaEventos;

            //revision para el cargue de causales
            /*
             * 
             * 
                
            "APLAZADO - Área de crédito" y "APLAZADO - Área de crédito - No gestionado Call center" carga las causales 2 y 3 y 4
            "NEGADO - Área de crédito carga las causales" y "NEGADO - Área de crédito - No gestionado Call center " carga las causales 1 y 4
            */

            if (model.EstadoActualSolicitud == "APLAZADO - Área de crédito" || model.EstadoActualSolicitud == "APLAZADO - Área de crédito - No gestionado Call center")
            {
                var Motivos = _stpcForms.Options.Where(p => p.Category_Uid == 44 && (p.Key == "2" || p.Key == "3" || p.Key == "4"));

                var modelmotivos = Motivos.Select(m => new StructControl()
                {
                    Text = m.Value,
                    Value = m.Uid.ToString()

                });

                ViewBag.ListaMotivosGestion = modelmotivos;
            }
            else if (model.EstadoActualSolicitud == "NEGADO - Área de crédito" || model.EstadoActualSolicitud == "NEGADO - Área de crédito - No gestionado Call center")
            {
                var Motivos = _stpcForms.Options.Where(p => p.Category_Uid == 44 && (p.Key == "1" || p.Key == "4"));

                var modelmotivos = Motivos.Select(m => new StructControl()
                {
                    Text = m.Value,
                    Value = m.Uid.ToString()

                });

                ViewBag.ListaMotivosGestion = modelmotivos;
            }
            else
            {
                var Motivos = _stpcForms.Options.Where(p => p.Category_Uid == 44);

                var modelmotivos = Motivos.Select(m => new StructControl()
                {
                    Text = m.Value,
                    Value = m.Uid.ToString()
                });

                ViewBag.ListaMotivosGestion = modelmotivos;
            }


            ViewBag.RequestId = model.Radicado;
            ViewBag.EstadoActual = model.EstadoActualSolicitud;

            return PartialView("_QueryDetalle", model);
        }


        [HttpPost]
        public JsonResult GuardarGestion(GuardarGestionView model)
        {
            String result = "1";
            try
            {
                if (ModelState.IsValid)
                {
                    model.Usuario = User.Identity.Name;

                    if (model.Causal == "16344") //Pasar a lista de trabajo comercial
                    {
                        model.EstadoNuevo = "81752936-C299-4EF7-9084-D764B8D4BDC3"; //81752936-C299-4EF7-9084-D764B8D4BDC3	NEGADO - Área de crédito - Call center
                    }
                    else if (model.Causal == "16345") //Pasar a lista de trabajo analista de crédito
                    {
                        model.EstadoNuevo = "E1C1D19E-FF78-41DA-9FD2-A9AF45D500C0"; //E1C1D19E-FF78-41DA-9FD2-A9AF45D500C0	APLAZADO - Área de crédito - Call center 
                    }
                    else if (model.Causal == "16346") //Para desembolso
                    {
                        model.EstadoNuevo = "A8E75909-BC12-46E4-8C4B-FCEDD7D1A1F9"; //A8E75909-BC12-46E4-8C4B-FCEDD7D1A1F9	PENDIENTE - Cargue de soporte UBICA
                    }
                    else if (model.Causal == "16347") //No gestionado
                    {
                        if (model.EstadoActual.Contains("NEGADO"))
                        {
                            model.EstadoNuevo = "3E091C7B-9845-459B-9DF0-CC113C5CA340"; //3E091C7B-9845-459B-9DF0-CC113C5CA340	NEGADO - Área de crédito - No gestionado Call center
                        }
                        else if (model.EstadoActual.Contains("APLAZADO"))
                        {
                            model.EstadoNuevo = "3CDB5BC2-9159-4EEB-AB6A-2BEA8B354DCD"; //3CDB5BC2-9159-4EEB-AB6A-2BEA8B354DCD	APLAZADO - Área de crédito - No gestionado Call center
                        }
                        else
                        {
                            model.EstadoNuevo = "";
                        }
                    }

                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("@requestId", model.Radicado);
                    parameters.Add("@Estadonuevo", model.EstadoNuevo);
                    parameters.Add("@Causal", model.Causal);
                    parameters.Add("@observaciones", model.Observaciones);
                    parameters.Add("@usuario", model.Usuario);

                    var request = _RequestServiceClient.GetIndicatorByProcedure("spGetConsultaSolicitudesUpdate", parameters);

                }
                else
                {
                    var error = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();
                    if (error.Count > 0)
                    {
                        result = "";
                        int c = 0;
                        foreach (var r in error)
                        {
                            result = result + r[c].ErrorMessage + '\n';
                            c = c + 1;
                        }
                    }
                    else
                    {
                        result = "Error no identificado";
                    }
                }
            }
            catch (Exception e)
            {
                result = "0";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //private ActionResult DefaultActionErrorHandling(Exception ex, string triggerAction)
        //{
        //    bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
        //    Guid correlationID = Guid.NewGuid();

        //    ILogging eventWriter = LoggingFactory.GetInstance();
        //    string errorMessage = string.Format(CustomMessages.E0007, "RequestController", triggerAction, correlationID, ex.Message);
        //    System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
        //    eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
        //    if (ShowErrorDetail)
        //    {
        //        return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(errorMessage), "RequestController", triggerAction));
        //    }
        //    else
        //    {
        //        return PartialView("_ErrorGeneral", string.Format(CustomMessages.E0001, correlationID.ToString()));
        //    }
        //}
    }
}
