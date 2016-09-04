using System.Web;
using System.Web.Optimization;

namespace VitEgoDictionary
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Scripts/owl.carousel*",
                "~/Scripts/dictionary-master-layout.js",
                "~/Scripts/dictionary-dialogs.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap*",
                "~/Scripts/html5shiv*",
                "~/Scripts/respond*",
                "~/Scripts/typeahead*",
                "~/Scripts/modernizr-*"));

              bundles.Add(new StyleBundle("~/Content/css").Include(          
                "~/Content/Style/bootstrap-select.css",
                "~/Content/Style/bootstrap.css",
                "~/Content/Style/font-awesome.css",
                "~/Content/Style/owl.carousel.css",
                "~/Content/Style/owl.theme.css",
                "~/Content/Style/owl.transitions.css",
                "~/Content/Style/dictionary-master-layout.css",
                "~/Content/Style/dictionary-search-panel.css",
                "~/Content/Style/dictionary-shared.css",
                "~/Content/Style/typeahead.css"));
        }
    }
}