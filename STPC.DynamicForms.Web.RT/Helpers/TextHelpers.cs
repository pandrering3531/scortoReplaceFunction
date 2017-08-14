using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using STPC.DynamicForms.Web.Common;

namespace STPC.DynamicForms.Web.RT.Helpers
{
    public static class TextHelpers
    {
        public static string KillHtml(this string text)
        {
            return string.IsNullOrEmpty(text) ? null : Regex.Replace(text, @"<(.|\n)*?>\n", string.Empty);
        }

        public static string PreserveBreaks(this string text)
        {
            return string.IsNullOrEmpty(text) ? null : text.Replace(Environment.NewLine, @"\\br\\");
        }

        public static string RestoreBreaks(this string text)
        {
            return string.IsNullOrEmpty(text) ? null : text.Replace(@"\\br\\", Environment.NewLine);
        }
    }

    public static class HtmlHelpers {

        [OutputCache(Duration = 600)]
        public static string GetUsername(this HtmlHelper helper, string userId)
        { 
            CustomMembershipProvider provider = (CustomMembershipProvider)System.Web.Security.Membership.Provider;
            var user=provider.GetUser(userId);
            if(user==null) return string.Empty; //TODO: Hacer log del error
            return user.GivenName + ' ' + user.LastName;
        }

        [OutputCache(Duration=600)]
        public static string GetLastLogin(this HtmlHelper helper, string userId)
        {
            CustomMembershipProvider provider = (CustomMembershipProvider)System.Web.Security.Membership.Provider;
            var user = provider.GetUser(userId);
            if (user == null) return string.Empty; //TODO: Hacer log del error
            return user.LastLoginDate.ToShortDateString();
        }
    }
}