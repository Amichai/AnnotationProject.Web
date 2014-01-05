using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnotationProject.Models {
    public class AnnotationResult {
        public int TextID { get; set; }
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public List<string> Tags { get; set; }
        public int BaseTextID { get; set; }
        public string BaseTextTitle { get; set; }
        public string TextAnchor { get; set; }
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

        public bool Expanded {
            get {
                return false;
            }
        }
    }
}