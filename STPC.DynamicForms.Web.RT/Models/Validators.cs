using System.ComponentModel.DataAnnotations;

namespace STPC.DynamicForms.Web.RT.Models
{
    //These DO NOT work with Html.EnableClientValidation(); -- they only work server-side as-is (postback, ugh)
    //TODO: ScottGu's article referenced that you could do client-side with some additional set-up... hmm...
    //If it did work, this would enable it in a partial Metadata class for model validation: [Email(ErrorMessage = "Bad email...")]
    public class EmailAttribute : RegularExpressionAttribute
    {
        public EmailAttribute()
            : base(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}$") {}        
    }

    public class LinkAttribute : RegularExpressionAttribute
    {
        public LinkAttribute()
            : base(@"^(http|https|ftp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$") {}
    }
}