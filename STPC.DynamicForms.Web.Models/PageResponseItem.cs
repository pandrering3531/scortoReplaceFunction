using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace STPC.DynamicForms.Web.Models
{
    public class PageResponseItem
    {
        [Key]
        public Guid Uid{get;set;}

        public virtual FormPageResponse PageResponse { get; set; }

        public virtual PageField PageItem { get; set; }

        [MaxLength(300)]
        public string ResponseStr { get; set; }

        public DateTime Timestamp { get; set; }

    }
}
