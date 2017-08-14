using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace STPC.DynamicForms.Web.Models
{
    public class Panel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Uid { get; set; }
        [Required]
        public int SortOrder { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        [Required]
        public virtual FormPage Page { get; set; }

        public virtual ICollection<PageField> PanelFields { get; set; }

        [MaxLength(128)]
        public string DivCssStyle { get; set; }

        [Required]
        public int Columns { get; set; }

        [ScaffoldColumn(false)]
        public DateTime Timestamp { get; set; }

        public string ViewRoles { get; set; }

        public string EditRoles { get; set; }

        /////////////////////////////////
        //Se adiciona el parametro Type para identificar si es un panel de detalle o es normal junto con el nombre de la tabla.
        //Por: Jorge Alonso
        //Fecha:2016-09-17
        /////////////////////////////////
        [MaxLength(10)]
        public string Type { get; set; }
        public string TableDetailName { get; set; }
        /////////////////////////////////

        public Panel()
        {
            this.PanelFields = new List<PageField>();
            this.Timestamp = DateTime.Now;
        }
    }
}