using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnotationProject.Models {
    public class TextResult {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public int ID { get; set; }
        public string Snippet {
            get {
                return string.Concat(this.Content.Take(100));
            }
        }
    }
}