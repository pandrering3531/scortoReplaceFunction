using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace STPC.DynamicForms.Web.RT.Models
{
    public class MyViewModels
    {
        [Required]
        public HttpPostedFileBase MyExcelFile { get; set; }

        public string MSExcelTable { get; set; }
    }

    public class MyViewModelsSinVal
    {
        public HttpPostedFileBase MyExcelFile { get; set; }

        public string MSExcelTable { get; set; }
    }
}