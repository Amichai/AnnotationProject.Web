using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AnnotationProject.Controllers
{
    [Authorize]
    public class ResponsesController : Controller
    {
        //
        // GET: /Responses/

        public ActionResult Index()
        {
            return View();
        }

    }
}
