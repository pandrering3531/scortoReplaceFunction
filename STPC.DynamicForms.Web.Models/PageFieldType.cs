using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;


namespace STPC.DynamicForms.Web.Models
{

	[KnownType(typeof(PageField))]
    public class PageFieldType
    {
        [Key]
        public Guid Uid { get; set; }
        [Required]
        [MaxLength(30)]
        public string FieldTypeName { get; set; }
        public int SortOrder { get; set; }
        [Required]
        [MaxLength(30)]
        public string FieldType { get; set; }
        [Required]
        [MaxLength(30)]
        public string ControlType { get; set; }
        [MaxLength(300)]
        public string ErrorMsgRequired { get; set; }
        [MaxLength(300)]
        public string RegExDefault { get; set; }
        [MaxLength(300)]
        public string ErrorMsgRegEx { get; set; }
        [MaxLength(300)]
        public string ValidExtensions { get; set; }
        [MaxLength(300)]
        public string ErrorExtensions { get; set; }
        public DateTime Timestamp { get; set; }
        public virtual ICollection<PageField> Fields { get; set; }
    }
}
