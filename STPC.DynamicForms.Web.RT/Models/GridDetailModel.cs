using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using STPC.DynamicForms.Web.RT.Services.Entities;

namespace STPC.DynamicForms.Web.RT.Models
{
    public class GridDetailModel
    {
        public string F_Id { get; set; } //Consecutivo
        public string F_RequestId { get; set; } //Numero de solicitud
        public Dictionary<String, String> DetalleDatos { get; set; } //Campos dinamicos
        public Guid UID { get; set; } //UID de la solicitud
        public string tableName { get; set; } //Nombre de la tabla

    }

}