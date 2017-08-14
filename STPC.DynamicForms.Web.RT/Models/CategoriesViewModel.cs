using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using STPC.DynamicForms.Web.RT.Services.Entities;


namespace STPC.DynamicForms.Web.RT.Models
{
    public class CategoriesViewModel
    {
        public string Current { get; set; }
        public List<Category> Categories { get; set; }
    }
}