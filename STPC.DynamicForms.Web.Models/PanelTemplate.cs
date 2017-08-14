using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;

namespace STPC.DynamicForms.Web.Models
{
    public class PanelTemplate
    {
        [Key]
        public Guid Uid { get; set; }
        public int Columns { get; set; }

        public int SortOrder { get; set; }

        [ScaffoldColumn(false)]
        public DateTime Timestamp { get; set; }
    }
}
