using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.Core
{
    public static class RegexPatterns
    {
      
        /// <summary>
        /// Matches a well-formed HTML element's id attribute value. 
        /// Naming rules: Must begin with a letter A-Z or a-z.  
        /// Can be followed by: letters (A-Za-z), digits (0-9), hyphens ("-"), and underscores ("_").
        /// </summary>
        public static string HtmlId
        {
            get
            {
                return Constants.RP_HTMLID;
            }
        }
        /// <summary>
        /// Matches a well-formed HTML input element's name attribute value. 
        /// Naming rules: Must begin with a letter A-Z or a-z.  
        /// Can be followed by: letters (A-Za-z), digits (0-9), hyphens ("-"), and underscores ("_"), colons (":"), and periods (".").
        /// </summary>
        public static string HtmlInputName
        {
            get
            {
                return Constants.RP_HTMLINPUTNAME;
            }
        }
        /// <summary>
        /// A practical regular expression used to validate email addresses.
        /// This will match the vast majority of email addresses in use.
        /// This is not RFC 2822 compliant.
        /// See http://www.regular-expressions.info/email.html for more information.
        /// </summary>
        public static string EmailAddress
        {
            get
            {
                return Constants.RP_EMAILADDRESS;
            }
        }
    }
}
