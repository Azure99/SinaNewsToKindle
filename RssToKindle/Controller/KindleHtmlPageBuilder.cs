﻿using System.Text;
using RssToKindle.Model;

namespace RssToKindle.Controller
{
    class KindleHtmlPageBuilder
    {
        private StringBuilder _index;
        private StringBuilder _body;
        private int _count;
        public KindleHtmlPageBuilder()
        {
            _index = new StringBuilder();
            _body = new StringBuilder();
            _count = 0;
        }

        /// <summary>
        /// 添加新闻
        /// </summary>
        /// <param name="newsBody"></param>
        public void AddNews(NewsBody newsBody)
        {
            int count = _count++;

            _index.AppendLine(string.Format("<div id=\"idiv{0}\">", count));
            _index.AppendLine(string.Format("<a href=\"#div{0}\">" +
                "<font size=\"5\">{1}</font>" +
                "</a>", count, newsBody.Title));
            _index.AppendLine("<p style=\"font-size:13px;\">" + newsBody.Description + "</p>");
            _index.AppendLine("</div>");
            _index.AppendLine("<br/>");

            _body.AppendLine(string.Format("<div id=\"div{0}\">", count));
            _body.AppendLine("<h1>" + newsBody.Title + "</h1>");

            //文章分类、当前第几篇、返回链接
            _body.Append("<p>" + newsBody.Class);
            _body.Append("&nbsp");
            _body.Append("第" + (count + 1) + "篇");
            _body.Append("&nbsp;&nbsp;");
            _body.Append(string.Format("<a href=\"#idiv{0}\">" +
                "<font size=\"5\">返回</font>" +
                "</a>", count));
            _body.AppendLine("</p>");

            _body.AppendLine(newsBody.Content);
            _body.AppendLine("<br/>");
            _body.AppendLine("<br/>");
            _body.AppendLine("<br/>");
            _body.AppendLine("<br/>");

            _body.AppendLine("</div>");
        }

        /// <summary>
        /// 生成HTML
        /// </summary>
        /// <returns></returns>
        public string GetHtml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<title>News</title>");
            sb.AppendLine("<meta charset=\"utf-8\"/>");
            sb.AppendLine("<body>");

            sb.Append(_index.ToString());
            sb.Append(_body.ToString());

            sb.AppendLine("</body>");
            sb.AppendLine("</head>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
        private class SingleClassBuilder
        {

        }
    }
}