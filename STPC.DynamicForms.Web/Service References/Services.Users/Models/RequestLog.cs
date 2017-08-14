using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace STPC.DynamicForms.Web.Models
{
    [MetadataType(typeof(RequestLogMetaData))]
    public partial class RequestLog
    {
        [Bind(Exclude = "Uid")]
        public class RequestLogMetaData
        {
            [ScaffoldColumn(false)]
            public object Uid { get; set; }

            [ScaffoldColumn(false)]
            public object AutoResponderUid { get; set; }

            [ScaffoldColumn(false)]
            public object Timestamp { get; set; }

            [DisplayName("Your Name")]
            [DataType(DataType.Text)]
            [Required(ErrorMessage = "Please enter your name.")]
            [StringLength(100, ErrorMessage = "Please ensure that your name is less than 100 characters.")]
            public object Name { get; set; }

            [DisplayName("Your Email Address")]
            [DataType(DataType.EmailAddress)]
            [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}$", ErrorMessage = "Please enter a valid email address.")]
            [Required(ErrorMessage = "Please enter your email address.")]
            [StringLength(100, ErrorMessage = "Please ensure that your email address is less than 100 characters.")]
            public object Email { get; set; }
        }
    }
}