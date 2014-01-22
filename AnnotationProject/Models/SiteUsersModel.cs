using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnotationProject.Models {
    public class SiteUsersModel {
        public string Username { get; set; }
        public int Annotations { get; set; }
        public int Favorited { get; set; }
        public List<string> Texts { get; set; }
    }
}