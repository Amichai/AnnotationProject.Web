using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnotationProject.Models {
    public class AnnotationResult {
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public List<string> Tags { get; set; }
        public int BaseTextID { get; set; }
        public string TextAnchor { get; set; }
        public string PreviewText {
            get {
                return string.Concat(Content.Take(20));
            }
        }
        public bool Expanded {
            get {
                return false;
            }
        }
    }
}