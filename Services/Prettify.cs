using AngleSharp.Css.Parser;
using AngleSharp.Css;
using AngleSharp.Html.Parser;
using AngleSharp.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Css.Dom;

namespace LinkListCreator.Services
{
    internal static class Prettify
    {
        /// <summary>
        /// Prettify the HTML string
        /// </summary>
        /// <param name="html">html string to prettify</param>
        /// <returns>prittified html string</returns>
        public static string PrettifyHtml(string html)
        {
            HtmlParser parser = new();
            IHtmlDocument document = parser.ParseDocument(html);
            StringWriter sw = new();
            document.ToHtml(sw, new PrettyMarkupFormatter());
            return sw.ToString();
        }

        /// <summary>
        /// Prettify the CSS string
        /// </summary>
        /// <param name="css">css string to prettify</param>
        /// <returns>prettified html string</returns>
        public static string PrettifyCss(string css)
        {
            CssParser parser = new();
            ICssStyleSheet stylesheet = parser.ParseStyleSheet(css);
            StringWriter sw = new();
            stylesheet.ToCss(sw, new PrettyStyleFormatter());
            return sw.ToString();
        }
    }
}
