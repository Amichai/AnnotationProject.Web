using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AnnotationProject.Controllers
{
    [Authorize(Roles="IsAdmin")]
    public class ArchivedTextsController : Controller
    {
        //
        // GET: /ArchivedTexts/

        public ActionResult Index()
        {
            return View();
        }

    }
}
