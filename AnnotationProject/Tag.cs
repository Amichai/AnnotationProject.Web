//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AnnotationProject
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tag
    {
        public Tag()
        {
            this.TextTags = new HashSet<TextTag>();
        }
    
        public int ID { get; set; }
        public string Tag1 { get; set; }
    
        public virtual ICollection<TextTag> TextTags { get; set; }
    }
}
