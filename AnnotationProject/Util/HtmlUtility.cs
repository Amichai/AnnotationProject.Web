using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace AnnotationProject.Util {
    public class HtmlUtility {
        private static volatile HtmlUtility _instance;
        private static object _root = new object();
 
        private HtmlUtility() { }
 
        public static HtmlUtility Instance
        {
            get
            {
                if (_instance == null)
                    lock (_root)
                        if (_instance == null)
                            _instance = new HtmlUtility();
 
                return _instance;
            }
        }
 
        // Original list courtesy of Robert Beal :
        // http://www.robertbeal.com/
 
        private static readonly Dictionary<string, string[]> ValidHtmlTags =
            new Dictionary<string, string[]>
        {
            {"p", new string[]          { }},
            {"div", new string[]        { }},
            {"span", new string[]       {}},
            {"br", new string[]         {}},
            {"hr", new string[]         {}},
            {"label", new string[]      {}},
 
            {"h1", new string[]         {}},
            {"h2", new string[]         {}},
            {"h3", new string[]         {}},
            {"h4", new string[]         {}},
            {"h5", new string[]         {}},
            {"h6", new string[]         {}},
 
            {"font", new string[]       {}},
            {"strong", new string[]     {}},
            {"b", new string[]          {}},
            {"em", new string[]         {}},
            {"i", new string[]          {}},
            {"u", new string[]          {}},
            {"strike", new string[]     {}},
            {"ol", new string[]         {}},
            {"ul", new string[]         {}},
            {"li", new string[]         {}},
            {"blockquote", new string[] {}},
            {"code", new string[]       {}},
            {"pre", new string[]        {}},
 
            {"a", new string[]          { "href", "title"}},
            {"img", new string[]        { "src", "height", "width", "alt", "title" }},
 
            {"table", new string[]      {}},
            {"thead", new string[]      {}},
            {"tbody", new string[]      {}},
            {"tfoot", new string[]      {}},
            {"th", new string[]         {}},
            {"tr", new string[]         {}},
            {"td", new string[]         { }},
 
            {"q", new string[]          { }},
            {"cite", new string[]       {}},
            {"abbr", new string[]       {}},
            {"acronym", new string[]    {}},
            {"del", new string[]        {}},
            {"ins", new string[]        {}}
        };
 
        /// <summary>
        /// Takes raw HTML input and cleans against a whitelist
        /// </summary>
        /// <param name="source">Html source</param>
        /// <returns>Clean output</returns>
        public string SanitizeHtml(string source)
        {
            HtmlDocument html = GetHtml(source);
            
            if (html == null) return String.Empty;
 
            // All the nodes
            HtmlNode allNodes = html.DocumentNode;
 
            // Select whitelist tag names
            string[] whitelist = (from kv in ValidHtmlTags
                                  select kv.Key).ToArray();
 
            // Scrub tags not in whitelist
            CleanNodes(allNodes, whitelist);
 
            // Filter the attributes of the remaining
            foreach (KeyValuePair<string, string[]> tag in ValidHtmlTags)
            {
                IEnumerable<HtmlNode> nodes = (from n in allNodes.DescendantsAndSelf()
                                               where n.Name == tag.Key
                                               select n);
 
                // No nodes? Skip.
                if (nodes == null) continue;
 
                foreach (var n in nodes)
                {
                    // No attributes? Skip.
                    if (!n.HasAttributes) continue;
 
                    // Get all the allowed attributes for this tag
                    HtmlAttribute[] attr = n.Attributes.ToArray();
                    foreach (HtmlAttribute a in attr)
                    {
                        if (!tag.Value.Contains(a.Name))
                        {
                            a.Remove(); // Attribute wasn't in the whitelist
                        }
                        else
                        {
                            // *** New workaround. This wasn't necessary with the old library
                            if (a.Name == "href" || a.Name == "src") {
                                a.Value = (!string.IsNullOrEmpty(a.Value))? a.Value.Replace("\r", "").Replace("\n", "") : "";
                                a.Value =
                                    (!string.IsNullOrEmpty(a.Value) &&
                                    (a.Value.IndexOf("javascript") < 10 || a.Value.IndexOf("eval") < 10)) ?
                                    a.Value.Replace("javascript", "").Replace("eval", "") : a.Value;
                            }
                            else if (a.Name == "class" || a.Name == "style")
                            {
                                a.Value =
                                    Microsoft.Security.Application.Encoder.CssEncode(a.Value);
                            }
                            else
                            {
                                a.Value =
                                    Microsoft.Security.Application.Encoder.HtmlAttributeEncode(a.Value);
                            }
                        }
                    }
                }
            }
 
            // *** New workaround (DO NOTHING HAHAHA! Fingers crossed)
            return allNodes.InnerHtml;
 
            // *** Original code below
 
            /*
            // Anything we missed will get stripped out
            return
                Microsoft.Security.Application.Sanitizer.GetSafeHtmlFragment(allNodes.InnerHtml);
             */
        }
 
        /// <summary>
        /// Takes a raw source and removes all HTML tags
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public string StripHtml(string source)
        {
            source = SanitizeHtml(source);
 
            // No need to continue if we have no clean Html
            if (String.IsNullOrEmpty(source))
                return String.Empty;
 
            HtmlDocument html = GetHtml(source);
            StringBuilder result = new StringBuilder();
 
            // For each node, extract only the innerText
            foreach (HtmlNode node in html.DocumentNode.ChildNodes)
                result.Append(node.InnerText);
 
            return result.ToString();
        }
 
        /// <summary>
        /// Recursively delete nodes not in the whitelist
        /// </summary>
        private static void CleanNodes(HtmlNode node, string[] whitelist)
        {
            if (node.NodeType == HtmlNodeType.Element)
            {
                if (!whitelist.Contains(node.Name))
                {
                    node.ParentNode.RemoveChild(node);
                    return; // We're done
                }
            }
 
            if (node.HasChildNodes)
                CleanChildren(node, whitelist);
        }
 
        /// <summary>
        /// Apply CleanNodes to each of the child nodes
        /// </summary>
        private static void CleanChildren(HtmlNode parent, string[] whitelist)
        {
            for (int i = parent.ChildNodes.Count - 1; i >= 0; i--)
                CleanNodes(parent.ChildNodes[i], whitelist);
        }
 
        /// <summary>
        /// Helper function that returns an HTML document from text
        /// </summary>
        private static HtmlDocument GetHtml(string source)
        {
            HtmlDocument html = new HtmlDocument();
            html.OptionFixNestedTags = false;
            html.OptionAutoCloseOnEnd = false;
            html.OptionDefaultStreamEncoding = Encoding.UTF8;
 
            html.LoadHtml(source);
 
            // Encode any code blocks independently so they won't
            // be stripped out completely when we do a final cleanup
            foreach (var n in html.DocumentNode.DescendantNodesAndSelf())
            {
                if (n.Name == "code") {
                    //** Code tag attribute vulnerability fix 28-9-12 (thanks to Natd)
                    HtmlAttribute[] attr = n.Attributes.ToArray();
                    foreach (HtmlAttribute a in attr) {
                        if (a.Name != "style" && a.Name != "class")  { a.Remove(); }
                    } //** End fix
                    n.InnerHtml =
                        Microsoft.Security.Application.Encoder.HtmlEncode(n.InnerHtml);
                }
            }
 
            return html;
        }
    }
}