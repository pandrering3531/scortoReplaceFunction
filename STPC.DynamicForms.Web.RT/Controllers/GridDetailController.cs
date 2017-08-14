using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using STPC.DynamicForms.Core;
using STPC.DynamicForms.Web.RT.Services.Entities;
using STPC.DynamicForms.Web.RT.Models;
using STPC.DynamicForms.Core.Fields;
using STPC.DynamicForms.Web.Common;
using STPC.DynamicForms.Web.RT.Helpers;
using System.Web.Security;
using Newtonsoft.Json;
using System.Configuration;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.Common.Messages;
using System.Text;
using STPC.DynamicForms.DecisionEngine;
using System.Web.Helpers;
using System.Globalization;

namespace STPC.DynamicForms.Web.RT.Controllers
{
    [HandleError]
    public class GridDetailController : BaseController
    {
        Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
        
        #region Simulador BE
        public ActionResult BEIndex(int requestId, string EditRol, int page = 1)
        {
            int pageSize = 10;
            int totalpages = 0;
            int totalRecords = 0;

            ViewBag.RequestId = requestId;

            if (EditRol == "true" || ViewBag.EditRoles == "block")
            {
                ViewBag.EditRoles = "none";
            }
            else
            {
                ViewBag.EditRoles = "block";
            }

            var datosgrid = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasBE.Where(p => p.F_RequestId == requestId && p.F_bitEliminado==false);

            totalRecords = datosgrid.Count();
            totalpages = (totalRecords / pageSize) + ((totalRecords % pageSize) > 0 ? 1 : 0);

            var datosGridPagina = datosgrid.OrderBy(a => a.F_Id).Skip(((page - 1) * pageSize)).Take(pageSize).ToList();
            
            ViewBag.TotalRows = totalRecords;
            ViewBag.PageSize = pageSize;

            return PartialView(datosGridPagina.ToList());
        }

        [HttpPost]
        public JsonResult BEInsertData(Services.Entities.TBL_CapturaInformacionBasica_GridSimuladorCuotasBE Registro)
        {
            String result = String.Empty;
            try
            {
                if (ModelState.IsValid)
                {

                    if (Registro.F_varEntidadBE == null)
                    {
                        result = "\n El campo Entidad es obligatorio";
                    }

                    if (Registro.F_varNumeroObligacionBe == null)
                    {
                        result = result + "\n El campo Número Obligación es obligatorio";
                    }

                    if (result == String.Empty)
                    {
                        _stpcForms.AddObject("TBL_CapturaInformacionBasica_GridSimuladorCuotasBE", Registro);
                        _stpcForms.SaveChanges();
                        result = "1";
                    }
                }
                else
                {
                    var error = ModelState.Select(x => x.Value.Errors)
                           .Where(y=>y.Count>0)
                           .ToList();

                    result = error.ToString();
                }
            }
            catch (Exception e)
            {
                result = "0";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BESaveData(Services.Entities.TBL_CapturaInformacionBasica_GridSimuladorCuotasBE Registro)
        {
            String result = String.Empty;

            try
            {
                if (ModelState.IsValid)
                {

                    if (Registro.F_varEntidadBE == null)
                    {
                        result = "\n El campo Entidad es obligatorio";
                    }

                    if (Registro.F_varNumeroObligacionBe == null)
                    {
                        result = result + "\n El campo Número Obligación es obligatorio";
                    }

                    if (result == String.Empty)
                    {

                        TBL_CapturaInformacionBasica_GridSimuladorCuotasBE _row = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasBE.Where(e => e.F_Id == Registro.F_Id).FirstOrDefault();

                        _row.F_monValorCuotaBe = Registro.F_monValorCuotaBe;
                        _row.F_monValorObligacionBe = Registro.F_monValorObligacionBe;
                        _row.F_varEntidadBE = Registro.F_varEntidadBE;
                        _row.F_varNumeroObligacionBe = Registro.F_varNumeroObligacionBe;


                        _stpcForms.UpdateObject(_row);
                        // guardar cambios
                        _stpcForms.SaveChanges();
                        result = "1";
                    }
                }
            }
            catch (Exception e)
            {
                result = "0";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BEDeleteData(Services.Entities.TBL_CapturaInformacionBasica_GridSimuladorCuotasBE Registro)
        {
            String result = String.Empty;

            try
            {
                TBL_CapturaInformacionBasica_GridSimuladorCuotasBE _row = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasBE.Where(e => e.F_Id == Registro.F_Id).FirstOrDefault();

                if (_row != null)
                {
                    _row.F_bitEliminado = true;
                    _stpcForms.UpdateObject(_row);
                    // guardar cambios
                    _stpcForms.SaveChanges();
                    result = "1";
                }
                else
                {
                    result = "No se encontro el registro que desea eliminar, por favo verifique.";
                }
            }
            catch
            {
                result = "0";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BEUpdateSelect(Services.Entities.TBL_CapturaInformacionBasica_GridSimuladorCuotasBE Registro)
        {
            String result = String.Empty;

            try
            {
                if (ModelState.IsValid)
                {

                    if (result == String.Empty)
                    {

                        TBL_CapturaInformacionBasica_GridSimuladorCuotasBE _row = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasBE.Where(e => e.F_Id == Registro.F_Id).FirstOrDefault();

                        _row.F_bitSeleccionado = Registro.F_bitSeleccionado;


                        _stpcForms.UpdateObject(_row);
                        // guardar cambios
                        _stpcForms.SaveChanges();

                        var datosgrid = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasBE.Where(p => p.F_RequestId == Registro.F_RequestId && p.F_bitEliminado == false && p.F_bitSeleccionado == true);
                        long total = 0;
                        foreach (TBL_CapturaInformacionBasica_GridSimuladorCuotasBE row in datosgrid)
                        {
                            total = total + row.F_monValorCuotaBe;
                        }
                        NumberFormatInfo nfi = new CultureInfo("es-CO", false).NumberFormat;

                        result = "$ " + total.ToString("C", nfi).Replace("$", "").Replace(",00", "");
                    }
                }
            }
            catch (Exception e)
            {
                result = "-1";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        
        #endregion


        #region Simulador FNC
        public ActionResult FNCIndex(int requestId, string EditRol, int page = 1)
        {
            int pageSize = 10;
            int totalpages = 0;
            int totalRecords = 0;

            ViewBag.RequestId = requestId;

            //si disabled=true, entonces debe colocar block
            if (EditRol == "true" || ViewBag.EditRoles == "block")
            {
                ViewBag.EditRoles = "none";
            }
            else
            {
                ViewBag.EditRoles = "block";
            }

            var datosgrid = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC.Where(p => p.F_RequestId == requestId && p.F_bitEliminado == false);

            totalRecords = datosgrid.Count();
            totalpages = (totalRecords / pageSize) + ((totalRecords % pageSize) > 0 ? 1 : 0);

            var datosGridPagina = datosgrid.OrderBy(a => a.F_Id).Skip(((page - 1) * pageSize)).Take(pageSize).ToList();

            ViewBag.TotalRows = totalRecords;
            ViewBag.PageSize = pageSize;

            return PartialView(datosGridPagina.ToList());

        }


        [HttpPost]
        public JsonResult FNCInsertData(Services.Entities.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC Registro)
        {
            String result = String.Empty;
            try
            {
                if (ModelState.IsValid)
                {

                    if (Registro.F_varEntidadFnc == null)
                    {
                        result = "\n El campo Entidad es obligatorio";
                    }

                    if (Registro.F_varNumeroObligacionFnc == null)
                    {
                        result = result + "\n El campo Número Obligación es obligatorio";
                    }

                    if (result == String.Empty)
                    {
                        _stpcForms.AddObject("TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC", Registro);
                        _stpcForms.SaveChanges();
                        result = "1";
                    }
                }
                else
                {
                    var error = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();

                    result = error.ToString();
                }
            }
            catch (Exception e)
            {
                result = "0";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult FNCSaveData(Services.Entities.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC Registro)
        {
            String result = String.Empty;

            try
            {
                if (ModelState.IsValid)
                {

                    if (Registro.F_varEntidadFnc == null)
                    {
                        result = "\n El campo Entidad es obligatorio";
                    }

                    if (Registro.F_varNumeroObligacionFnc == null)
                    {
                        result = result + "\n El campo Número Obligación es obligatorio";
                    }

                    if (result == String.Empty)
                    {

                        TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC _row = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC.Where(e => e.F_Id == Registro.F_Id).FirstOrDefault();

                        _row.F_monValorCuotaFnc = Registro.F_monValorCuotaFnc;
                        _row.F_monValorObligacionFnc = Registro.F_monValorObligacionFnc;
                        _row.F_varEntidadFnc = Registro.F_varEntidadFnc;
                        _row.F_varNumeroObligacionFnc = Registro.F_varNumeroObligacionFnc;


                        _stpcForms.UpdateObject(_row);
                        // guardar cambios
                        _stpcForms.SaveChanges();
                        result = "1";
                    }
                }
            }
            catch (Exception e)
            {
                result = "0";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult FNCDeleteData(Services.Entities.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC Registro)
        {
            String result = String.Empty;

            try
            {
                TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC _row = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC.Where(e => e.F_Id == Registro.F_Id).FirstOrDefault();

                if (_row != null)
                {
                    _row.F_bitEliminado = true;
                    _stpcForms.UpdateObject(_row);
                    // guardar cambios
                    _stpcForms.SaveChanges();
                    result = "1";
                }
                else
                {
                    result = "No se encontro el registro que desea eliminar, por favo verifique.";
                }
            }
            catch
            {
                result = "0";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult FNCUpdateSelect(Services.Entities.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC Registro)
        {
            String result = String.Empty;

            try
            {
                if (ModelState.IsValid)
                {

                    if (result == String.Empty)
                    {

                        TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC _row = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC.Where(e => e.F_Id == Registro.F_Id).FirstOrDefault();

                        _row.F_bitSeleccionado = Registro.F_bitSeleccionado;


                        _stpcForms.UpdateObject(_row);
                        // guardar cambios
                        _stpcForms.SaveChanges();

                        var datosgrid = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC.Where(p => p.F_RequestId == Registro.F_RequestId && p.F_bitEliminado == false && p.F_bitSeleccionado == true);
                        long total =0;
                        foreach (TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC row in datosgrid)
                        {
                            total = total + row.F_monValorCuotaFnc;
                        }
                        NumberFormatInfo nfi = new CultureInfo("es-CO", false).NumberFormat;

                        result = "$ " + total.ToString("C", nfi).Replace("$", "").Replace(",00", "");
                    }
                }
            }
            catch (Exception e)
            {
                result = "-1";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Desembolso
        public ActionResult DesembolsoIndex(int requestId, string EditRol, int page = 1)
        {
            int pageSize = 10;
            int totalpages = 0;
            int totalRecords = 0;

            ViewBag.RequestId = requestId;

            if (EditRol == "true" || ViewBag.EditRoles == "block")
            {
                ViewBag.EditRoles = "none";
            }
            else
            {
                ViewBag.EditRoles = "block";
            }

            var datosgrid = _stpcForms.TBL_Desembolso_FINCOMERCIOGrid.Where(p => p.F_RequestId == requestId && p.F_bitEliminado == false);

            totalRecords = datosgrid.Count();
            totalpages = (totalRecords / pageSize) + ((totalRecords % pageSize) > 0 ? 1 : 0);

            var datosGridPagina = datosgrid.OrderBy(a => a.F_Id).Skip(((page - 1) * pageSize)).Take(pageSize).ToList();

            ViewBag.TotalRows = totalRecords;
            ViewBag.PageSize = pageSize;

            List<Option> TheOptions = new List<Option>();

			TheOptions = _stpcForms.Options.Where(cn => cn.Category_Uid == 28).ToList();

			var modelData = TheOptions.Select(m => new SelectListItem()
			{
				Text = m.Value,
                Value = m.Uid.ToString(),
                //m.Uid.ToString(),

			});

            ViewBag.listBanks = modelData;

            List<Option> TheOptions1 = new List<Option>();

            TheOptions1 = _stpcForms.Options.Where(cn => cn.Category_Uid == 27).ToList();

            var modelData1 = TheOptions1.Select(m => new SelectListItem()
            {
                Text = m.Value,
                Value = m.Uid.ToString(),
                //m.Uid.ToString(),

            });

            ViewBag.listAccountType = modelData1;

            return PartialView(datosGridPagina.ToList());
        }

        [HttpPost]
        public JsonResult DesembolsoInsertData(Services.Entities.TBL_Desembolso_FINCOMERCIOGrid Registro)
        {
            String result = String.Empty;
            string permisos = ViewBag.EditRoles;
            try
            {
                if (ModelState.IsValid)
                {

                    if (Registro.F_varBanco == null)
                    {
                        result = "\n El campo Banco es obligatorio";
                    }

                    if (Registro.F_varNumeroCuenta == null)
                    {
                        result = result + "\n El campo Número Cuenta es obligatorio";
                    }

                    if (Registro.F_varTipoCuenta == null)
                    {
                        result = result + "\n El campo Tipo Cuenta es obligatorio";
                    }

                    if (result == String.Empty)
                    {
                        _stpcForms.AddObject("TBL_Desembolso_FINCOMERCIOGrid", Registro);
                        _stpcForms.SaveChanges();
                        result = "1";
                    }
                }
                else
                {
                    var error = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();

                    result = error.ToString();
                }
            }
            catch (Exception e)
            {
                result = "0";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DesembolsoSaveData(Services.Entities.TBL_Desembolso_FINCOMERCIOGrid Registro)
        {
            String result = String.Empty;

            try
            {
                if (ModelState.IsValid)
                {

                    if (Registro.F_varBanco == null)
                    {
                        result = "\n El campo Banco es obligatorio";
                    }

                    if (Registro.F_varNumeroCuenta == null)
                    {
                        result = result + "\n El campo Número Cuenta es obligatorio";
                    }

                    if (Registro.F_varTipoCuenta == null)
                    {
                        result = result + "\n El campo Tipo Cuenta es obligatorio";
                    }

                    if (result == String.Empty)
                    {

                        TBL_Desembolso_FINCOMERCIOGrid _row = _stpcForms.TBL_Desembolso_FINCOMERCIOGrid.Where(e => e.F_Id == Registro.F_Id).FirstOrDefault();

                        _row.F_varBanco = Registro.F_varBanco;
                        _row.F_varNumeroCuenta = Registro.F_varNumeroCuenta;
                        _row.F_varTipoDesembolso = Registro.F_varTipoDesembolso;

                        _stpcForms.UpdateObject(_row);
                        // guardar cambios
                        _stpcForms.SaveChanges();
                        result = "1";
                    }
                }
            }
            catch (Exception e)
            {
                result = "0";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DesembolsoDeleteData(Services.Entities.TBL_Desembolso_FINCOMERCIOGrid Registro)
        {
            String result = String.Empty;

            try
            {
                TBL_Desembolso_FINCOMERCIOGrid _row = _stpcForms.TBL_Desembolso_FINCOMERCIOGrid.Where(e => e.F_Id == Registro.F_Id).FirstOrDefault();

                if (_row != null)
                {
                    _row.F_bitEliminado = true;
                    _stpcForms.UpdateObject(_row);
                    // guardar cambios
                    _stpcForms.SaveChanges();
                    result = "1";
                }
                else
                {
                    result = "No se encontro el registro que desea eliminar, por favo verifique.";
                }
            }
            catch
            {
                result = "0";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DesembolsoUpdateSelect(Services.Entities.TBL_Desembolso_FINCOMERCIOGrid Registro)
        {
            String result = String.Empty;

            try
            {
                if (ModelState.IsValid)
                {

                    if (result == String.Empty)
                    {
                        //Actualiza todos en 0

                        //TBL_Desembolso_FINCOMERCIOGrid _rows = _stpcForms.TBL_Desembolso_FINCOMERCIOGrid.Where(e => e.F_RequestId == Registro.F_RequestId);
                        //_rows.F_bitSeleccionado = Registro.F_bitSeleccionado;
                        //_stpcForms.UpdateObject(_row);


                        TBL_Desembolso_FINCOMERCIOGrid _row = _stpcForms.TBL_Desembolso_FINCOMERCIOGrid.Where(e => e.F_Id == Registro.F_Id).FirstOrDefault();
                        _row.F_bitSeleccionado = Registro.F_bitSeleccionado;
                        _stpcForms.UpdateObject(_row);
                        // guardar cambios
                        _stpcForms.SaveChanges();

                        var datosgrid = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC.Where(p => p.F_RequestId == Registro.F_RequestId && p.F_bitEliminado == false && p.F_bitSeleccionado == true);
                        long total = 0;
                        foreach (TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC row in datosgrid)
                        {
                            total = total + row.F_monValorCuotaFnc;
                        }
                        NumberFormatInfo nfi = new CultureInfo("es-CO", false).NumberFormat;

                        result = "$ " + total.ToString("C", nfi).Replace("$", "").Replace(",00", "");
                    }
                }
            }
            catch (Exception e)
            {
                result = "-1";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Simulador FNC - Movil
        public ActionResult MovilFNCIndex(int requestId, string EditRol, int page = 1)
        {
            int pageSize = 10;
            int totalpages = 0;
            int totalRecords = 0;

            ViewBag.RequestId = requestId;

            //si disabled=true, entonces debe colocar block
            if (EditRol == "true" || ViewBag.EditRoles == "block")
            {
                ViewBag.EditRoles = "none";
            }
            else
            {
                ViewBag.EditRoles = "block";
            }

            var datosgrid = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC.Where(p => p.F_RequestId == requestId && p.F_bitEliminado == false);

            totalRecords = datosgrid.Count();
            totalpages = (totalRecords / pageSize) + ((totalRecords % pageSize) > 0 ? 1 : 0);

            var datosGridPagina = datosgrid.OrderBy(a => a.F_Id).Skip(((page - 1) * pageSize)).Take(pageSize).ToList();

            ViewBag.TotalRows = totalRecords;
            ViewBag.PageSize = pageSize;

            return PartialView(datosGridPagina.ToList());

        }

        [HttpPost]
        public JsonResult MovilFNCUpdateSelect(Services.Entities.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC Registro)
        {
            String result = String.Empty;

            try
            {
                if (ModelState.IsValid)
                {

                    if (result == String.Empty)
                    {

                        TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC _row = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC.Where(e => e.F_Id == Registro.F_Id).FirstOrDefault();

                        _row.F_bitSeleccionado = Registro.F_bitSeleccionado;


                        _stpcForms.UpdateObject(_row);
                        // guardar cambios
                        _stpcForms.SaveChanges();

                        var datosgrid = _stpcForms.TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC.Where(p => p.F_RequestId == Registro.F_RequestId && p.F_bitEliminado == false && p.F_bitSeleccionado == true);
                        long total = 0;
                        foreach (TBL_CapturaInformacionBasica_GridSimuladorCuotasFNC row in datosgrid)
                        {
                            total = total + row.F_monValorCuotaFnc;
                        }
                        NumberFormatInfo nfi = new CultureInfo("es-CO", false).NumberFormat;

                        result = "$ " + total.ToString("C", nfi).Replace("$", "").Replace(",00", "");
                    }
                }
            }
            catch (Exception e)
            {
                result = "-1";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Grafologia
        public ActionResult GrafologiaIndex(int requestId, string EditRol, int page = 1)
        {
            int pageSize = 10;
            int totalpages = 0;
            int totalRecords = 0;

            ViewBag.RequestId = requestId;

            if (EditRol == "true" || ViewBag.EditRoles == "block")
            {
                ViewBag.EditRoles = "none";
            }
            else
            {
                ViewBag.EditRoles = "block";
            }

            var datosgrid = _stpcForms.TBL_CapturaInformacionBasicaAnalista_GridGrafologia.Where(p => p.F_RequestId == requestId && p.F_bitEliminado == false);

            totalRecords = datosgrid.Count();
            totalpages = (totalRecords / pageSize) + ((totalRecords % pageSize) > 0 ? 1 : 0);

            var datosGridPagina = datosgrid.OrderBy(a => a.F_Id).Skip(((page - 1) * pageSize)).Take(pageSize).ToList();

            ViewBag.TotalRows = totalRecords;
            ViewBag.PageSize = pageSize;

            List<Option> TheOptions = new List<Option>();

            TheOptions = _stpcForms.Options.Where(cn => cn.Category_Uid == 23).ToList();

            var modelData = TheOptions.Select(m => new SelectListItem()
            {
                Text = m.Value,
                Value = m.Value,
                //m.Uid.ToString(),

            });

            ViewBag.listResult = modelData;

            return PartialView(datosGridPagina.ToList());
        }

        [HttpPost]
        public JsonResult GrafologiaInsertData(Services.Entities.TBL_CapturaInformacionBasicaAnalista_GridGrafologia Registro)
        {
            String result = String.Empty;
            string permisos = ViewBag.EditRoles;
            try
            {
                if (ModelState.IsValid)
                {

                    if (Registro.F_varSoporte == null)
                    {
                        result = "\n El campo Soporte es obligatorio";
                    }

                    if (Registro.F_varResultado == null)
                    {
                        result = result + "\n El campo Resultado es obligatorio";
                    }

                    //if (Registro.F_varTipoCuenta == null)
                    //{
                    //    result = result + "\n El campo Tipo Cuenta es obligatorio";
                    //}

                    if (result == String.Empty)
                    {
                        _stpcForms.AddObject("TBL_CapturaInformacionBasicaAnalista_GridGrafologia", Registro);
                        _stpcForms.SaveChanges();
                        result = "1";
                    }
                }
                else
                {
                    var error = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();

                    result = error.ToString();
                }
            }
            catch (Exception e)
            {
                result = "0";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GrafologiaSaveData(Services.Entities.TBL_CapturaInformacionBasicaAnalista_GridGrafologia Registro)
        {
            String result = String.Empty;

            try
            {
                if (ModelState.IsValid)
                {

                    if (Registro.F_varSoporte == null)
                    {
                        result = "\n El campo Soporte es obligatorio";
                    }

                    if (Registro.F_varResultado == null)
                    {
                        result = result + "\n El campo Resultado es obligatorio";
                    }

                    //if (Registro.F_varTipoCuenta == null)
                    //{
                    //    result = result + "\n El campo Tipo Cuenta es obligatorio";
                    //}

                    if (result == String.Empty)
                    {

                        TBL_CapturaInformacionBasicaAnalista_GridGrafologia _row = _stpcForms.TBL_CapturaInformacionBasicaAnalista_GridGrafologia.Where(e => e.F_Id == Registro.F_Id).FirstOrDefault();

                        _row.F_varResultado = Registro.F_varResultado;
                        _row.F_varObservacion = Registro.F_varObservacion;
                        _row.F_varSoporte = Registro.F_varSoporte;

                        _stpcForms.UpdateObject(_row);
                        // guardar cambios
                        _stpcForms.SaveChanges();
                        result = "1";
                    }
                }
            }
            catch (Exception e)
            {
                result = "0";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GrafologiaDeleteData(Services.Entities.TBL_CapturaInformacionBasicaAnalista_GridGrafologia Registro)
        {
            String result = String.Empty;

            try
            {
                TBL_CapturaInformacionBasicaAnalista_GridGrafologia _row = _stpcForms.TBL_CapturaInformacionBasicaAnalista_GridGrafologia.Where(e => e.F_Id == Registro.F_Id).FirstOrDefault();

                if (_row != null)
                {
                    //_stpcForms.DeleteObject(_row);
                    // guardar cambios
                    _row.F_bitEliminado = true;

                    _stpcForms.UpdateObject(_row);

                    _stpcForms.SaveChanges();
                    result = "1";
                }
                else
                {
                    result = "No se encontro el registro que desea eliminar, por favo verifique.";
                }
            }
            catch (Exception ex)
            {
                result = "0"; // +ex.Message + "-" + ex.InnerException;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
    
    
    }
}