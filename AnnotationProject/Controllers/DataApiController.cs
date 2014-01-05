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
        public List<TextResult> GetText(string query, string tags) {

            var db = new TextAnnotationEntities();
            if (tags != null) {
                List<Tag> inspectionTags = new List<Tag>();
                foreach (var tag in tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                    inspectionTags.AddRange(db.Tags.Where(i => i.Tag1.ToLower().Contains(tag.ToLower().Trim())));
                }
                var tagQueryResult = inspectionTags.SelectMany(i => i.TextTags).Select(i => i.Text).Where(i => i.IsBaseText).ToList();

                if (query == null) {
                    return toTextResult(tagQueryResult);
                } else {
                    List<int> tagTextIds = tagQueryResult.Select(i => i.ID).ToList();
                    var queryResults = db.Texts.Where(i => i.Title.ToLower().Contains(query.ToLower()));
                    queryResults = queryResults.Where(i => tagTextIds.Contains(i.ID));
                    var result = toTextResult(queryResults.ToList());
                    return result;
                }
            } else {
                var queryResults = db.Texts.Where(i => i.Title.ToLower().Contains(query.ToLower()));
                return toTextResult(queryResults.ToList());
            }
        }

        private static List<TextResult> toTextResult(List<Text> queryResults) {
            var result = queryResults.Select(i => new TextResult() {
                Content = i.Content,
                Title = i.Title,
                Author = i.Author,
                Description = i.Description,
                ID = i.ID,
                Tags = string.Concat(i.TextTags.Select(j => j.Tag.Tag1 + ", "))
            }).ToList();
            return result;
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
            var toReturn = db.Texts.Where(i => i.ID == id).ToList();
            return toReturn.Select(i => new TextResult() {
                Content = i.Content,
                Title = i.Title,
                Author = i.Author,
                Description = i.Description,
                ID = i.ID,
                Uploader = i.Username,
                Tags =  string.Concat(i.TextTags.Select(j => j.Tag.Tag1 + ", "))
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

            List<AnnotationResult> results = new List<AnnotationResult>();
            foreach (var t in toReturn) {
                string tags = string.Concat(db.TextTags.Where(i => i.TextID == t.ID).Select(i => i.Tag.Tag1 + ", "));
                results.Add(new AnnotationResult() {
                    Content = t.Content,
                    Timestamp = t.Timestamp,
                    BaseTextID = textID,
                    TextAnchor = annotationIds.Where(j => j.AnnotationTextID == t.ID).FirstOrDefault().TextAnchor,
                    TextID = t.ID,
                    Username = t.Username,
                    Tags = tags
                });
            }
            
            return results;
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

            updateTags(annotation.Tags, annotation.TextID, db, newText);
            return GetAnnotations(annotation.BaseTextID);
        }

        private static void updateTags(string tagString, int annotationID, TextAnnotationEntities db, Text newText) {
            ///Delete any preexisting tags if necessary
            var toDelete = db.TextTags.Where(i => i.TextID == annotationID);
            foreach (var t in toDelete) {
                db.TextTags.Remove(t);
            }
            var tagList = tagString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tagList) {
                var tag = t.Trim().ToLower();
                Tag inspectionTag;
                var matches = db.Tags.Where(i => i.Tag1 == tag);
                if (matches.Count() == 0) {
                    inspectionTag = new Tag() {
                        Tag1 = tag,
                    };
                    db.Tags.Add(inspectionTag);
                    db.SaveChanges();
                } else {
                    inspectionTag = matches.Single();
                }
                db.TextTags.Add(new TextTag() {
                    TextID = newText.ID,
                    TagID = inspectionTag.ID
                });
            }
            db.SaveChanges();
        }

        [HttpPost]
        public void PostText(TextResult text) {
            var db = new TextAnnotationEntities();
            var newText = new Text() {
                Title = text.Title,
                Content = text.Content,
                Timestamp = DateTime.Now,
                Author = text.Author,
                Description = text.Description,
                IsBaseText = true,
                Username = User.Identity.Name
            };
            db.Texts.Add(newText);
            var tagList = text.Tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tagList) {
                var tag = t.Trim().ToLower();
                Tag inspectionTag;
                var matches = db.Tags.Where(i => i.Tag1 == tag);
                if (matches.Count() == 0) {
                    inspectionTag = new Tag() {
                        Tag1 = tag,
                    };
                    db.Tags.Add(inspectionTag);
                    db.SaveChanges();
                } else {
                    inspectionTag = matches.Single();
                }
                db.TextTags.Add(new TextTag() {
                    TextID = newText.ID,
                    TagID = inspectionTag.ID
                });
            }

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
            updateTags(ann.Tags, ann.TextID, db, toEdit);
            db.SaveChanges();
            return GetAnnotations(ann.BaseTextID);
        }

        [HttpPost]
        public void UpdateTextDetails(TextResult text) {
            var db = new TextAnnotationEntities();
            var toEdit = db.Texts.Where(i => i.ID == text.ID).Single();
            toEdit.Content = text.Content;
            toEdit.Author = text.Author;
            toEdit.Description = text.Description;
            toEdit.Title = text.Title;
            updateTags(text.Tags, text.ID, db, toEdit);
            db.SaveChanges();
        }
    }
}
