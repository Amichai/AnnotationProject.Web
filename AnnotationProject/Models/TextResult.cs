using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnotationProject.Models {
    public class TextResult {
        public string Title { get; set; }
        public string ContentNoHtml { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public int ID { get; set; }
        public string Tags { get; set; }
        public string Source { get; set; }
        public string Uploader { get; set; }
        public string Timestamp { get; set; }

        public int AnnotationCount { get; set; }
        public string Snippet { get; set; }

        public int? PrevText { get; set; }
        public int? NextText { get; set; }
        public bool IsBaseText { get; set; }
    }
}