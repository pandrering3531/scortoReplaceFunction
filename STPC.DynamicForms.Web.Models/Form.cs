using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace STPC.DynamicForms.Web.Models
{
    //[MetadataType(typeof(FormMetaData))]
    public class Form
    {

        [ScaffoldColumn(false)]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Uid { get; set; }

        [ScaffoldColumn(false)]
        public Guid UserId { get; set; }

        [ScaffoldColumn(false)]
        public DateTime Timestamp { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        public virtual ICollection<FormPage> Pages { get; set; }

		  public virtual ICollection<Report> Reports { get; set; }

        public Form()
        {
            this.Pages = new List<FormPage>();
            this.Timestamp = DateTime.Now;
        }

    }
}