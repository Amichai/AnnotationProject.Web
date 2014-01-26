using AnnotationProject.Models;
using AnnotationProject.Util;
using Microsoft.Security.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;

namespace AnnotationProject.Controllers {
    public class DataApiController : ApiController {
        private List<Text> combineResults(List<Text> a, List<Text> b, List<Text> c) {
            if (a == null && b == null && c == null) {
                return null;
            }
            if (a == null && b == null) {
                return c;
            }
            if (a == null && c == null) {
                return b;
            }
            if (c == null && b == null) {
                return a;
            }
            if (a == null) {
                return b.Intersect(c).ToList();
            }
            if (b == null) {
                return a.Intersect(c).ToList();
            }
            if (c == null) {
                return a.Intersect(b).ToList();
            }

            return a.Intersect(b).Intersect(c).ToList();
        }

        [HttpGet]
        public List<TextResult> GetText(string title, string tags, string author) {
            if (title == null && tags == null && author == null) {
                return GetAll();
            }

            var db = new TextAnnotationEntities();
            List<Text> tagQueryResult = null;
            List<Text> titleQueryResult = null;
            List<Text> authorQueryResult = null;
            if (tags != null) {
                List<Tag> inspectionTags = new List<Tag>();
                foreach (var tag in parseTagString(tags)) {
                    inspectionTags.AddRange(db.Tags.Where(i => i.Tag1.ToLower().Contains(tag.ToLower().Trim())));
                }
                tagQueryResult = inspectionTags.SelectMany(i => i.TextTags).Select(i => i.Text).Where(i => i.IsBaseText).ToList();
            }
            if (title != null) {
                titleQueryResult = db.Texts.Where(i => i.Title.ToLower().Contains(title.ToLower())).ToList();
            }
            if (author != null) {
                authorQueryResult = db.Texts.Where(i => i.Author.ToLower().Contains(author.ToLower())).ToList();
            }
            var result = combineResults(tagQueryResult, titleQueryResult, authorQueryResult);
            if (result == null) {
                return new List<TextResult>();
            } else {
                return toTextResult(result);
            }
        }

        [HttpGet]
        public List<TextResult> SearchTexts(string query) {
            if (query == null) {
                return GetAll();
            }

            var db = new TextAnnotationEntities();
            List<Text> tagQueryResult = null;
            List<Text> titleQueryResult = null;
            List<Text> authorQueryResult = null;
            List<Tag> inspectionTags = new List<Tag>();
            foreach (var tag in parseTagString(query)) {
                inspectionTags.AddRange(db.Tags.Where(i => i.Tag1.ToLower().Contains(tag.ToLower().Trim())));
            }
            tagQueryResult = inspectionTags.SelectMany(i => i.TextTags).Select(i => i.Text).Where(i => i.IsBaseText).ToList();

            titleQueryResult = db.Texts.Where(i => i.Title.ToLower().Contains(query.ToLower())).ToList();

            authorQueryResult = db.Texts.Where(i => i.Author.ToLower().Contains(query.ToLower())).ToList();

            var result = tagQueryResult.Union(titleQueryResult).Union(authorQueryResult).ToList();
            if (result == null) {
                return new List<TextResult>();
            } else {
                return toTextResult(result);
            }        
        }

        private static string getSafeHtml(string input) {
            return HtmlUtility.Instance.SanitizeHtml(m.Transform(input));
        }

        private static MarkdownSharp.Markdown m = new MarkdownSharp.Markdown(new MarkdownSharp.MarkdownOptions() { EncodeProblemUrlCharacters = false }); 

        private static List<TextResult> toTextResult(List<Text> queryResults) {
            var result = queryResults.Select(i => new TextResult() {
                ContentNoHtml = i.Content,
                Snippet = string.Concat(i.Content.Take(100)),
                Content = getSafeHtml(i.Content),
                Title = i.Title,
                Author = i.Author,
                Description = i.Description,
                ID = i.ID,
                Source = i.Source,
                Tags = getTextTags(i),
                AnnotationCount = i.AnnotationCount,
                NextText = i.NextTextID,
                PrevText = i.PrevTextID,
                Uploader = getUsername(i.UserID),
                Timestamp = i.Timestamp.ToString(),
                IsBaseText = i.IsBaseText
            }).ToList();

            return result;
        }

        [HttpGet]
        public List<TextResult> GetAll() {
            var db = new TextAnnotationEntities();
            return toTextResult(db.Texts.Where(i => i.IsBaseText).OrderByDescending(i => i.Timestamp).Take(50).ToList());
        }

        private int getUserID(string username) {
            var db = new UsersContext();
            return db.UserProfiles.Where(i => i.UserName == username).Single().UserId;
        }

        private static string getUsername(int userId) {
            var db = new UsersContext();
            var users = db.UserProfiles.Where(i => i.UserId == userId);
            if (users.Count() == 0) {
                return "";
            }
            return users.Single().UserName;
        }

        [HttpGet]
        public TextResult GetText(int id) {
            var db = new TextAnnotationEntities();
            var toReturn = db.Texts.Where(i => i.ID == id).ToList();
            return toTextResult(toReturn).Single();
        }

        [HttpGet]
        public List<AnnotationResult> RecentAnnotations() {
            var db = new TextAnnotationEntities();

            var annotationTexts = db.Texts.Where(i => !i.IsBaseText && !i.IsArchived)
                .OrderByDescending(i => i.Timestamp).Take(10)
                .ToList();
            var annotationIDs = annotationTexts.Select(i => i.ID);

            var annotations = db.Annotations.Where(i => annotationIDs.Contains(i.AnnotationTextID)
                );
            return toAnnotationResult(annotations.ToList()).OrderByDescending(i => i.Timestamp).ToList(); 
        }

        private static string getTextTags(int id, TextAnnotationEntities db) {
            return string.Concat(db.TextTags.Where(i => i.TextID == id).Select(i => i.Tag.Tag1 + ", ")).TrimEnd(new char[] { ' ', ',' });
        }

        [HttpGet]
        public List<AnnotationResult> GetAnnotations(int textID) {
            var db = new TextAnnotationEntities();
            var annotationIds = db.Annotations.Where(i => i.BaseTextID == textID).ToDictionary(
                i => i.AnnotationTextID, i => new { i.TextAnchor, i.ID });

            var annotationTextIds = annotationIds.Keys.ToList();
            var toReturn = db.Texts.Where(i => annotationTextIds.Contains(i.ID) &&
                !i.IsArchived).ToList();
            if (annotationIds.Count() == 0) {
                return new List<AnnotationResult>();
            }

            List<AnnotationResult> results = new List<AnnotationResult>();
            foreach (var t in toReturn) {
                string tags = getTextTags(t.ID, db);
                var user = Membership.GetUser();
                bool userFavorited = false;
                if (user != null) {
                    var userID = (int)user.ProviderUserKey;
                    var annID = annotationIds[t.ID].ID;
                    userFavorited = db.UserLikes.Any(i => i.AnnotationID == annID && i.UserID == userID);
                
                }
                results.Add(new AnnotationResult() {
                    PreviewText = string.Concat(t.Content.Take(100)),
                    ContentNoHtml = t.Content,
                    Ellipse = t.Content.Count() > 100,
                    Content = getSafeHtml(t.Content),
                    Timestamp = t.Timestamp,
                    BaseTextID = textID,
                    TextAnchor = annotationIds[t.ID].TextAnchor,
                    TextID = t.ID,
                    Username = getUsername(t.UserID),
                    Tags = tags,
                    UserFavorited = userFavorited,
                    AnnotationID = annotationIds[t.ID].ID,
                    Source = t.Source,
                    CommentCount = t.AnnotationCount,
                    FavoriteCount = t.FavoritedCount,
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
                UserID = (int)Membership.GetUser().ProviderUserKey,
                Source = annotation.Source
            };
            db.Texts.Add(newText);
            db.SaveChanges();
            var newAnnotation = new Annotation() {
                AnnotationTextID = newText.ID,
                BaseTextID = annotation.BaseTextID,
                TextAnchor = annotation.TextAnchor
            };
            db.Annotations.Add(newAnnotation);
            db.SaveChanges();
            db.Texts.Where(i => i.ID == annotation.BaseTextID).Single().AnnotationCount++;
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
            var tagList = parseTagString(tagString);
            foreach (var t in tagList) {
                var tag = t.Trim().ToLower();
                Tag inspectionTag;
                var matches = db.Tags.Where(i => i.Tag1 == tag);
                if (matches.Count() == 0) {
                    inspectionTag = addNewTag(db, tag);
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

        private static Tag addNewTag(TextAnnotationEntities db, string tag) {
            Tag inspectionTag;
            inspectionTag = new Tag() {
                Tag1 = tag.ToLower(),
            };
            db.Tags.Add(inspectionTag);
            db.SaveChanges();
            return inspectionTag;
        }

        private static string[] parseTagString(string tagString) {
            var tagList = tagString.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return tagList;
        }

        /// <returns>the textID of the newly uploaded text</returns>
        [HttpPost]
        public int PostText(TextResult text) {
            var db = new TextAnnotationEntities();
            var newText = new Text() {
                Title = text.Title,
                Content = text.Content,
                //Content = text.Content,
                Timestamp = DateTime.Now,
                Author = text.Author,
                Description = text.Description,
                IsBaseText = true,
                Source = text.Source,
                UserID = (int)Membership.GetUser().ProviderUserKey
            };
            db.Texts.Add(newText);
            string[] tagList = null;
            if (text.Tags != null) {
                tagList = parseTagString(text.Tags);
            }
            foreach (var t in tagList) {
                var tag = t.Trim().ToLower();
                Tag inspectionTag;
                var matches = db.Tags.Where(i => i.Tag1 == tag);
                if (matches.Count() == 0) {
                    inspectionTag = addNewTag(db, tag);
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
            return newText.ID;
        }

        [HttpGet]
        public List<SiteUsersModel> GetSiteUsers() {
            var udb = new UsersContext();
            List<SiteUsersModel> users = new List<SiteUsersModel>();
            var db = new TextAnnotationEntities();
            foreach (var u in udb.UserProfiles) {
                var likes = db.UserLikes.Where(i => i.Annotation.Text.UserID == u.UserId).Count();

                var annotations = db.Annotations.Where(i => i.Text1.UserID == u.UserId);
                var texts = annotations.ToList().Select(i => i.Text.ID.ToString() + "." + i.Text.Title).Distinct().Where(i => i.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries).Count() > 1).ToList();
                
                users.Add(new SiteUsersModel() {
                    Username = u.UserName,
                    Favorited = likes,
                    Annotations = annotations.Count(),
                    Texts = texts,
                });
            }
            return users.OrderByDescending(i => i.Favorited).ToList();
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
        public List<UserAdminModel> DeleteUser(string user) {
            var roles = Roles.GetRolesForUser(user);
            foreach (var r in roles) {
                Roles.RemoveUserFromRole(user, r);
            }
            Membership.DeleteUser(user);
            return GetUsers();
        }

        [HttpPost]
        public List<AnnotationResult> ArchiveAnnotation(int annotationID, int textID) {
            var db = new TextAnnotationEntities();

            var t = db.Texts.Where(i => i.ID == textID).Single();
            t.AnnotationCount--;
            db.SaveChanges();

            db.Texts.Where(i => i.ID == annotationID).Single().IsArchived = true;
            db.SaveChanges();
            return GetAnnotations(textID);
        }

        [HttpPost]
        public List<AnnotationResult> UpdateAnnotation(AnnotationResult ann) {
            var db = new TextAnnotationEntities();
            var toEdit = db.Texts.Where(i => i.ID == ann.TextID).Single();
            toEdit.Content = ann.ContentNoHtml;
            updateTags(ann.Tags, ann.TextID, db, toEdit);
            db.SaveChanges();
            return GetAnnotations(ann.BaseTextID);
        }

        [HttpPost]
        public void UpdateTextDetails(TextResult text) {
            var db = new TextAnnotationEntities();
            var toEdit = db.Texts.Where(i => i.ID == text.ID).Single();
            toEdit.Content = text.ContentNoHtml;
            toEdit.Author = text.Author;
            toEdit.Description = text.Description;
            toEdit.Title = text.Title;
            toEdit.Source = text.Source;
            toEdit.NextTextID = text.NextText;
            toEdit.PrevTextID = text.PrevText;
            updateTags(text.Tags, text.ID, db, toEdit);
            db.SaveChanges();
        }

        [HttpGet]
        public List<AnnotationResult> GetUserAnnotations(string username) {
            if (@User.Identity.Name == "") {
                return new List<AnnotationResult>();
            }
            var db = new TextAnnotationEntities();
            var userId = getUserID(username);
            var annotationTexts = db.Texts.Where(i => !i.IsBaseText &&
                i.UserID == userId &&
                !i.IsArchived)
                .OrderByDescending(i => i.Timestamp)
                .ToList();
            var annotationIDs = annotationTexts.Select(i => i.ID);

            var annotations = db.Annotations.Where(i => annotationIDs.Contains(i.AnnotationTextID)
                );
            return toAnnotationResult(annotations).OrderByDescending(i => i.Timestamp).ToList();
        }

        private static string getTextTags(Text i) {
            return string.Concat(i.TextTags.Select(j => j.Tag.Tag1 + ", ")).TrimEnd(new char[] { ' ', ',' });
        }

        [HttpGet]
        public List<TextResult> GetUserTexts(string username) {
            if (@User.Identity.Name == "") {
                return new List<TextResult>();
            }
            var db = new TextAnnotationEntities();
            var userId = getUserID(username);
            return toTextResult(db.Texts.Where(i => i.IsBaseText && i.UserID == userId).ToList());
        }

        [HttpPost]
        public void FavoriteAnnotation(int annotationID) {
            var db = new TextAnnotationEntities();
            var userID = (int)Membership.GetUser().ProviderUserKey;
            var existing = db.UserLikes.Where(i => i.UserID == userID && i.AnnotationID == annotationID);
            if (existing.Count() > 0) {
                foreach (var e in existing) {
                    e.Annotation.Text1.FavoritedCount--;
                    db.UserLikes.Remove(e);
                }
            } else {
                db.UserLikes.Add(new UserLike() {
                    UserID = userID,
                    AnnotationID = annotationID
                });
                var ann = db.Annotations.Where(i => i.ID == annotationID).Single();
                ann.Text1.FavoritedCount++;
            }
            db.SaveChanges();
        }

        private List<AnnotationResult> toAnnotationResult(IEnumerable<Annotation> annotations) {
            return annotations.ToList().Select(i =>
                new AnnotationResult() {
                    PreviewText = string.Concat(i.Text1.Content.Take(100)),
                    Ellipse = i.Text1.Content.Count() > 100,
                    Content = getSafeHtml(i.Text1.Content),
                    Timestamp = i.Text1.Timestamp,
                    BaseTextID = i.Text.ID,
                    TextAnchor = i.TextAnchor,
                    TextID = i.Text1.ID,
                    Username = getUsername(i.Text1.UserID),
                    AnnotationID = i.ID,
                    BaseTextTitle = i.Text.Title,
                    Source = i.Text1.Source,
                }).ToList();
        }

        [HttpGet]
        public List<AnnotationResult> GetFavoriteAnnotations(string username) {
            var db = new TextAnnotationEntities();
            if (@User.Identity.Name == "") {
                return new List<AnnotationResult>();
            }
            var userId = getUserID(username);
            var annotationTexts = db.Texts.Where(i => i.UserID == userId && !i.IsBaseText);
            var results = toAnnotationResult(db.UserLikes.Where(i => i.UserID == userId).Select(i => i.Annotation));
            return results;
        }

        [HttpGet]
        public List<TextResult> GetArchivedTexts() {
            TextAnnotationEntities db = new TextAnnotationEntities();
            return toTextResult(db.Texts.Where(i => i.IsArchived).ToList());
        }

        [HttpPost]
        public List<TextResult> RestoreText(int id) {
            TextAnnotationEntities db = new TextAnnotationEntities();
            var t = db.Texts.Where(i => i.ID == id).Single();
            var ann = db.Annotations.Where(i => i.AnnotationTextID == id).FirstOrDefault();
            db.Texts.Where(i => i.ID == ann.BaseTextID).Single().AnnotationCount++;
            t.IsArchived = false;
            db.SaveChanges();
            return GetArchivedTexts();
        }

        [HttpGet]
        public List<AnnotationResult> GetNotifications() {
            if (@User.Identity.Name == "") {
                return new List<AnnotationResult>();
            }
            var db = new TextAnnotationEntities();
            var id = getUserID(@User.Identity.Name);
            var annotations = db.Annotations.Where(i => i.Text.UserID == id && !i.Text.IsArchived && !i.Text1.IsArchived).OrderByDescending(i => i.Text1.Timestamp).Take(10);
            return toAnnotationResult(annotations);
        }


    }
}
/*
archived texts view (with delete and restore)
Filter and search annotations on tags/users
recently uploaded?
Tag pages
 * Left align hebrew text titles
*/