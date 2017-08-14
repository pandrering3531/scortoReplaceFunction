using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace STPC.DynamicForms.Web.Models
{
    public class FormPageResponse
    {
        [Key]
        public Guid Uid { get; set; }

        public FormPage FormPage { get; set; }
                       
        public virtual FormResponse FormResponse { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
