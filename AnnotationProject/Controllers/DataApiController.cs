using AnnotationProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;

namespace AnnotationProject.Controllers
{
    public class DataApiController : ApiController
    {
        [HttpGet]
        public List<TextResult> GetText(string query) {
            var db = new TextAnnotationEntities();
            var toReturn = db.Texts.Where(i => i.Title.ToLower().Contains(query.ToLower()));
            return toReturn.Select(i => new TextResult() { 
                Content = i.Content, 
                Title = i.Title, 
                Author = i.Author,
                Description = i.Description,
                ID = i.ID
            }).ToList();
        }

        [HttpGet]
        public List<TextResult> GetAll() {
            var db = new TextAnnotationEntities();
            return db.Texts.Where(i => i.IsBaseText).Take(50).Select(i => new TextResult() {
                Content = i.Content,
                Title = i.Title,
                Author = i.Author,
                Description = i.Description,
                ID = i.ID
            }).ToList();
        }

        [HttpGet]
        public TextResult GetText(int id) {
            var db = new TextAnnotationEntities();
            var toReturn = db.Texts.Where(i => i.ID == id);
            return toReturn.Select(i => new TextResult() {
                Content = i.Content,
                Title = i.Title,
                Author = i.Author,
                Description = i.Description,
                ID = i.ID,
            }).Single();
        }

        [HttpGet]
        public List<AnnotationResult> RecentAnnotations() {
            var db = new TextAnnotationEntities();
            
            var annotationTexts = db.Texts.Where(i => !i.IsBaseText &&

                (!i.Archived.HasValue || !i.Archived.Value))
                .OrderByDescending(i => i.Timestamp).Take(10)
                .ToList();
            var annotationIDs = annotationTexts.Select( i=> i.ID);
            
            var annotations = db.Annotations.Where(i => annotationIDs.Contains(i.AnnotationTextID)
                );
            return annotations.Select(i => new AnnotationResult() {
                Content = i.Text1.Content,
                Timestamp = i.Text1.Timestamp,
                BaseTextID = i.BaseTextID,
                TextAnchor = i.TextAnchor,
                BaseTextTitle = i.Text.Title
            }).OrderByDescending(i => i.Timestamp).ToList();
        }

        [HttpGet]
        public List<AnnotationResult> GetAnnotations(int textID) {
            var db = new TextAnnotationEntities();
            var annotationIds = db.Annotations.Where(i => i.BaseTextID == textID)
                .Select(i => new { i.AnnotationTextID, i.TextAnchor });
            
            var toReturn = db.Texts.Where(i => annotationIds.Select(j => j.AnnotationTextID).Contains(i.ID) && 
                (!i.Archived.HasValue || !i.Archived.Value));
            if (annotationIds.Count() == 0) {
                return new List<AnnotationResult>();
            }
            return toReturn.Select(i => new AnnotationResult() {
                Content = i.Content,
                Timestamp = i.Timestamp,
                BaseTextID = textID,
                TextAnchor = annotationIds.Where(j => j.AnnotationTextID == i.ID).FirstOrDefault().TextAnchor,
                TextID = i.ID,
                Username = i.Username
            }).ToList();
        }

        [HttpPost]
        public List<AnnotationResult> PostAnnotation(AnnotationResult annotation) {
            var db = new TextAnnotationEntities();
            var newText = new Text() {
                Content = annotation.Content,
                Timestamp = DateTime.Now,
                IsBaseText = false,
                Username = annotation.Username
            };
            db.Texts.Add(newText);
            db.SaveChanges();
            db.Annotations.Add(new Annotation() {
                AnnotationTextID = newText.ID,
                BaseTextID = annotation.BaseTextID,
                TextAnchor = annotation.TextAnchor
            });
            db.SaveChanges();
            return GetAnnotations(annotation.BaseTextID);
        }

        [HttpPost]
        public void PostText(TextResult text) {
            var db = new TextAnnotationEntities();
            db.Texts.Add(new Text() {
                Title = text.Title,
                Content = text.Content,
                Timestamp = DateTime.Now,
                Author = text.Author,
                Description = text.Description,
                IsBaseText = true,
            });
            //UserID = (Guid)Membership.GetUser().ProviderUserKey,
            db.SaveChanges();
        }

        [HttpGet]
        public List<UserAdminModel> GetUsers() {
            var db = new UsersContext();
            List<UserAdminModel> users = new List<UserAdminModel>();
            foreach (var u in db.UserProfiles) {
                users.Add(new UserAdminModel() {
                    ID = u.UserId,
                    Username = u.UserName,
                    Roles = Roles.GetRolesForUser(u.UserName).ToList(),
                    IsLockedOut = false
                });
            }
            return users;
        }

        [HttpGet]
        public string MembershipTest() {
            try {
                var allUsers = Membership.GetAllUsers();
                //var db = new UsersContext();
                //List<UserAdminModel> users = new List<UserAdminModel>();
                //foreach (var u in db.UserProfiles) {
                //    users.Add(new UserAdminModel() {
                //        ID = u.UserId,
                //        Username = u.UserName,
                //        Roles = Roles.GetRolesForUser(u.UserName).ToList(),
                //        IsLockedOut = false
                //    });
                //}
                return "success " + allUsers.Count;
            } catch (Exception ex) {
                return string.Format("Failed. Ex: {0}, inner: {1}", ex.Message, ex.InnerException);
            }
        }

        [HttpPost]
        public List<UserAdminModel> AddRole(string user, string role) {
            if (!Roles.RoleExists(role)) {
                Roles.CreateRole(role);
            }
            Roles.AddUserToRole(user, role);
            return GetUsers();
        }


        [HttpPost]
        public List<UserAdminModel> RemoveRole(string user, string role) {
            if (!Roles.RoleExists(role)) {
                return GetUsers();
            }
            Roles.RemoveUserFromRole(user, role);
            return GetUsers();
        }

        [HttpPost]
        public List<AnnotationResult> ArchiveAnnotation(int annotationID, int textID) {
            var db = new TextAnnotationEntities();
            db.Texts.Where(i => i.ID == annotationID).Single().Archived = true;
            db.SaveChanges();
            return GetAnnotations(textID);
        }

        [HttpPost]
        public List<AnnotationResult> UpdateAnnotation(AnnotationResult ann) {
            var db = new TextAnnotationEntities();
            var toEdit = db.Texts.Where(i => i.ID == ann.TextID).Single();
            toEdit.Content = ann.Content;
            db.SaveChanges();
            return GetAnnotations(ann.BaseTextID);
        }
    }
}
