using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnotationProject.Models {
    public class UserAdminModel {
        public string Username { get; set; }
        public int ID { get; set; }
        public List<string> Roles { get; set; }
        public bool IsLockedOut { get; set; }
        public string RoleToAdd { get; set; }
    }
}