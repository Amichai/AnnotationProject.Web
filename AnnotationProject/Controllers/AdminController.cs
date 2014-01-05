using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AnnotationProject.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/

        //[Authorize(Roles="IsAdmin")]
        public ActionResult Index()
        {
            return View();
        }

    }
}
