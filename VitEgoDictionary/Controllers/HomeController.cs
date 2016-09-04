using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VitEgoDictionary.Models;
using VitEgoDictionary.Models.Parameters;
using VitEgoDictionary.Models.ViewModels;

namespace VitEgoDictionary.Controllers
{
    public class HomeController : DictionaryController
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View(new MasterLayoutViewModel(this.Request.IsAuthenticated, this.HttpContext.User, null));
        }

        [HttpGet]
        public ActionResult Construction()
        {
            return View(new MasterLayoutViewModel(this.Request.IsAuthenticated, this.HttpContext.User, null));
        }

        [HttpGet]
        public ActionResult About()
        {
            return View(new MasterLayoutViewModel(this.Request.IsAuthenticated, this.HttpContext.User, null));
        }
    }
}
