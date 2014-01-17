using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnotationProject.Models {
    public class AnnotationResult {
        public int AnnotationID { get; set; }
        public int TextID { get; set; }
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string Tags { get; set; }
        public int BaseTextID { get; set; }
        public string BaseTextTitle { get; set; }
        public string TextAnchor { get; set; }
        public string Source { get; set; }
        public int CommentCount { get; set; }

        public bool EditMode {
            get {
                return false;
            }
        }
        public string PreviewText {
            get {
                return string.Concat(Content.Take(200));
            }
        }
        public string DateString {
            get {
                return this.Timestamp.ToShortDateString();
            }
        }

        public bool UserFavorited { get; set; }

        public bool Expanded {
            get {
                return false;
            }
        }

        public int FavoriteCount { get; set; }
    }
}