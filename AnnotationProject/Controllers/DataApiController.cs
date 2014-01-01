﻿using AnnotationProject.Models;
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
            var annotationTexts = db.Texts.Where(i => !i.IsBaseText)
                .OrderBy(i => i.Timestamp).Take(10)
                .ToList();
            var annotationIDs = annotationTexts.Select( i=> i.ID);
            var annotations = db.Annotations.Where(i => annotationIDs.Contains(i.AnnotationTextID));
            return annotations.Select(i => new AnnotationResult() {
                Content = i.Text1.Content,
                Timestamp = i.Text.Timestamp,
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
            
            var toReturn = db.Texts.Where(i => annotationIds.Select(j => j.AnnotationTextID).Contains(i.ID));
            if (annotationIds.Count() == 0) {
                return new List<AnnotationResult>();
            }
            return toReturn.Select(i => new AnnotationResult() {
                Content = i.Content, 
                Timestamp = i.Timestamp,
                BaseTextID = textID,
                TextAnchor = annotationIds.Where(j => j.AnnotationTextID == i.ID).FirstOrDefault().TextAnchor,
            }).ToList();
        }

        [HttpPost]
        public List<AnnotationResult> PostAnnotation(AnnotationResult annotation) {
            var db = new TextAnnotationEntities();
            var newText = new Text() {
                Content = annotation.Content,
                Timestamp = DateTime.Now,
                IsBaseText = false,
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
    }
}
