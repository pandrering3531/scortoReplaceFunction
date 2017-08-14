using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace STPC.DynamicForms.Web.Models
{
    public class FormResponse
    {
        [Key]
        public Guid Uid { get; set; }

        public DateTime Timestamp { get; set; }

        public virtual Form FormSpec { get; set; }
    }
}
