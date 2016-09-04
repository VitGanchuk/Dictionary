using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VitEgoDictionary.Models.Extensions
{
    public static class MvcHtmlStringExtension
    {
        public static MvcHtmlString IsDisabled(this MvcHtmlString htmlString, bool disabled)
        {
            string rawstring = htmlString.ToString();
            if (disabled)
            {
                rawstring = rawstring.Insert(rawstring.IndexOf('>'), " disabled='disabled'");
            }
            return new MvcHtmlString(rawstring);
        }
    }
}