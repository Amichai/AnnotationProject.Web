using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AnnotationProject.Controllers
{
    public class TextViewController : Controller
    {
        //
        // GET: /TextView/



        public ActionResult Index(int textID) {
            return View();
        }
        ///TODO: start supporting annotation tags and filtering
    }
}
