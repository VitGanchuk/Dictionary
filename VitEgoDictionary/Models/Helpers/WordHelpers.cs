using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace VitEgoDictionary.Models.Helpers
{
    public static class WordHelpers
    {
        public static MvcHtmlString VariationFor(this HtmlHelper html, Variation variation)
        {
            String mvcHtmlString = String.Format("<span class='variation'>{0}</span> " +
                "<span class='grammar-structure'>({1}); </span>", variation.Name, variation.GrammarStructure.Name);
            return new MvcHtmlString(mvcHtmlString);
        }
    }
}