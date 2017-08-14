using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using STPC.DynamicForms.Web.Services.Entities;


namespace STPC.DynamicForms.Web.Models
{
    public class CategoriesViewModel
    {
        public string Current { get; set; }
        public List<Category> Categories { get; set; }
    }
}